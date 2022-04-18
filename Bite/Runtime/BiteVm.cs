//#define BITE_VM_DEBUG_TRACE_EXECUTION

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bite.Runtime.Bytecode;
using Bite.Runtime.CodeGen;
using Bite.Runtime.Functions;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Memory;

namespace Bite.Runtime
{

public class BiteVm
{
    private enum ContextMode
    {
        CurrentContext,
        SynchronizedContext
    }

    private BinaryChunk m_CurrentChunk;
    private int m_CurrentInstructionPointer;
    private DynamicBiteVariableStack m_VmStack;

    private readonly List < DynamicBiteVariable > m_FunctionArguments = new List < DynamicBiteVariable >();

    private ObjectPoolFastMemory m_PoolFastMemoryFastMemory;
    private FastGlobalMemorySpace m_GlobalMemorySpace;
    private FastMemorySpace m_CurrentMemorySpace;
    private FastMemoryStack m_CallStack = new FastMemoryStack();
    private UsingStatementStack m_UsingStatementStack;

    private readonly MethodCache m_CachedMethods = new MethodCache();
    private readonly PropertyCache m_CachedProperties = new PropertyCache();
    public FastPropertyCache FastCachedProperties = new FastPropertyCache();
    private readonly FieldCache m_CachedFields = new FieldCache();

    private int m_LastGetLocalVarId = -1;
    private int m_LastGetLocalVarModuleId = -1;
    private int m_LastGetLocalVarDepth = -1;
    private int m_LastGetLocalClassId = -1;
    private int m_LoopEndJumpCode = -1;
    private string m_LastElement = "";
    private bool m_SetElement = false;
    private bool m_SetMember = false;
    private string m_MemberWithStringToSet = "";
    private bool m_SetMemberWithString = false;
    private bool m_KeepLastItemOnStackToReturn = false;
    private bool m_SetVarWithExternalName = false;
    private string m_SetVarExternalName = "";
    private bool m_GetNextVarByRef = false;
    private bool m_PushNextAssignmentOnStack = false;
    private BiteVmOpCodes m_CurrentByteCodeInstruction = BiteVmOpCodes.OpNone;

    private Dictionary < string, BinaryChunk > m_CompiledChunks;

    private readonly Dictionary < string, object > m_ExternalObjects = new Dictionary < string, object >();
    private readonly Dictionary < string, IBiteVmCallable > m_Callables = new Dictionary < string, IBiteVmCallable >();

    private BiteFunctionCall m_CallBack;

    private bool m_CallbackWaiting = false;
    private bool m_SpinLock = false;
    private int m_InstructionPointerBeforeExecutingCallback = -1;
    
    private ContextMode m_ContextMode;
    private bool m_ExitRunLoop;
    private bool m_Stopping = false;

    private CancellationToken m_CancellationToken;
    
    private int m_CurrentLineNumberPointer = 0;

    /// <summary>
    ///     Will contain the last value on the stack when the program exits
    /// </summary>
    public DynamicBiteVariable ReturnValue { get; private set; }

    /// <summary>
    ///     Gets or sets the <see cref="SynchronizationContext" /> to be used inside a sync block
    /// </summary>
    public SynchronizationContext SynchronizationContext { get; set; }

    #region Public
    
    public void InitVm()
    {
        m_VmStack = new DynamicBiteVariableStack();
        m_UsingStatementStack = new UsingStatementStack();
        m_CallStack = new FastMemoryStack();
        m_PoolFastMemoryFastMemory = new ObjectPoolFastMemory();

        m_GlobalMemorySpace =
            new FastGlobalMemorySpace( 10 );

        m_CompiledChunks = new Dictionary < string, BinaryChunk >();
    }

    /// <summary>
    ///     Executes the specified <see cref="BiteProgram" /> on the current thread
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    public BiteVmInterpretResult Interpret( BiteProgram program )
    {
        return Interpret( program, new CancellationToken() );
    }

    /// <summary>
    ///     Executes the specified <see cref="BiteProgram" /> on the current thread
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    public BiteVmInterpretResult Interpret( BiteProgram program, CancellationToken token )
    {
        m_CurrentChunk = program.CompiledMainChunk;

        m_CancellationToken = token;
        m_Stopping = false;

        foreach ( KeyValuePair < string, BinaryChunk > compiledChunk in m_CompiledChunks )
        {
            if ( !program.CompiledChunks.ContainsKey( compiledChunk.Key ) )
            {
                program.CompiledChunks.Add( compiledChunk.Key, compiledChunk.Value );
            }
        }

        m_CompiledChunks = program.CompiledChunks;
        m_CurrentInstructionPointer = 0;
        m_CurrentLineNumberPointer = 0;
        BiteVmInterpretResult result = BiteVmInterpretResult.Continue;

        // This while loop exists to allow us to switch contexts from within code using the sync keyword
        while ( result == BiteVmInterpretResult.Continue && !m_Stopping )
        {
            m_ExitRunLoop = false;

            switch ( m_ContextMode )
            {
                // Execute instructions in the current context (thread)
                case ContextMode.CurrentContext:
                    result = Run();

                    break;

                // Execute instructions synchronously in the specified synchronized context (thread)
                case ContextMode.SynchronizedContext:
                    if ( SynchronizationContext == null )
                    {
                        // No context, try to run normally
                        result = Run();
                    }
                    else
                    {
                        SynchronizationContext.Send( o => { result = Run(); }, null );
                    }

                    break;
            }
        }

        return result;
    }

    /// <summary>
    ///     Executes the specified <see cref="BiteProgram" /> on new Task via Task.Run
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    public Task < BiteVmInterpretResult > InterpretAsync( BiteProgram program )
    {
        return InterpretAsync( program, CancellationToken.None );
    }

    /// <summary>
    ///     Executes the specified <see cref="BiteProgram" /> on new Task via Task.Run
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    public async Task < BiteVmInterpretResult > InterpretAsync( BiteProgram program, CancellationToken token )
    {
        return await Task.Run( () => { return Interpret( program, token ); } );
    }

    /// <summary>
    ///     Registers a <see cref="IBiteVmCallable" /> class as an extern callable method with the specified linkId
    /// </summary>
    /// <param name="linkId"></param>
    /// <param name="callable"></param>
    public void RegisterCallable( string linkId, IBiteVmCallable callable )
    {
        m_Callables.Add( linkId, callable );
    }

    /// <summary>
    ///     Registers an external object as a global variable
    /// </summary>
    /// <param name="varName"></param>
    /// <param name="data"></param>
    public void RegisterExternalGlobalObject( string varName, object data )
    {
        m_ExternalObjects.Add( varName, data );
    }

    /// <summary>
    ///     Registers a set of external objects as global variables
    /// </summary>
    /// <param name="externalObjects"></param>
    public void RegisterExternalGlobalObjects( Dictionary < string, object > externalObjects )
    {
        if ( externalObjects != null )
        {
            foreach ( KeyValuePair < string, object > externalObject in externalObjects )
            {
                m_ExternalObjects.Add( externalObject.Key, externalObject.Value );
            }
        }
    }
    
