//#define SRSL_VM_DEBUG_TRACE_EXECUTION
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using MemoizeSharp.Ast;
using Srsl_Parser.SymbolTable;
using Type = System.Type;

namespace Srsl_Parser.Runtime
{
[StructLayout(LayoutKind.Explicit)]
struct IntByteStruct
{
    [FieldOffset(0)]
    public byte byte0;
    [FieldOffset(1)]
    public byte byte1;
    [FieldOffset(2)]
    public byte byte2;
    [FieldOffset(3)]
    public byte byte3;

    [FieldOffset(0)]
    public int integer;

}

public class SrslVm
{
    private BinaryChunk m_CurrentChunk;
    private int m_CurrentInstructionPointer;
    private Stack<DynamicSrslVariable> m_VmStack;
    private DynamicSrslVariable[] m_VmTosCache;
    private Dictionary <string, BinaryChunk > m_CompiledChunks;
    //private BufferPool < object > m_ObjectBufferPool;
    
    private List < DynamicSrslVariable > m_FunctionArguments = new List < DynamicSrslVariable >();
    private ObjectPoolFastMemory < FastMemorySpace > m_PoolFastMemoryFastMemory;
    //private ObjectPool < FastCallMemorySpace > m_PoolFastCallMemory;
    private FastGlobalMemorySpace m_GlobalMemorySpace;
    // private FastMemorySpace m_CurrentCallMemorySpace;
    private Scope m_CurrentScope;
    private FastMemorySpace m_CurrentMemorySpace;
    private FastMemorySpace m_LastMemorySpaceDebug;
    //private Stack < FastMemorySpace > m_TempMemoryStack = new Stack < FastMemorySpace >();
    private Stack < FastMemorySpace > m_CallStack = new Stack < FastMemorySpace >();
    private Dictionary < string, FastMethodInfo > CachedMethods = new Dictionary < string, FastMethodInfo >();
    private int m_LastGetLocalVarId = -1;
    private int m_LastGetLocalVarModuleId  = -1;
    private int m_LastGetLocalVarDepth = -1;
    private int m_LastGetLocalClassId = -1;
    private bool m_SetMember = false;
    private bool m_KeepLastItemOnStackToReturn = false;
    private SrslVmOpCodes m_CurrentByteCodeInstruction = SrslVmOpCodes.OpNone;
    private int m_CurrentByteCodeInstructionDataPointer = 0;
    public Dictionary < string, BinaryChunk > CompiledChunks => m_CompiledChunks;

    protected void InitMemorySpaces()
    {
        m_CurrentMemorySpace = m_GlobalMemorySpace;
        string moduleName = "System";
        FastMemorySpace callSpace = new FastMemorySpace( "", m_GlobalMemorySpace, 0, m_CurrentChunk, m_CurrentInstructionPointer, 4 );
        //m_TempMemoryStack.Push( callSpace );
        m_GlobalMemorySpace.Modules.Add( callSpace );
        callSpace.Define(
            DynamicVariableExtension.ToDynamicVariable(new SrslChunkWrapper( new BinaryChunk())), "System.Object" );
        
        callSpace.Define(
            DynamicVariableExtension.ToDynamicVariable(new PrintFunctionVm()), "System.Print" );
        callSpace.Define(
            DynamicVariableExtension.ToDynamicVariable(new ForeignLibraryInterfaceVm()),"System.CSharpInterfaceCall" );
        
        callSpace.Define(
            DynamicVariableExtension.ToDynamicVariable(new SrslChunkWrapper( m_CompiledChunks["System.CSharpInterface"])), "System.CSharpInterface" );
        
        //TempMemoryStack.Push(m_GlobalMemorySpace);
    }
    