    public void CallBiteFunction( BiteFunctionCall biteFunctionCall )
    {
        m_CallBack = biteFunctionCall;
        m_SpinLock = true;
        m_CallbackWaiting = true;

        while ( m_SpinLock )
        {
            Thread.Sleep( new TimeSpan( 1 ) );
        }
    }

    /// <summary>
    ///     Requests the current <see cref="BiteVm" /> to stop execution and exit as soon as the current instruction has
    ///     completed.
    ///     If execution is running on a thread, this action will not by synchronous
    /// </summary>
    public void Stop()
    {
        m_ExitRunLoop = true;
        m_Stopping = true;
    }

    #endregion

    #region Private

    private ConstantValue ReadConstant()
    {
        ConstantValue instruction =
            m_CurrentChunk.Constants[m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                     ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                     ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                     ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 )];

        m_CurrentInstructionPointer += 4;

        return instruction;
    }

    private BiteVmOpCodes ReadInstruction()
    {
        m_CurrentByteCodeInstruction = ( BiteVmOpCodes ) m_CurrentChunk.Code[m_CurrentInstructionPointer];
        m_CurrentInstructionPointer++;

        return m_CurrentByteCodeInstruction;
    }

    private BiteVmInterpretResult Run()
    {
        BiteVmInterpretResult result = BiteVmInterpretResult.Continue;

        while ( !m_ExitRunLoop )
        {
            if ( m_CancellationToken != null && m_CancellationToken.IsCancellationRequested )
            {
                return BiteVmInterpretResult.Cancelled;
            }

            if ( m_CallbackWaiting && m_CurrentInstructionPointer != m_InstructionPointerBeforeExecutingCallback )
            {
                 m_InstructionPointerBeforeExecutingCallback = m_CurrentInstructionPointer;
                    string method = m_CallBack.FunctionName;

                    if ( m_CallBack.BiteChunkWrapper != null )
                    {
                        FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();

                        //callSpace.ResetPropertiesArray( m_FunctionArguments.Count );
                        callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                        callSpace.CallerChunk = m_CurrentChunk;
                        callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                        callSpace.CallerLineNumberPointer = m_CurrentLineNumberPointer;
                        callSpace.StackCountAtBegin = m_VmStack.Count;
                        callSpace.IsRunningCallback = true;
                        m_CurrentMemorySpace = callSpace;
                        m_CallStack.Push( callSpace );

                        for ( int i = 0; i < m_CallBack.FunctionArguments.Length; i++ )
                        {
                            m_CurrentMemorySpace.Define(
                                m_CallBack.FunctionArguments[i] );
                        }

                        m_CurrentChunk = m_CallBack.BiteChunkWrapper.ChunkToWrap;
                        m_CurrentInstructionPointer = 0;
                        m_CurrentLineNumberPointer = 0;
                    }
                    else
                    {
                        DynamicBiteVariable call = m_CurrentMemorySpace.Get( method );

                        if ( call.ObjectData is BiteChunkWrapper function )
                        {
                            FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();

                            //callSpace.ResetPropertiesArray( m_FunctionArguments.Count );
                            callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                            callSpace.CallerChunk = m_CurrentChunk;
                            callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                            callSpace.CallerLineNumberPointer = m_CurrentLineNumberPointer;
                            callSpace.StackCountAtBegin = m_VmStack.Count;
                            callSpace.IsRunningCallback = true;
                            m_CurrentMemorySpace = callSpace;
                            m_CallStack.Push( callSpace );

                            for ( int i = 0; i < m_CallBack.FunctionArguments.Length; i++ )
                            {
                                m_CurrentMemorySpace.Define(
                                    m_CallBack.FunctionArguments[i] );
                            }

                            m_CurrentChunk = function.ChunkToWrap;
                            m_CurrentInstructionPointer = 0;
                            m_CurrentLineNumberPointer = 0;
                        }
                        else if ( call.ObjectData is IBiteVmCallable callable )
                        {
                            object returnVal = callable.Call( m_FunctionArguments );
                            m_FunctionArguments.Clear();

                            if ( returnVal != null )
                            {
                                m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( returnVal ) );
                            }
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Function " + method + " not found!" );
                        }
                    }
                    m_CallbackWaiting = false;
            }
            if ( m_CurrentInstructionPointer < m_CurrentChunk.Code.Length )
            {
#if BITE_VM_DEBUG_TRACE_EXECUTION
                Console.Write( "Stack:   " );

                for ( int i = 0; i < m_VmStack.Count; i++ )
                {
                    Console.Write( "[" + m_VmStack.Peek( i ) + "]" );
                }

                Console.Write( "\n" );

                m_CurrentChunk.DissassembleInstruction( m_CurrentInstructionPointer, m_CurrentLineNumberPointer );
#endif

                BiteVmOpCodes instruction = ReadInstruction();

                switch ( instruction )
                {
                    case BiteVmOpCodes.OpSwitchContext:
                    {
                        m_ContextMode = ContextMode.SynchronizedContext;
                        m_ExitRunLoop = true;

                        break;
                    }

                    case BiteVmOpCodes.OpReturnContext:
                    {
                        m_ContextMode = ContextMode.CurrentContext;
                        m_ExitRunLoop = true;

                        break;
                    }

                    case BiteVmOpCodes.OpNone:
                    {
                        break;
                    }

                    case BiteVmOpCodes.OpPopStack:
                    {
                        m_VmStack.Pop();

                        break;
                    }

                    case BiteVmOpCodes.OpBreak:
                    {
                        m_CurrentInstructionPointer = m_LoopEndJumpCode;

                        break;
                    }

                    case BiteVmOpCodes.OpDefineModule:
                    {
                        string moduleName = ReadConstant().StringConstantValue;
                        int depth = 0;

                        int numberOfMembers = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;
                        FastMemorySpace fastMemorySpace = m_GlobalMemorySpace.GetModule( $"$module_{moduleName}" );

                        if ( fastMemorySpace == null )
                        {
                            FastModuleMemorySpace callSpace = new FastModuleMemorySpace(
                                $"$module_{moduleName}",
                                m_GlobalMemorySpace,
                                m_VmStack.Count,
                                m_CurrentChunk,
                                m_CurrentInstructionPointer,
                                m_CurrentLineNumberPointer,
                                numberOfMembers );

                            m_GlobalMemorySpace.AddModule( callSpace );
                            m_CurrentChunk = m_CompiledChunks[moduleName];
                            m_CurrentInstructionPointer = 0;
                            m_CurrentLineNumberPointer = 0;
                            m_CurrentMemorySpace = callSpace;
                            m_CallStack.Push( callSpace );
                        }
                        else
                        {
                            m_CurrentChunk = m_CompiledChunks[moduleName];
                            m_CurrentInstructionPointer = 0;
                            m_CurrentMemorySpace = fastMemorySpace;
                            m_CallStack.Push( fastMemorySpace );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpDefineClass:
                    {
                        string className = ReadConstant().StringConstantValue;

                        BiteChunkWrapper chunkWrapper = new BiteChunkWrapper(
                            m_CompiledChunks[className] );

                        m_CurrentMemorySpace.Define(
                            DynamicVariableExtension.ToDynamicVariable( chunkWrapper ) );

                        break;
                    }

                    case BiteVmOpCodes.OpDefineMethod:
                    {
                        string fullQualifiedMethodName = ReadConstant().StringConstantValue;

                        string methodName = m_VmStack.Pop().StringData;

                        BiteChunkWrapper chunkWrapper = new BiteChunkWrapper(
                            m_CompiledChunks[fullQualifiedMethodName] );

                        m_CurrentMemorySpace.Define(
                            DynamicVariableExtension.ToDynamicVariable( chunkWrapper ),
                            methodName );

                        break;
                    }

                    case BiteVmOpCodes.OpDefineCallableMethod:
                    {
                        string fullQualifiedMethodName = ReadConstant().StringConstantValue;

                        string methodName = m_VmStack.Pop().StringData;

                        if ( m_Callables.TryGetValue( methodName, out IBiteVmCallable callable ) )
                        {
                            m_CurrentMemorySpace.Define(
                                DynamicVariableExtension.ToDynamicVariable( callable ),
                                methodName );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException( $"No such Callable {methodName}" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBindToFunction:
                    {
                        int numberOfArguments = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        for ( int i = 0; i < numberOfArguments; i++ )
                        {
                            m_FunctionArguments.Add( m_VmStack.Pop() );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpSetFunctionParameterName:
                    {
                        int numberOfParameter = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        for ( int i = numberOfParameter; i > 0; i-- )
                        {
                            m_CurrentMemorySpace.SetNameOfVariable( i - 1, m_VmStack.Pop().StringData );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpCallFunctionByName:
                    {
                        string method = ReadConstant().StringConstantValue;
                        DynamicBiteVariable call = m_CurrentMemorySpace.Get( method );

                        if ( call.ObjectData is BiteChunkWrapper function )
                        {
                            FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();

                            //callSpace.ResetPropertiesArray( m_FunctionArguments.Count );
                            callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                            callSpace.CallerChunk = m_CurrentChunk;
                            callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                            callSpace.CallerLineNumberPointer = m_CurrentLineNumberPointer;
                            callSpace.StackCountAtBegin = m_VmStack.Count;
                            m_CurrentMemorySpace = callSpace;
                            m_CallStack.Push( callSpace );

                            for ( int i = 0; i < m_FunctionArguments.Count; i++ )
                            {
                                m_CurrentMemorySpace.Define( m_FunctionArguments[i] );
                            }

                            m_FunctionArguments.Clear();
                            m_CurrentChunk = function.ChunkToWrap;
                            m_CurrentInstructionPointer = 0;
                            m_CurrentLineNumberPointer = 0;
                        }
                        else if ( call.ObjectData is IBiteVmCallable callable )
                        {
                            object returnVal = callable.Call( m_FunctionArguments );
                            m_FunctionArguments.Clear();

                            if ( returnVal != null )
                            {
                                m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( returnVal ) );
                            }
                        }
                        else
                        {
                            throw new BiteVmRuntimeException( "Runtime Error: Function " + method + " not found!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpCallFunctionFromStack:
                    {
                        FastMemorySpace fastMemorySpace = m_VmStack.Pop().ObjectData as FastMemorySpace;

                        m_CurrentMemorySpace = fastMemorySpace;

                        if ( m_VmStack.Count > 0 &&
                             m_VmStack.Peek().ObjectData is BiteChunkWrapper functionFromStack )
                        {
                            m_VmStack.Pop();
                            FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();

                            //callSpace.ResetPropertiesArray( m_FunctionArguments.Count );
                            callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                            callSpace.CallerChunk = m_CurrentChunk;
                            callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                            callSpace.CallerLineNumberPointer = m_CurrentLineNumberPointer;
                            callSpace.StackCountAtBegin = m_VmStack.Count;
                            m_CurrentMemorySpace = callSpace;
                            m_CallStack.Push( callSpace );

                            foreach ( DynamicBiteVariable functionArgument in m_FunctionArguments )
                            {
                                m_CurrentMemorySpace.Define( functionArgument );
                            }

                            m_FunctionArguments.Clear();
                            m_CurrentChunk = functionFromStack.ChunkToWrap;
                            m_CurrentInstructionPointer = 0;
                            m_CurrentLineNumberPointer = 0;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException( "Runtime Error: Function not found!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpCallMemberFunction:
                    {
                        ConstantValue constant = ReadConstant();
                        DynamicBiteVariable dynamicBiteVariable = m_VmStack.Pop();

                        if ( dynamicBiteVariable.ObjectData is StaticWrapper wrapper )
                        {
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

                            if ( m_CachedMethods.TryGetMethod(
                                    wrapper.StaticWrapperType,
                                    functionArgumentTypes,
                                    constant.StringConstantValue,
                                    out FastMethodInfo fastMethodInfo ) )
                            {
                                object returnVal = fastMethodInfo.
                                    Invoke( dynamicBiteVariable.ObjectData, functionArguments );

                                m_FunctionArguments.Clear();

                                if ( returnVal != null )
                                {
                                    m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( returnVal ) );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Function " + constant.StringConstantValue + " not found!" );
                            }
                        }
                        else if ( dynamicBiteVariable.ObjectData is ICSharpEvent cSharpEvent )
                        {
                            EventInfo eventInfo = (EventInfo)m_VmStack.Pop().ObjectData;
                            cSharpEvent.Invoke( eventInfo.Name, m_FunctionArguments );
                            m_FunctionArguments.Clear();
                        }
                        else if ( dynamicBiteVariable.ObjectData is FastMemorySpace fastMemorySpace )
                        {
                            string methodName = constant.StringConstantValue;
                            DynamicBiteVariable call = fastMemorySpace.Get( methodName );

                            m_CurrentMemorySpace = fastMemorySpace;

                            if ( call.ObjectData != null )
                            {
                                if ( call.ObjectData is BiteChunkWrapper function )
                                {
                                    FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();

                                    //callSpace.ResetPropertiesArray( m_FunctionArguments.Count );
                                    callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                                    callSpace.CallerChunk = m_CurrentChunk;
                                    callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                                    callSpace.CallerLineNumberPointer = m_CurrentLineNumberPointer;
                                    callSpace.StackCountAtBegin = m_VmStack.Count;
                                    m_CurrentMemorySpace = callSpace;
                                    m_CallStack.Push( callSpace );

                                    foreach ( DynamicBiteVariable functionArgument in m_FunctionArguments )
                                    {
                                        m_CurrentMemorySpace.Define( functionArgument );
                                    }

                                    m_FunctionArguments.Clear();
                                    m_CurrentChunk = function.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;
                                    m_CurrentLineNumberPointer = 0;
                                }

                                if ( call.ObjectData is IBiteVmCallable callable )
                                {
                                    object returnVal = callable.Call( m_FunctionArguments );
                                    m_FunctionArguments.Clear();

                                    if ( returnVal != null )
                                    {
                                        m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( returnVal ) );
                                    }
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Function " + constant.StringConstantValue + " not found!" );
                            }
                        }
                        else if ( dynamicBiteVariable.ObjectData is object obj )
                        {
                            //string callString = obj + "." + constant.StringConstantValue;
                            Type type = obj.GetType();

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

                            if ( m_CachedMethods.TryGetMethod(
                                    type,
                                    functionArgumentTypes,
                                    constant.StringConstantValue,
                                    out FastMethodInfo fastMethodInfo ) )
                            {
                                object returnVal = fastMethodInfo.
                                    Invoke( dynamicBiteVariable.ObjectData, functionArguments );

                                m_FunctionArguments.Clear();

                                if ( returnVal != null )
                                {
                                    m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( returnVal ) );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Function " + constant.StringConstantValue + " not found!" );
                            }
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Function " + constant.StringConstantValue + " not found!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpDefineInstance:
                    {
                        int moduleIdClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                            ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                            ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                            ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int depthClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                         ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                         ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                         ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int idClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int classMemberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                               ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                               ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                               ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        BiteVmOpCodes biteVmOpCode = ReadInstruction();
                        string instanceName = ReadConstant().StringConstantValue;

                        if ( m_CurrentMemorySpace.Get( moduleIdClass, depthClass, -1, idClass ).ObjectData is
                            BiteChunkWrapper classWrapper )
                        {
                            FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace(
                                $"{moduleIdClass}",
                                m_CurrentMemorySpace,
                                m_VmStack.Count + 1,
                                m_CurrentChunk,
                                m_CurrentInstructionPointer,
                                m_CurrentLineNumberPointer,
                                classMemberCount );

                            m_CurrentMemorySpace.Define(
                                DynamicVariableExtension.ToDynamicVariable( classInstanceMemorySpace ),
                                instanceName );

                            m_CurrentMemorySpace = classInstanceMemorySpace;
                            m_CurrentChunk = classWrapper.ChunkToWrap;
                            m_CurrentInstructionPointer = 0;
                            m_CurrentLineNumberPointer = 0;
                            m_CallStack.Push( m_CurrentMemorySpace );
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( classInstanceMemorySpace ) );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpDefineVar:
                    {
                        BiteVmOpCodes biteVmOpCode = ReadInstruction();
                        string instanceName = ReadConstant().StringConstantValue;
                        m_CurrentMemorySpace.Define( m_VmStack.Pop(), instanceName );

                        break;
                    }

                    case BiteVmOpCodes.OpDeclareVar:
                    {
                        BiteVmOpCodes biteVmOpCode = ReadInstruction();
                        string instanceName = ReadConstant().StringConstantValue;
                        m_CurrentMemorySpace.Define( DynamicVariableExtension.ToDynamicVariable(), instanceName );

                        break;
                    }

                    case BiteVmOpCodes.OpSetInstance:
                    {
                        int moduleIdLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int depthLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int idLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int moduleIdClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                            ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                            ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                            ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int depthClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                         ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                         ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                         ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int idClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int classMemberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                               ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                               ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                               ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        BiteVmOpCodes biteVmOpCode = ReadInstruction();
                        string instanceName = ReadConstant().StringConstantValue;

                        if ( m_CurrentMemorySpace.Get( moduleIdClass, depthClass, -1, idClass ).ObjectData is
                            BiteChunkWrapper classWrapper )
                        {
                            FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace(
                                $"{moduleIdClass}",
                                m_CurrentMemorySpace,
                                m_VmStack.Count,
                                m_CurrentChunk,
                                m_CurrentInstructionPointer,
                                m_CurrentLineNumberPointer,
                                classMemberCount );

                            classInstanceMemorySpace.m_EnclosingSpace = m_CurrentMemorySpace;
                            classInstanceMemorySpace.CallerChunk = m_CurrentChunk;
                            classInstanceMemorySpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                            classInstanceMemorySpace.StackCountAtBegin = m_VmStack.Count;

                            m_CurrentMemorySpace.Put(
                                moduleIdLocalInstance,
                                depthLocalInstance,
                                -1,
                                idLocalInstance,
                                DynamicVariableExtension.ToDynamicVariable( classInstanceMemorySpace ) );

                            m_CurrentMemorySpace = classInstanceMemorySpace;
                            m_CurrentChunk = classWrapper.ChunkToWrap;
                            m_CurrentInstructionPointer = 0;
                            m_CurrentLineNumberPointer = 0;
                            m_CallStack.Push( classInstanceMemorySpace );
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( classInstanceMemorySpace ) );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpGetNextVarByRef:
                    {
                        m_GetNextVarByRef = true;

                        break;
                    }

                    case BiteVmOpCodes.OpGetVar:
                    {
                        m_LastGetLocalVarModuleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        m_LastGetLocalVarDepth = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        m_LastGetLocalClassId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        m_LastGetLocalVarId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        if ( m_GetNextVarByRef )
                        {
                            m_VmStack.Push(
                                m_CurrentMemorySpace.Get(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId ) );

                            m_GetNextVarByRef = false;
                        }
                        else
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId ) ) );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpGetVarExternal:
                    {
                        string varName = ReadConstant().StringConstantValue;

                        if ( m_ExternalObjects.ContainsKey( varName ) )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( m_ExternalObjects[varName] ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException( $"Runtime Error: External object: {varName} not found!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpGetModule:
                    {
                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;
                        FastMemorySpace obj = m_GlobalMemorySpace.GetModule( id );
                        m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( obj ) );

                        break;
                    }

                    case BiteVmOpCodes.OpGetMember:
                    {
                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;
                        FastMemorySpace obj = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                        m_VmStack.Push( obj.Get( -1, 0, -1, id ) );

                        break;
                    }

                    case BiteVmOpCodes.OpGetMemberWithString:
                    {
                        string member = ReadConstant().StringConstantValue;
                        if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                        {
                            FastMemorySpace obj = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                            m_VmStack.Push( obj.Get( member ) );
                        }
                        else
                        {
                            object obj = m_VmStack.Pop().ObjectData;

                            if ( obj != null )
                            {
                                if ( obj is StaticWrapper wrapper )
                                {
                                    if ( FastCachedProperties.TryGetProperty(
                                            wrapper.StaticWrapperType,
                                            member,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                fastPropertyInfo.InvokeGet( null ) ) );
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            wrapper.StaticWrapperType,
                                            member,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                propertyInfo.GetValue( null ) ) );
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 wrapper.StaticWrapperType,
                                                 member,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                fieldInfo.GetField( null ) ) );
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {member} not found!" );
                                    }
                                }
                                else if ( obj is ICSharpEvent eventWrapper )
                                {
                                    if ( eventWrapper.TryGetEventInfo( member, out EventInfo eventInfo ) )
                                    {
                                        m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(eventInfo) );
                                        m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable(eventWrapper) );
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {member} not found!" );
                                    }
                                }
                                else
                                {
                                    Type type = obj.GetType();

                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            member,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                fastPropertyInfo.InvokeGet( obj ) ) );
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            member,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                propertyInfo.GetValue( obj ) ) );
                                    }
                                    else if ( m_CachedFields.TryGetField( type, member, out FastFieldInfo fieldInfo ) )
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                fieldInfo.GetField( obj ) ) );
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {member} not found!" );
                                    }
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException( $"Runtime Error: Member: {member} not found!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpSetMember:
                    {
                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;
                        m_SetMember = true;
                        m_LastGetLocalVarId = id;

                        break;
                    }

                    case BiteVmOpCodes.OpSetMemberWithString:
                    {
                        m_MemberWithStringToSet = ReadConstant().StringConstantValue;
                        m_SetMemberWithString = true;

                        break;
                    }

                    case BiteVmOpCodes.OpSetVar:
                    {
                        int moduleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                       ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                       ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                       ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int depth = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                    ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int classId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                      ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;
                        m_LastGetLocalVarId = id;
                        m_LastGetLocalVarModuleId = moduleId;
                        m_LastGetLocalVarDepth = depth;
                        m_LastGetLocalClassId = classId;

                        m_VmStack.Push( m_CurrentMemorySpace.Get( moduleId, depth, classId, id ) );

                        break;
                    }

                    case BiteVmOpCodes.OpSetVarExternal:
                    {
                        m_SetVarExternalName = ReadConstant().StringConstantValue;
                        m_SetVarWithExternalName = true;

                        break;
                    }

                    case BiteVmOpCodes.OpConstant:
                    {
                        ConstantValue constantValue = ReadConstant();

                        switch ( constantValue.ConstantType )
                        {
                            case ConstantValueType.Integer:
                                m_VmStack.Push(
                                    DynamicVariableExtension.ToDynamicVariable( constantValue.IntegerConstantValue ) );

                                break;

                            case ConstantValueType.Double:
                                m_VmStack.Push(
                                    DynamicVariableExtension.ToDynamicVariable( constantValue.DoubleConstantValue ) );

                                break;

                            case ConstantValueType.String:
                                m_VmStack.Push(
                                    DynamicVariableExtension.ToDynamicVariable( constantValue.StringConstantValue ) );

                                break;

                            case ConstantValueType.Bool:
                                m_VmStack.Push(
                                    DynamicVariableExtension.ToDynamicVariable( constantValue.BoolConstantValue ) );
                                break;
                            case ConstantValueType.Null:
                                DynamicBiteVariable dynamicBiteVariable = new DynamicBiteVariable();
                                dynamicBiteVariable.DynamicType = DynamicVariableType.Null;
                                m_VmStack.Push(
                                    dynamicBiteVariable );

                                break;

                            default:
                                throw new BiteVmRuntimeException(
                                    $"Runtime Error: Wrong constant value type: {( int ) constantValue.ConstantType}" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpWhileLoop:
                    {
                        int jumpCodeHeaderStart = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                  ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                  ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                  ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        int jumpCodeBodyEnd = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                              ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;
                        ;
                        m_LoopEndJumpCode = jumpCodeBodyEnd + 10;

                        if ( m_VmStack.Pop().DynamicType == DynamicVariableType.True )
                        {
                            m_CurrentChunk.Code[jumpCodeBodyEnd] = ( byte ) BiteVmOpCodes.OpJump;
                            IntByteStruct intByteStruct = new IntByteStruct();
                            intByteStruct.integer = jumpCodeHeaderStart;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 1] = intByteStruct.byte0;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 2] = intByteStruct.byte1;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 3] = intByteStruct.byte2;
                            m_CurrentChunk.Code[jumpCodeBodyEnd + 4] = intByteStruct.byte3;
                        }
                        else
                        {
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

                    case BiteVmOpCodes.OpJump:
                    {
                        int jumpCode = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                       ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                       ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                       ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;
                        m_CurrentInstructionPointer = jumpCode;

                        break;
                    }

                    case BiteVmOpCodes.OpPushNextAssignmentOnStack:
                    {
                        m_PushNextAssignmentOnStack = true;

                        break;
                    }

                    case BiteVmOpCodes.OpAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            if ( fastMemorySpace.Exist( -1, 0, -1, m_LastGetLocalVarId ) )
                            {
                                fastMemorySpace.Put( -1, 0, -1, m_LastGetLocalVarId, m_VmStack.Pop() );
                            }
                            else
                            {
                                fastMemorySpace.Define( m_VmStack.Pop() );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetVarWithExternalName )
                        {
                            m_ExternalObjects[m_SetVarExternalName] = m_VmStack.Pop().ToObject();

                            m_SetVarWithExternalName = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.Get( -1, 0, -1, m_LastGetLocalVarId ).
                                                                        ObjectData;

                                if ( m.NamesToProperties.ContainsKey( m_LastElement ) )
                                {
                                    m.Put( m_LastElement, m_VmStack.Pop() );
                                }
                                else
                                {
                                    m.Define( m_VmStack.Pop(), m_LastElement, false );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                if ( fastMemorySpace.NamesToProperties.ContainsKey( m_LastElement ) )
                                {
                                    fastMemorySpace.Put( m_LastElement, m_VmStack.Pop() );
                                }
                                else
                                {
                                    fastMemorySpace.Define( m_VmStack.Pop(), m_LastElement, false );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                if ( fastMemorySpace.NamesToProperties.ContainsKey( m_MemberWithStringToSet ) )
                                {
                                    fastMemorySpace.Put( m_MemberWithStringToSet, m_VmStack.Pop() );
                                }
                                else
                                {
                                    fastMemorySpace.Define( m_VmStack.Pop(), m_MemberWithStringToSet );
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        object data = m_VmStack.PopDataByType( fastPropertyInfo.PropertyType );

                                        fastPropertyInfo.InvokeSet( obj, data );
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        object data = m_VmStack.PopDataByType( propertyInfo.PropertyType );

                                        propertyInfo.SetValue( obj, data );
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        object data = m_VmStack.PopDataByType( fieldInfo.FieldType );

                                        fieldInfo.SetField( obj, data );
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            m_CurrentMemorySpace.Put(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId,
                                m_VmStack.Pop() );
                        }

                        if ( m_PushNextAssignmentOnStack )
                        {
                            m_PushNextAssignmentOnStack = false;
                            m_VmStack.Count++;
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpDivideAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData / valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData / valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData / valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData / valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / ( float ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / ( float ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / ( float ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs / ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData / valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpMultiplyAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData * valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData * valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData * valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData * valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * ( float ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * ( float ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * ( float ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs * ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData * valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpPlusAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData + valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData + valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData + valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData + valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else if ( m_VmStack.Peek().ObjectData is ICSharpEvent eventWrapper )
                            {
                                m_VmStack.Pop();

                                eventWrapper.TryAddEventHandler(
                                    m_MemberWithStringToSet,
                                    ( BiteChunkWrapper ) m_VmStack.Pop().ObjectData,
                                    this );
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + ( float ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + ( float ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + ( float ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs + ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData + valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpMinusAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData - valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData - valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData - valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData - valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - ( float ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.InvokeSet( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - ( float ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - ( int ) valueRhs.NumberData );

                                            propertyInfo.SetValue( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - ( float ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs - ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData - valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpModuloAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData % valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData % valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData % valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData % valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % ( float ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.InvokeSet( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % ( float ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % ( int ) valueRhs.NumberData );

                                            propertyInfo.SetValue( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % ( float ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs % ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData % valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseAndAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData & ( int ) valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData & ( int ) valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData & ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData & ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs & ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs & ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs & ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.InvokeSet( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs & ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs & ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs & ( int ) valueRhs.NumberData );

                                            propertyInfo.SetValue( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs & ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs & ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs & ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData & ( int ) valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseOrAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData | ( int ) valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData | ( int ) valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData | ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData | ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs | ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs | ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs | ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.InvokeSet( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs | ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs | ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs | ( int ) valueRhs.NumberData );

                                            propertyInfo.SetValue( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs | ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs | ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs | ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData | ( int ) valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseXorAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData ^ ( int ) valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData ^ ( int ) valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData ^ ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData ^ ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs ^ ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs ^ ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs ^ ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.InvokeSet( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs ^ ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs ^ ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs ^ ( int ) valueRhs.NumberData );

                                            propertyInfo.SetValue( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs ^ ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs ^ ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs ^ ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData ^ ( int ) valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseLeftShiftAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData << ( int ) valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData << ( int ) valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData << ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData << ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs << ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs << ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs << ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.InvokeSet( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs << ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs << ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs << ( int ) valueRhs.NumberData );

                                            propertyInfo.SetValue( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs << ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs << ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs << ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData << ( int ) valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseRightShiftAssign:
                    {
                        if ( m_SetMember && !m_SetElement )
                        {
                            FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                            DynamicBiteVariable valueLhs = fastMemorySpace.GetLocalVar( m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData >> ( int ) valueRhs.NumberData );

                                fastMemorySpace.PutLocalVar( m_LastGetLocalVarId, value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }

                            m_SetMember = false;
                        }
                        else if ( m_SetElement )
                        {
                            if ( m_SetMember )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                FastMemorySpace m =
                                    ( FastMemorySpace ) fastMemorySpace.GetLocalVar( m_LastGetLocalVarId ).ObjectData;

                                DynamicBiteVariable valueLhs = m.NamesToProperties[m_LastElement];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData >> ( int ) valueRhs.NumberData );

                                    m.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }
                            else
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;
                                DynamicBiteVariable valueLhs = fastMemorySpace.NamesToProperties[m_LastElement];
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData >> ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                                }
                            }

                            m_SetMember = false;
                            m_SetElement = false;
                        }
                        else if ( m_SetMemberWithString )
                        {
                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace fastMemorySpace = ( FastMemorySpace ) m_VmStack.Pop().ObjectData;

                                DynamicBiteVariable valueLhs =
                                    fastMemorySpace.NamesToProperties[m_MemberWithStringToSet];

                                DynamicBiteVariable valueRhs = m_VmStack.Pop();

                                if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True )
                                {
                                    DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                        ( int ) valueLhs.NumberData >> ( int ) valueRhs.NumberData );

                                    fastMemorySpace.Put(
                                        m_LastElement,
                                        value );

                                    if ( m_PushNextAssignmentOnStack )
                                    {
                                        m_PushNextAssignmentOnStack = false;
                                        m_VmStack.Push( value );
                                    }
                                }
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                if ( obj != null )
                                {
                                    Type type = null;

                                    if ( obj is StaticWrapper wrapper )
                                    {
                                        type = wrapper.StaticWrapperType;
                                    }
                                    else
                                    {
                                        type = obj.GetType();
                                    }

                                    DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                    if ( FastCachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out IFastPropertyInfo fastPropertyInfo ) )
                                    {
                                        if ( fastPropertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs >> ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs >> ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.
                                                InvokeSet(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fastPropertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fastPropertyInfo.InvokeGet( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs >> ( int ) valueRhs.NumberData );

                                            fastPropertyInfo.InvokeSet( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedProperties.TryGetProperty(
                                            type,
                                            m_MemberWithStringToSet,
                                            out PropertyInfo propertyInfo ) )
                                    {
                                        if ( propertyInfo.PropertyType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs >> ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs >> ( int ) valueRhs.NumberData );

                                            propertyInfo.
                                                SetValue(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( propertyInfo.PropertyType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) propertyInfo.GetValue( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs >> ( int ) valueRhs.NumberData );

                                            propertyInfo.SetValue( obj, ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else if ( m_CachedFields.TryGetField(
                                                 type,
                                                 m_MemberWithStringToSet,
                                                 out FastFieldInfo fieldInfo ) )
                                    {
                                        if ( fieldInfo.FieldType == typeof( double ) &&
                                             valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            double valueLhs =
                                                ( double ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs >> ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( float ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            float valueLhs =
                                                ( float ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    ( int ) valueLhs >> ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( float ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else if ( fieldInfo.FieldType == typeof( int ) &&
                                                  valueRhs.DynamicType < DynamicVariableType.True )
                                        {
                                            int valueLhs =
                                                ( int ) fieldInfo.GetField( obj );

                                            DynamicBiteVariable value =
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    valueLhs >> ( int ) valueRhs.NumberData );

                                            fieldInfo.
                                                SetField(
                                                    obj,
                                                    ( int ) value.NumberData );

                                            if ( m_PushNextAssignmentOnStack )
                                            {
                                                m_PushNextAssignmentOnStack = false;
                                                m_VmStack.Push( value );
                                            }
                                        }
                                        else
                                        {
                                            throw new BiteVmRuntimeException(
                                                "Runtime Error: Invalid types for arithmetic operation!" );
                                        }
                                    }
                                    else
                                    {
                                        throw new BiteVmRuntimeException(
                                            $"Runtime Error: Member: {m_MemberWithStringToSet} not found!" );
                                    }
                                }
                                else
                                {
                                    throw new BiteVmRuntimeException(
                                        "Runtime Error: Invalid object!" );
                                }
                            }

                            m_SetMemberWithString = false;
                        }
                        else
                        {
                            m_VmStack.Pop();

                            DynamicBiteVariable valueLhs = m_CurrentMemorySpace.Get(
                                m_LastGetLocalVarModuleId,
                                m_LastGetLocalVarDepth,
                                m_LastGetLocalClassId,
                                m_LastGetLocalVarId );

                            DynamicBiteVariable valueRhs = m_VmStack.Pop();

                            if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                 valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                DynamicBiteVariable value = DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData >> ( int ) valueRhs.NumberData );

                                m_CurrentMemorySpace.Put(
                                    m_LastGetLocalVarModuleId,
                                    m_LastGetLocalVarDepth,
                                    m_LastGetLocalClassId,
                                    m_LastGetLocalVarId,
                                    value );

                                if ( m_PushNextAssignmentOnStack )
                                {
                                    m_PushNextAssignmentOnStack = false;
                                    m_VmStack.Push( value );
                                }
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only use integers and floating point numbers for arithmetic Operations!" );
                            }
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpSetElement:
                    {
                        int elementAccessCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        //string element = "";
                        m_LastElement = "";

                        for ( int i = 0; i < elementAccessCount; i++ )
                        {
                            if ( m_VmStack.Peek().DynamicType == DynamicVariableType.String )
                            {
                                m_LastElement += m_VmStack.Pop().StringData;
                            }
                            else
                            {
                                m_LastElement += m_VmStack.Pop().NumberData;
                            }
                        }

                        m_SetElement = true;

                        //FastMemorySpace fastMemorySpace = m_VmStack.Pop().ObjectData as FastMemorySpace;
                        //m_VmStack.Push(fastMemorySpace.Get( element ));
                        break;
                    }

                    case BiteVmOpCodes.OpGetElement:
                    {
                        int elementAccessCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                                 ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        //string element = "";
                        m_LastElement = "";

                        for ( int i = 0; i < elementAccessCount; i++ )
                        {
                            if ( m_VmStack.Peek().DynamicType == DynamicVariableType.String )
                            {
                                m_LastElement += m_VmStack.Pop().StringData;
                            }
                            else
                            {
                                m_LastElement += m_VmStack.Pop().NumberData;
                            }
                        }

                        //SetElement = true;
                        FastMemorySpace fastMemorySpace = m_VmStack.Pop().ObjectData as FastMemorySpace;
                        m_VmStack.Push( fastMemorySpace.Get( m_LastElement ) );

                        break;
                    }

                    case BiteVmOpCodes.OpUsingStatmentHead:
                    {
                        m_UsingStatementStack.Push( m_CurrentMemorySpace.Get( -1, 0, -1, 0 ).ObjectData );

                        break;
                    }

                    case BiteVmOpCodes.OpUsingStatmentEnd:
                    {
                        m_UsingStatementStack.Pop();

                        break;
                    }

                    case BiteVmOpCodes.OpTernary:
                    {
                        if ( m_VmStack.Pop().DynamicType == DynamicVariableType.True )
                        {
                            DynamicBiteVariable biteVariable = m_VmStack.Pop();
                            m_VmStack.Pop();
                            m_VmStack.Push( biteVariable );
                        }
                        else
                        {
                            m_VmStack.Pop();
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpAnd:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType == DynamicVariableType.True &&
                             valueRhs.DynamicType == DynamicVariableType.True )
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( true ) );
                        }
                        else
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( false ) );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpOr:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType == DynamicVariableType.True ||
                             valueRhs.DynamicType == DynamicVariableType.True )
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( true ) );
                        }
                        else
                        {
                            m_VmStack.Push( DynamicVariableExtension.ToDynamicVariable( false ) );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseOr:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData | ( int ) valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only perform bitwise or on integers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseXor:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData ^ ( int ) valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only perform bitwise xor on integers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseAnd:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData & ( int ) valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only perform bitwise and on integers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseLeftShift:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData << ( int ) valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only perform bitwise left shift on integers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpBitwiseRightShift:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    ( int ) valueLhs.NumberData >> ( int ) valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only perform bitwise right shift on integers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpNegate:
                    {
                        DynamicBiteVariable currentStack = m_VmStack.Peek();

                        if ( currentStack.DynamicType < DynamicVariableType.True )
                        {
                            currentStack.NumberData *= -1;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only negate integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpAffirm:
                    {
                        DynamicBiteVariable currentStack = m_VmStack.Peek();

                        if ( currentStack.DynamicType < DynamicVariableType.True )
                        {
                            currentStack.NumberData *= 1;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only affirm integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpCompliment:
                    {
                        DynamicBiteVariable currentStack = m_VmStack.Peek();

                        if ( currentStack.DynamicType < DynamicVariableType.True )
                        {
                            currentStack.NumberData = ~( int ) currentStack.NumberData;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException( "Runtime Error: Can only complement integer numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpPrefixDecrement:
                    {
                        DynamicBiteVariable currentStack = m_VmStack.Peek();

                        if ( currentStack.DynamicType < DynamicVariableType.True )
                        {
                            currentStack.NumberData -= 1;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only decrement integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpPrefixIncrement:
                    {
                        DynamicBiteVariable currentStack = m_VmStack.Peek();

                        if ( currentStack.DynamicType < DynamicVariableType.True )
                        {
                            currentStack.NumberData += 1;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only increment integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpPostfixDecrement:
                    {
                        DynamicBiteVariable currentStack = m_VmStack.Pop();

                        if ( currentStack.DynamicType < DynamicVariableType.True )
                        {
                            currentStack.NumberData -= 1;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only decrement integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpPostfixIncrement:
                    {
                        DynamicBiteVariable currentStack = m_VmStack.Pop();

                        if ( currentStack.DynamicType < DynamicVariableType.True )
                        {
                            currentStack.NumberData += 1;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only increment integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpLess:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( ( valueLhs.DynamicType == 0 || valueLhs.DynamicType < DynamicVariableType.True ) &&
                             ( valueRhs.DynamicType == 0 || valueRhs.DynamicType < DynamicVariableType.True ) )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData < valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only compare integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpLessOrEqual:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData <= valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only compare integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpGreater:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData > valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only compare integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpGreaterEqual:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData >= valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only compare integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpEqual:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData == valueRhs.NumberData ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.True &&
                                  valueRhs.DynamicType == DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( true ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.False &&
                                  valueRhs.DynamicType == DynamicVariableType.False )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( true ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.String &&
                                  valueRhs.DynamicType == DynamicVariableType.String )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.StringData == valueRhs.StringData ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.Object &&
                                  valueRhs.DynamicType == DynamicVariableType.Object )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.ObjectData == valueRhs.ObjectData ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.False &&
                                  valueRhs.DynamicType == DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( false ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.True &&
                                  valueRhs.DynamicType == DynamicVariableType.False )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( false ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only check equality on integers, floating point numbers, strings, objects and boolean values!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpNotEqual:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData != valueRhs.NumberData ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.True &&
                                  valueRhs.DynamicType == DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( false ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.False &&
                                  valueRhs.DynamicType == DynamicVariableType.False )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( false ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.String &&
                                  valueRhs.DynamicType == DynamicVariableType.String )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.StringData != valueRhs.StringData ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.Object &&
                                  valueRhs.DynamicType == DynamicVariableType.Object )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.ObjectData != valueRhs.ObjectData ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.Object &&
                                  valueRhs.DynamicType == DynamicVariableType.Null )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.ObjectData != null ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.Null &&
                                  valueRhs.DynamicType == DynamicVariableType.Object )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    null != valueRhs.ObjectData ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.False &&
                                  valueRhs.DynamicType == DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( true ) );
                        }
                        else if ( valueLhs.DynamicType == DynamicVariableType.True &&
                                  valueRhs.DynamicType == DynamicVariableType.False )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable( true ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only check equality on integers, floating point numbers, strings, objects and boolean values!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpNot:
                    {
                        DynamicBiteVariable value = m_VmStack.Pop();

                        if ( value.DynamicType == DynamicVariableType.False )
                        {
                            value.DynamicType = DynamicVariableType.True;
                        }
                        else if ( value.DynamicType == DynamicVariableType.True )
                        {
                            value.DynamicType = DynamicVariableType.False;
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only perform not-operation on boolean values!" );
                        }

                        m_VmStack.Push( value );

                        break;
                    }

                    case BiteVmOpCodes.OpAdd:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType == DynamicVariableType.String ||
                             valueRhs.DynamicType == DynamicVariableType.String )
                        {
                            // TODO: String concat rules
                            if ( valueLhs.DynamicType < DynamicVariableType.True )
                            {
                                m_VmStack.Push(
                                    DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.NumberData + valueRhs.StringData ) );
                            }
                            else if ( valueRhs.DynamicType < DynamicVariableType.True )
                            {
                                m_VmStack.Push(
                                    DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.StringData + valueRhs.NumberData ) );
                            }
                            else if ( valueLhs.DynamicType == DynamicVariableType.String ||
                                      valueRhs.DynamicType == DynamicVariableType.String )
                            {
                                m_VmStack.Push(
                                    DynamicVariableExtension.ToDynamicVariable(
                                        valueLhs.StringData + valueRhs.StringData ) );
                            }
                            else
                            {
                                throw new BiteVmRuntimeException(
                                    "Runtime Error: Can only concatenate strings with integers and floating point numbers!" );
                            }
                        }
                        else if ( valueLhs.DynamicType < DynamicVariableType.True &&
                                  valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData + valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only concatenate strings with integers and floating point numbers. Or add integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpSubtract:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData - valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only subtract integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpMultiply:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData * valueRhs.NumberData ) );
                        }
                        else if(valueLhs.DynamicType == DynamicVariableType.Object && valueRhs.DynamicType < DynamicVariableType.True)
                        {
                            object obj = valueLhs.ToObject();
                            Type objType = obj.GetType();
                            Type[] argsTypes = { objType, typeof(float) };

                            if ( m_CachedMethods.TryGetMethod(
                                    obj.GetType(),
                                    argsTypes,
                                    "op_Multiply",
                                    out FastMethodInfo fastMethodInfo ) )
                            {
                                object[] args = { obj, (float)valueRhs.NumberData };
                                fastMethodInfo.Invoke( obj, args );
                            }
                        }
                        else if(valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType == DynamicVariableType.Object)
                        {
                            object obj = valueRhs.ToObject();
                            Type objType = obj.GetType();
                            Type[] argsTypes = { typeof(float), objType };

                            if ( m_CachedMethods.TryGetMethod(
                                    obj.GetType(),
                                    argsTypes,
                                    "op_Multiply",
                                    out FastMethodInfo fastMethodInfo ) )
                            {
                                object[] args = { (float)valueLhs.NumberData, obj };
                                fastMethodInfo.Invoke( obj, args );
                            }
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only multiply integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpDivide:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData / valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only divide integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpModulo:
                    {
                        DynamicBiteVariable valueRhs = m_VmStack.Pop();
                        DynamicBiteVariable valueLhs = m_VmStack.Pop();

                        if ( valueLhs.DynamicType < DynamicVariableType.True &&
                             valueRhs.DynamicType < DynamicVariableType.True )
                        {
                            m_VmStack.Push(
                                DynamicVariableExtension.ToDynamicVariable(
                                    valueLhs.NumberData % valueRhs.NumberData ) );
                        }
                        else
                        {
                            throw new BiteVmRuntimeException(
                                "Runtime Error: Can only modulo integers and floating point numbers!" );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpJumpIfFalse:
                    {
                        int offset = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                     ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                     ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                     ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;

                        if ( m_VmStack.Pop().DynamicType == DynamicVariableType.False )
                        {
                            m_CurrentInstructionPointer = offset;
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpEnterBlock:
                    {
                        int memberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                          ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8 ) |
                                          ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16 ) |
                                          ( m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24 );

                        m_CurrentInstructionPointer += 4;
                        FastMemorySpace block = m_PoolFastMemoryFastMemory.Get();

                        //block.ResetPropertiesArray( memberCount );
                        block.m_EnclosingSpace = m_CurrentMemorySpace;
                        block.StackCountAtBegin = m_VmStack.Count;
                        m_CurrentMemorySpace = block;
                        m_CallStack.Push( m_CurrentMemorySpace );

                        break;
                    }

                    case BiteVmOpCodes.OpExitBlock:
                    {
                        if ( m_KeepLastItemOnStackToReturn )
                        {
                            ReturnValue = m_VmStack.Pop();
                        }

                        if ( m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count )
                        {
                            int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                            m_VmStack.Count -= stackCounter;
                        }

                        FastMemorySpace fastMemorySpace = m_CallStack.Pop();

                        if ( fastMemorySpace.CallerChunk != null )
                        {
                            m_CurrentChunk = fastMemorySpace.CallerChunk;
                            m_CurrentInstructionPointer = fastMemorySpace.CallerIntructionPointer;
                            m_CurrentLineNumberPointer = fastMemorySpace.CallerLineNumberPointer;
                        }

                        m_PoolFastMemoryFastMemory.Return( fastMemorySpace );
                        m_CurrentMemorySpace = m_CallStack.Peek();

                        if ( m_KeepLastItemOnStackToReturn )
                        {
                            m_VmStack.Push( ReturnValue );
                        }

                        break;
                    }

                    case BiteVmOpCodes.OpKeepLastItemOnStack:
                    {
                        m_KeepLastItemOnStackToReturn = true;

                        break;
                    }

                    case BiteVmOpCodes.OpReturn:
                    {
                        if ( m_KeepLastItemOnStackToReturn )
                        {
                            ReturnValue = m_VmStack.Pop();
                        }

                        if ( m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count )
                        {
                            int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                            m_VmStack.Count -= stackCounter;
                        }

                        if ( m_CallStack.Peek().CallerChunk != null && 
                             m_CallStack.Peek().CallerChunk.Code != null)
                        {
                            m_CurrentChunk = m_CallStack.Peek().CallerChunk;
                            m_CurrentInstructionPointer = m_CallStack.Peek().CallerIntructionPointer;
                            m_CurrentLineNumberPointer = m_CallStack.Peek().CallerLineNumberPointer;
                        }

                        m_PoolFastMemoryFastMemory.Return( m_CallStack.Pop() );
                        
                        if ( m_KeepLastItemOnStackToReturn )
                        {
                            m_VmStack.Push( ReturnValue );
                        }

                        m_KeepLastItemOnStackToReturn = false;
                        
                        if ( m_CurrentMemorySpace.IsRunningCallback )
                        {
                            m_SpinLock = false;
                        }
                        m_CurrentMemorySpace = m_CallStack.Peek();
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException( "Instruction : " + instruction );
                }
                m_CurrentLineNumberPointer++;
            }
            else
            {
                if ( m_CallStack.Count > 0 )
                {
                    if ( m_VmStack.Count > 0 )
                    {
                        ReturnValue = m_VmStack.Peek();
                    }

                    if ( m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count )
                    {
                        int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                        m_VmStack.Count -= stackCounter;
                    }

                    if (m_CallStack.Peek().CallerChunk != null && 
                        m_CallStack.Peek().CallerChunk.Code != null)
                    {
                        m_CurrentChunk = m_CallStack.Peek().CallerChunk;
                        m_CurrentInstructionPointer = m_CallStack.Peek().CallerIntructionPointer;
                        m_CurrentLineNumberPointer = m_CallStack.Peek().CallerLineNumberPointer;
                    }

                    if ( m_CurrentMemorySpace is FastClassMemorySpace || m_CurrentMemorySpace is FastModuleMemorySpace )
                    {
                        m_CallStack.Pop();
                    }
                    else
                    {
                        m_PoolFastMemoryFastMemory.Return( m_CallStack.Pop() );
                    }

                    if ( m_CurrentMemorySpace.IsRunningCallback )
                    {
                        m_SpinLock = false;
                    }
                    if ( m_CallStack.Count > 0 )
                    {
                        m_CurrentMemorySpace = m_CallStack.Peek();
                    }
                }
                else
                {
                    if ( m_VmStack.Count > 0 )
                    {
                        ReturnValue = m_VmStack.Peek();
                    }

                    return BiteVmInterpretResult.InterpretOk;
                }
            }
        }

        return result;
    }

    #endregion
}

}