    public SrslVmInterpretResult Interpret( BinaryChunk mainChunk, Dictionary <string, BinaryChunk > compiledChunks, SymbolTableBuilder symbolTableBuilder )
    {
        m_VmStack = new Stack<DynamicSrslVariable>();
        m_CurrentChunk = mainChunk;
        m_CompiledChunks = compiledChunks;
        m_CurrentInstructionPointer = 0;
        m_CurrentScope = symbolTableBuilder.CurrentScope;
        m_GlobalMemorySpace = new FastGlobalMemorySpace((symbolTableBuilder.CurrentScope as BaseScope).NumberOfSymbols);
        //m_TempMemoryStack = new Stack < FastMemorySpace >();
        m_CallStack = new Stack < FastMemorySpace >();
        m_VmTosCache = new DynamicSrslVariable[2];
        m_PoolFastMemoryFastMemory = new ObjectPoolFastMemory<FastMemorySpace>(() => new FastMemorySpace("", null, 0, null, 0,0), 150);
        InitMemorySpaces();
        //m_PoolFastCallMemory = new ObjectPool<FastCallMemorySpace>(() => new FastCallMemorySpace("", null, 0, null, 0), 100);
        return Run();
    }
    
    private SrslVmOpCodes ReadInstruction()
    {
        m_CurrentByteCodeInstruction = (SrslVmOpCodes)m_CurrentChunk.Code[m_CurrentInstructionPointer];
        m_CurrentInstructionPointer++;
        m_CurrentByteCodeInstructionDataPointer = 0;
        return m_CurrentByteCodeInstruction;
    }

    private ConstantValue ReadConstant()
    {
        ConstantValue instruction =
            m_CurrentChunk.Constants[m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24)];
        m_CurrentInstructionPointer += 4;
        m_CurrentByteCodeInstructionDataPointer++;
        return instruction;
    }
    
    private SrslVmInterpretResult Run()
    {
        
        while ( true )
        {
            if ( m_CurrentInstructionPointer < m_CurrentChunk.Code.Length )
            {
#if SRSL_VM_DEBUG_TRACE_EXECUTION

               /* if ( m_LastMemorySpaceDebug != m_CurrentMemorySpace )
                {
                    Console.WriteLine("New Memory Space: {0}", m_CurrentMemorySpace.Name);
                    m_LastMemorySpaceDebug = m_CurrentMemorySpace;
                }
                
                Console.Write("Stack:   ");
                foreach( object slot in m_VmStack )
                {
                    Console.Write("[" + slot + "]");
                }
                Console.Write("\n");*/
                
                m_CurrentChunk.DissassembleInstruction( m_CurrentInstructionPointer );
#endif
                
                SrslVmOpCodes instruction = ReadInstruction();
                switch (instruction) 
                {
                    case SrslVmOpCodes.OpNone:
                    {
                        break;
                    }
                    case SrslVmOpCodes.OpPopStack:
                    {
                        m_VmStack.Pop();
                        break;
                    }
                    case SrslVmOpCodes.OpDefineModule:
                    {
                        string moduleName = ReadConstant().StringConstantValue;
                        int depth = 0;
                        m_CurrentScope = (BaseScope)m_CurrentScope.resolve( moduleName, out int moduleId, ref depth );
                        int numberOfMembers = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        /*FastMemorySpace moduleSpace = m_PoolFastMemory.Get();
                        moduleSpace.m_EnclosingSpace = m_GlobalMemorySpace;
                        moduleSpace.Name = m_GlobalMemorySpace.Name + moduleName;*/
                        
                        //FastMemorySpace s = moduleSpace;

                        FastModuleMemorySpace callSpace = new FastModuleMemorySpace( "", m_GlobalMemorySpace, m_VmStack.Count, m_CurrentChunk, m_CurrentInstructionPointer, numberOfMembers );
                        //m_TempMemoryStack.Push( callSpace );
                        m_GlobalMemorySpace.Modules.Add( callSpace );
                        m_CurrentChunk = CompiledChunks[moduleName];
                        m_CurrentInstructionPointer = 0;
                        m_CurrentMemorySpace = callSpace;
                        m_CallStack.Push( callSpace );
                        break;
                    }
                    case SrslVmOpCodes.OpImportModule:
                    {
                        /*dynamic constant = ReadConstant();
                        m_ImportedModules.Add( constant );*/
                        break;
                    }
                    case SrslVmOpCodes.OpDefineClass:
                    {
                        string className = ReadConstant().StringConstantValue;
                        SrslChunkWrapper chunkWrapper = new SrslChunkWrapper(
                            CompiledChunks[className] );
                        
                        m_CurrentMemorySpace.Define(
                            DynamicVariableExtension.ToDynamicVariable(chunkWrapper));
                        break;
                    }
                    case SrslVmOpCodes.OpDefineMethod:
                    {
                        string methodName = ReadConstant().StringConstantValue;
                        SrslChunkWrapper chunkWrapper = new SrslChunkWrapper(
                            CompiledChunks[methodName] );
                        
                        m_CurrentMemorySpace.Define(
                            DynamicVariableExtension.ToDynamicVariable(chunkWrapper), methodName);

                        break;
                    }
                    case SrslVmOpCodes.OpBindToFunction:
                    {
                        int numberOfArguments = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_FunctionArguments.Clear();
                        for ( int i = 0; i < numberOfArguments; i++ )
                        {
                            m_FunctionArguments.Add( m_VmStack.Pop() );
                        }
                        
                       
                        break;
                    }
                    case SrslVmOpCodes.OpCallFunction:
                    {
                        string method = ReadConstant().StringConstantValue;
                        DynamicSrslVariable call = m_CurrentMemorySpace.Get( method );

                        if ( call.ObjectData is SrslChunkWrapper function )
                        {
                            /* FastMemorySpace memorySpace = m_PoolFastMemory.Get();
                             memorySpace.m_EnclosingSpace = m_TempMemoryStack.Peek();
                             memorySpace.StackCountAtBegin = m_VmStack.Count;
                             memorySpace.Name = m_TempMemoryStack.Peek().Name + method;*/
                            FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();
                            callSpace.ResetPropertiesArray( m_FunctionArguments.Count );
                            callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                            //callSpace.Name = m_CallStack.Peek().Name + method;
                            callSpace.CallerChunk = m_CurrentChunk;
                            callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                            callSpace.StackCountAtBegin = m_VmStack.Count;
                            //m_TempMemoryStack.Push( callSpace );
                            m_CurrentMemorySpace = callSpace;
                            m_CallStack.Push( callSpace );

                            for ( int i = 0; i < m_FunctionArguments.Count; i++ )
                            {
                                m_CurrentMemorySpace.Define( m_FunctionArguments[i] );
                            }
                            m_CurrentChunk = function.ChunkToWrap;
                            m_CurrentInstructionPointer = 0;
                        }
                        if ( call.ObjectData is ISrslVmCallable callable )
                        {
                            object returnVal = callable.Call( m_FunctionArguments );

                            if ( returnVal != null )
                            {
                                m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(returnVal) );
                            }
                        }
                        
                        break;
                    }
                    case SrslVmOpCodes.OpCallMemberFunction:
                    {
                        ConstantValue constant = ReadConstant(); 
                        if ( m_VmStack.Peek().ObjectData is StaticWrapper wrapper )
                        {
                            m_VmStack.Pop();
                            string methodName = constant.StringConstantValue;
                            object[] functionArguments = new object[m_FunctionArguments.Count];
                            Type[] functionArgumentTypes = new Type[m_FunctionArguments.Count];
                            int it = 0;
                            m_FunctionArguments.Reverse();
                            for ( int i = 0; i < m_FunctionArguments.Count; i++ )
                            {
                                functionArguments[it] = m_FunctionArguments[i].ToObject();
                                functionArgumentTypes[it] = m_FunctionArguments[i].GetType();
                                it++;
                            }
                            object returnVal = wrapper.InvokeMember( methodName, functionArguments, functionArgumentTypes );
                            if ( returnVal != null )
                            {
                                m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(returnVal) );
                            }
                        }
                        else if ( m_VmStack.Peek().ObjectData is FastMemorySpace fastMemorySpace )
                        {
                            m_VmStack.Pop();
                            
                            string methodName = constant.StringConstantValue;
                            DynamicSrslVariable call = fastMemorySpace.Get( methodName );
                            
                            m_CurrentMemorySpace = fastMemorySpace;
                            if ( call.ObjectData != null )
                            {
                                if ( call.ObjectData is SrslChunkWrapper function )
                                {
                                    /*FastMemorySpace memorySpace = m_PoolFastMemory.Get();
                                    memorySpace.m_EnclosingSpace = m_TempMemoryStack.Peek();
                                    memorySpace.StackCountAtBegin = m_VmStack.Count;
                                    memorySpace.Name = m_TempMemoryStack.Peek() + methodName;*/
                                    FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();
                                    callSpace.ResetPropertiesArray( m_FunctionArguments.Count );
                                    callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                                    callSpace.CallerChunk = m_CurrentChunk;
                                    callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                                    //callSpace.Name = m_CurrentMemorySpace.Name + methodName;
                                    callSpace.StackCountAtBegin = m_VmStack.Count;
                                    m_CurrentMemorySpace = callSpace;
                                    m_CallStack.Push( callSpace );
                                    foreach ( var functionArgument in m_FunctionArguments )
                                    {
                                        m_CurrentMemorySpace.Define( functionArgument );
                                    }
                                    m_CurrentChunk = function.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;
                                }
                                if ( call.ObjectData is ISrslVmCallable callable )
                                {
                                    object returnVal = callable.Call( m_FunctionArguments );

                                    if ( returnVal != null )
                                    {
                                        m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(returnVal) );
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception( "Error expected Function, got null!" );
                            }
                        }
                        else if ( m_VmStack.Peek().ObjectData is object obj )
                        {
                            m_VmStack.Pop();
                            string callString = obj + "." + constant.StringConstantValue;
                            if ( CachedMethods.ContainsKey( callString ) )
                            {
                                object[] functionArguments = new object[m_FunctionArguments.Count];
                                //Type[] functionArgumentTypes = new Type[m_FunctionArguments.Count];
                                int it = 0;
                                foreach ( var functionArgument in m_FunctionArguments )
                                {
                                    functionArguments[it] = functionArgument.ToObject();
                                  //  functionArgumentTypes[it] = functionArgument.GetType();
                                    it++;
                                }
                                object returnVal = CachedMethods[callString].Invoke( m_VmStack.Pop().ObjectData, functionArguments );
                                if ( returnVal != null )
                                {
                                    m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(returnVal) );
                                }
                            }
                            else
                            {
                                Type type = obj.GetType();
                                object[] functionArguments = new object[m_FunctionArguments.Count];
                                Type[] functionArgumentTypes = new Type[m_FunctionArguments.Count];
                                int it = 0;
                                foreach ( var functionArgument in m_FunctionArguments )
                                {
                                    functionArguments[it] = functionArgument.ToObject();
                                    functionArgumentTypes[it] = functionArgument.GetType();
                                    it++;
                                }
                                MethodInfo method = type.GetMethod(constant.StringConstantValue, functionArgumentTypes);
                                if ( method != null )
                                {
                                    FastMethodInfo fastMethodInfo = new FastMethodInfo( method );
                                    CachedMethods.Add( callString, fastMethodInfo );
                                    object returnVal = fastMethodInfo.Invoke( m_VmStack.Pop().ObjectData, functionArguments );
                                    if ( returnVal != null )
                                    {
                                        m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(returnVal) );
                                    }
                                }
                                else
                                {
                                    throw new Exception( "Error Function "+ constant.StringConstantValue + " not found!" );
                                }
                            }
                                
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpDefineLocalInstance:
                    {
                        int moduleIdClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int depthClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int idClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int classMemberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        
                        if ( m_CurrentMemorySpace.Get( moduleIdClass, depthClass, -1, idClass ).ObjectData is SrslChunkWrapper classWrapper )
                        {
                            FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace( "", m_CurrentMemorySpace, m_VmStack.Count, m_CurrentChunk, m_CurrentInstructionPointer, classMemberCount );
                            m_CurrentMemorySpace.Define( DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace)  );
                            m_CurrentMemorySpace = classInstanceMemorySpace;
                            m_CurrentChunk = classWrapper.ChunkToWrap;
                            m_CurrentInstructionPointer = 0;
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace) );
                            m_CallStack.Push( m_CurrentMemorySpace );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpDefineLocalVar:
                    {
                        m_CurrentMemorySpace.Define( m_VmStack.Pop() );
                        break;
                    }
                    case SrslVmOpCodes.OpDeclareLocalVar:
                    {
                        m_CurrentMemorySpace.Define( DynamicVariableExtension.ToDynamicVariable() );
                        break;
                    }
                    case SrslVmOpCodes.OpSetLocalInstance:
                    {
                        int moduleIdLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int depthLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int idLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        
                        int moduleIdClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int depthClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int idClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int classMemberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        if ( m_CurrentMemorySpace.Get( moduleIdClass, depthClass, -1, idClass ).ObjectData is SrslChunkWrapper classWrapper )
                        {
                            FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace( "", m_CurrentMemorySpace, m_VmStack.Count, m_CurrentChunk, m_CurrentInstructionPointer, classMemberCount );
                            classInstanceMemorySpace.m_EnclosingSpace = m_CurrentMemorySpace;
                            //classInstanceMemorySpace.Name = m_CurrentMemorySpace.Name + "ClassInstance";
                            classInstanceMemorySpace.CallerChunk = m_CurrentChunk;
                            classInstanceMemorySpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                            classInstanceMemorySpace.StackCountAtBegin = m_VmStack.Count;
                            m_CurrentMemorySpace.Put( moduleIdLocalInstance, depthLocalInstance, -1, idLocalInstance, DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace)  );
                            m_CurrentMemorySpace = classInstanceMemorySpace;
                            m_CurrentChunk = classWrapper.ChunkToWrap;
                            m_CurrentInstructionPointer = 0;
                            
                            m_CallStack.Push( classInstanceMemorySpace );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpGetLocalVar:
                    {
                        m_LastGetLocalVarModuleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_LastGetLocalVarDepth = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_LastGetLocalClassId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_LastGetLocalVarId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_VmStack.Push( m_CurrentMemorySpace.Get( m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId ) );
                        break;
                    }
                    case SrslVmOpCodes.OpGetModule:
                    {
                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        FastMemorySpace obj = m_GlobalMemorySpace.Modules[id];
                        m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(obj) );
                        break;
                    }
                    case SrslVmOpCodes.OpGetMember:
                    {
                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        FastMemorySpace obj = (FastMemorySpace)m_VmStack.Pop().ObjectData;
                        m_VmStack.Push( obj.Get( -1, 0, -1, id ) );
                        break;
                    }
                    case SrslVmOpCodes.OpSetMember:
                    {
                        //m_SetMemberPopCount = (int)ReadConstant();
                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_SetMember = true;
                        m_LastGetLocalVarId = id;
                        break;
                    }
                    case SrslVmOpCodes.OpSetLocalVar:
                    {
                        int moduleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int depth = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int classId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_LastGetLocalVarId = id;
                        m_LastGetLocalVarModuleId = moduleId;
                        m_LastGetLocalVarDepth = depth;
                        m_LastGetLocalClassId = classId;
                        m_VmStack.Push( m_CurrentMemorySpace.Get( moduleId, depth, classId, id ) );
                        break;
                    }
                    case SrslVmOpCodes.OpConstant:
                    {
                        ConstantValue constantValue = ReadConstant();

                        switch ( constantValue.ConstantType )
                        {
                            case ConstantValueType.Integer:
                                m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( constantValue.IntegerConstantValue ) );
                                break;

                            case ConstantValueType.Double:
                                m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(constantValue.DoubleConstantValue) );
                                break;

                            case ConstantValueType.String:
                                m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(constantValue.StringConstantValue) );
                                break;

                            case ConstantValueType.Bool:
                                m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(constantValue.BoolConstantValue) );
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        //;
                        break;
                    }
                    case SrslVmOpCodes.OpWhileLoop:
                    {
                        int jumpCodeHeaderStart= m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int jumpCodeBodyEnd = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;;

                        if (  m_VmStack.Pop().DynamicType == DynamicVariableType.True )
                        {
                            //DynamicVariableExtension.ReturnDynamicSrslVariable(  );
                            m_CurrentChunk.Code[jumpCodeBodyEnd] = (byte)SrslVmOpCodes.OpJump;
                            IntByteStruct intByteStruct = new IntByteStruct();
                            intByteStruct.integer = jumpCodeHeaderStart;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 1] = intByteStruct.byte0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 2] = intByteStruct.byte1;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 3] = intByteStruct.byte2;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 4] = intByteStruct.byte3;
                        }
                        else
                        {
                            //DynamicVariableExtension.ReturnDynamicSrslVariable(  );
                            m_CurrentInstructionPointer = jumpCodeBodyEnd + 10;
                            m_CurrentChunk.Code[jumpCodeBodyEnd] = 0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 1] = 0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 2] = 0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 3] = 0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 4] = 0;
                            
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 5] = 0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 6] = 0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 7] = 0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 8] = 0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 9] = 0;
                            
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpJump:
                    {
                        int jumpCode = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_CurrentInstructionPointer = jumpCode;
                        break;
                    }
                    case SrslVmOpCodes.OpAssign:
                    {
                        SrslVmOpCodes nextOpCode = ReadInstruction();
                        int moduleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int depth = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int classId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        m_LastGetLocalVarId = id;
                        m_LastGetLocalVarModuleId = moduleId;
                        m_LastGetLocalVarDepth = depth;
                        m_LastGetLocalClassId = classId;

                        if ( nextOpCode == SrslVmOpCodes.OpSetLocalVar )
                        {
                            DynamicSrslVariable value = m_VmStack.Pop();
                            m_CurrentMemorySpace.Put( m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, value );
                        }
                        else
                        {
                            FastMemorySpace fastMemorySpace = (FastMemorySpace)m_CurrentMemorySpace.Get( moduleId, depth, classId, id ).ObjectData;

                            if ( fastMemorySpace.Exist( -1, 0, -1, m_LastGetLocalVarId ) )
                            {
                                fastMemorySpace.Put( -1, 0, -1, m_LastGetLocalVarId, m_VmStack.Pop() );
                            }
                            else
                            {
                                fastMemorySpace.Define( m_VmStack.Pop() );
                            }
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpNegate:
                    {
                        DynamicSrslVariable value = m_VmStack.Pop();

                        if ( value.DynamicType < DynamicVariableType.True )
                        {
                            value.NumberData =  value.NumberData *= -1;
                        }
                        else
                        {
                            throw new Exception( "Can only negate Integers and Floating Point Numbers!" );
                        }
                        
                        m_VmStack.Push( value );
                        m_CurrentMemorySpace.Put( m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, value );
                        break;
                    }
                    case SrslVmOpCodes.OpAffirm:
                    {
                        DynamicSrslVariable value = m_VmStack.Pop();

                        if ( value.DynamicType < DynamicVariableType.True )
                        {
                            value.NumberData =  value.NumberData *= 1;
                        }
                        else
                        {
                            throw new Exception( "Can only negate Integers and Floating Point Numbers!" );
                        }
                        
                        m_VmStack.Push( value );
                        m_CurrentMemorySpace.Put( m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, value );
                        break;
                    }
                    case SrslVmOpCodes.OpCompliment:
                    {
                        break;
                    }
                    case SrslVmOpCodes.OpPrefixDecrement:
                    {
                        DynamicSrslVariable value = m_VmStack.Pop();

                        if ( value.DynamicType < DynamicVariableType.True )
                        {
                            value.NumberData =  value.NumberData -= 1;
                        }
                        else
                        {
                            throw new Exception( "Can only negate Integers and Floating Point Numbers!" );
                        }
                        
                        m_VmStack.Push( value );
                        m_CurrentMemorySpace.Put( m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, value );
                        break;
                    }
                    case SrslVmOpCodes.OpPrefixIncrement:
                    {
                        DynamicSrslVariable value = m_VmStack.Pop();

                        if ( value.DynamicType < DynamicVariableType.True )
                        {
                            value.NumberData =  value.NumberData += 1;
                        }
                        else
                        {
                            throw new Exception( "Can only negate Integers and Floating Point Numbers!" );
                        }
                        
                        m_VmStack.Push( value );
                        m_CurrentMemorySpace.Put( m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, value );
                        break;
                    }
                    case SrslVmOpCodes.OpPostfixDecrement:
                    {
                        DynamicSrslVariable value = m_VmStack.Pop();

                        if ( value.DynamicType < DynamicVariableType.True )
                        {
                            value.NumberData =  value.NumberData -= 1;
                        }
                        else
                        {
                            throw new Exception( "Can only negate Integers and Floating Point Numbers!" );
                        }
                        
                        m_VmStack.Push( value );
                        m_CurrentMemorySpace.Put( m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, value );
                        break;
                    }
                    case SrslVmOpCodes.OpPostfixIncrement:
                    {
                        DynamicSrslVariable value = m_VmStack.Pop();

                        if ( value.DynamicType < DynamicVariableType.True)
                        {
                            value.NumberData =  value.NumberData += 1;
                        }
                        else
                        {
                            throw new Exception( "Can only negate Integers and Floating Point Numbers!" );
                        }
                        
                        m_VmStack.Push( value );
                        m_CurrentMemorySpace.Put( m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, value );
                        break;
                    }
                    case SrslVmOpCodes.OpSmaller:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( (valueLhs.DynamicType == 0|| valueLhs.DynamicType < DynamicVariableType.True) &&  (valueRhs.DynamicType == 0 || valueRhs.DynamicType < DynamicVariableType.True))
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData < valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        
                       
                        break;
                    }
                    case SrslVmOpCodes.OpSmallerEqual:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData <= valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpGreater:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData > valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpGreaterEqual:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if (valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData >= valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpEqual:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData == valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpNotEqual:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData != valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpNot:
                    {
                        DynamicSrslVariable value = m_VmStack.Pop();

                        if ( value.DynamicType == DynamicVariableType.True )
                        {
                            value.DynamicType = DynamicVariableType.False;
                        }
                        else
                        {
                            value.DynamicType = DynamicVariableType.True;
                        }
                        m_VmStack.Push( value );
                        break;
                    }
                    case SrslVmOpCodes.OpAdd:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData + valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        
                        break;
                    }
                    
                    case SrslVmOpCodes.OpSubtract:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData - valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }

                    case SrslVmOpCodes.OpMultiply:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData * valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }

                    case SrslVmOpCodes.OpDivide:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData / valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpModulo:
                    {
                        DynamicSrslVariable valueRhs = m_VmStack.Pop();
                        DynamicSrslVariable valueLhs = m_VmStack.Pop();
                        
                        if ( valueLhs.DynamicType < DynamicVariableType.True &&  valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData % valueRhs.NumberData) );
                        }
                        else
                        {
                            throw new Exception( "Can only compare Integers and Floating Point Numbers!" );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpJumpIfFalse:
                    {
                        int offset =  m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        
                        DynamicSrslVariable value = m_VmStack.Pop();

                        if ( value.DynamicType == DynamicVariableType.False )
                        {
                            m_CurrentInstructionPointer = offset;
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpEnterBlock:
                    {
                        int memberCount =  m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer+1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer+3] << 24);m_CurrentInstructionPointer += 4;
                        FastMemorySpace block = m_PoolFastMemoryFastMemory.Get();
                        block.ResetPropertiesArray( memberCount );
                        block.m_EnclosingSpace = m_CurrentMemorySpace;
                        //block.Name = m_CurrentMemorySpace.Name + "Block" + m_CurrentInstructionPointer.ToString();
                        block.StackCountAtBegin = m_VmStack.Count;
                        m_CurrentMemorySpace = block;
                        m_CallStack.Push( m_CurrentMemorySpace );
                        break;
                    }
                    
                    case SrslVmOpCodes.OpExitBlock:
                    {
                        DynamicSrslVariable returnVal = null;
                        if ( m_KeepLastItemOnStackToReturn )
                        {
                            returnVal = m_VmStack.Pop();
                        }
                        if ( m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count )
                        {
                            int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                            for ( int i = 0; i < stackCounter; i++ )
                            {
                                m_VmStack.Pop();
                            }
                        }
                       
                        m_PoolFastMemoryFastMemory.Return( m_CallStack.Pop() );
                        m_CurrentMemorySpace = m_CallStack.Peek();
                        if ( m_KeepLastItemOnStackToReturn )
                        {
                            m_VmStack.Push( returnVal );
                        }
                        break;
                    }
                    case SrslVmOpCodes.OpKeepLastItemOnStack:
                    {
                        m_KeepLastItemOnStackToReturn = true;
                        break;
                    }
                    case SrslVmOpCodes.OpReturn:
                    {
                        DynamicSrslVariable returnVal = m_VmStack.Pop();
                        
                        if ( m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count )
                        {
                            int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                            for ( int i = 0; i < stackCounter; i++ )
                            {
                                m_VmStack.Pop();
                            }
                        }

                        if ( m_CallStack.Peek().CallerChunk.Code != null &&
                             m_CallStack.Peek().CallerChunk.Code != m_CurrentChunk.Code )
                        {
                            m_CurrentChunk = m_CallStack.Peek().CallerChunk;
                            m_CurrentInstructionPointer = m_CallStack.Peek().CallerIntructionPointer;
                        }
                        m_PoolFastMemoryFastMemory.Return( m_CallStack.Pop() );
                        m_CurrentMemorySpace = m_CallStack.Peek();
                        m_KeepLastItemOnStackToReturn = false;
                        m_VmStack.Push( returnVal );
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException("Instruction : " + instruction);
                }
            }
            else
            {
                if ( m_CallStack.Count > 0 )
                {
                    if ( m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count )
                    {
                        int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                        for ( int i = 0; i < stackCounter; i++ )
                        {
                            m_VmStack.Pop();
                        }
                    }
                    
                    if ( m_CallStack.Peek().CallerChunk.Code != null &&
                         m_CallStack.Peek().CallerChunk.Code != m_CurrentChunk.Code )
                    {
                        m_CurrentChunk = m_CallStack.Peek().CallerChunk;
                        m_CurrentInstructionPointer = m_CallStack.Peek().CallerIntructionPointer;
                    }

                    if ( m_CurrentMemorySpace is FastClassMemorySpace || m_CurrentMemorySpace is FastModuleMemorySpace )
                    {
                        m_CallStack.Pop();
                    }
                    else
                    {
                        m_PoolFastMemoryFastMemory.Return( m_CallStack.Pop() );
                    }
                    if ( m_CallStack.Count > 0 )
                    {
                        m_CurrentMemorySpace = m_CallStack.Peek();
                    }
                }
                else
                {
                    return SrslVmInterpretResult.InterpretOk;
                }
            }
        }
       
    }

    public void ShutdownVm()
    {
        //m_VmStack.Clear();
    }
}

}
