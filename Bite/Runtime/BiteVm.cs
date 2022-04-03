//#define BITE_VM_DEBUG_TRACE_EXECUTION

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Bite.Runtime.Bytecode;
using Bite.Runtime.CodeGen;
using Bite.Runtime.Functions;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Memory;
using Bite.SymbolTable;
using Type = System.Type;

namespace Bite.Runtime
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

    public class BiteVm
    {
        private BinaryChunk m_CurrentChunk;
        private int m_CurrentInstructionPointer;
        private DynamicBiteVariableStack m_VmStack;

        private Dictionary<string, BinaryChunk> m_CompiledChunks;

        private List<DynamicBiteVariable> m_FunctionArguments = new List<DynamicBiteVariable>();
        private ObjectPoolFastMemory m_PoolFastMemoryFastMemory;
        private FastGlobalMemorySpace m_GlobalMemorySpace;
        private Scope m_CurrentScope;
        private FastMemorySpace m_CurrentMemorySpace;
        private FastMemoryStack m_CallStack = new FastMemoryStack();
        private UsingStatementStack m_UsingStatementStack;
        private Dictionary<string, FastMethodInfo> CachedMethods = new Dictionary<string, FastMethodInfo>();
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
        private DynamicBiteVariable m_ReturnRegister;
        private BiteVmOpCodes m_CurrentByteCodeInstruction = BiteVmOpCodes.OpNone;

        public Dictionary<string, BinaryChunk> CompiledChunks => m_CompiledChunks;

        protected void InitMemorySpaces()
        {
            m_CurrentMemorySpace = m_GlobalMemorySpace;
            string moduleName = "System";

            FastMemorySpace callSpace = new FastMemorySpace(
                "$root",
                m_GlobalMemorySpace,
                0,
                m_CurrentChunk,
                m_CurrentInstructionPointer,
                4);

            m_GlobalMemorySpace.AddModule(callSpace);

            callSpace.Define(
                DynamicVariableExtension.ToDynamicVariable(new BiteChunkWrapper(new BinaryChunk())),
                "System.Object");

            callSpace.Define(
                DynamicVariableExtension.ToDynamicVariable(new PrintFunctionVm()),
                "System.Print");

            callSpace.Define(
                DynamicVariableExtension.ToDynamicVariable(new ForeignLibraryInterfaceVm()),
                "System.CSharpInterfaceCall");

            if (m_CompiledChunks.TryGetValue("System.CSharpInterface", out var chunk))
            {
                callSpace.Define(
                    DynamicVariableExtension.ToDynamicVariable(new BiteChunkWrapper(chunk)),
                    "System.CSharpInterface");
            }
        }

        private Dictionary<string, object> m_ExternalObjects = new Dictionary<string, object>();

        public void RegisterGlobalObject(string varName, object data)
        {
            m_ExternalObjects.Add(varName, data);
        }

        public BiteVmInterpretResult Interpret(BiteProgram context)
        {
            m_VmStack = new DynamicBiteVariableStack();
            m_UsingStatementStack = new UsingStatementStack();
            m_CallStack = new FastMemoryStack();
            m_PoolFastMemoryFastMemory = new ObjectPoolFastMemory();

            m_GlobalMemorySpace =
                new FastGlobalMemorySpace(context.BaseScope.NumberOfSymbols);

            m_CurrentChunk = context.CompiledMainChunk;
            m_CompiledChunks = context.CompiledChunks;
            m_CurrentInstructionPointer = 0;
            m_CurrentScope = context.BaseScope;

            InitMemorySpaces();

            return Run();
        }

        private BiteVmOpCodes ReadInstruction()
        {
            m_CurrentByteCodeInstruction = (BiteVmOpCodes)m_CurrentChunk.Code[m_CurrentInstructionPointer];
            m_CurrentInstructionPointer++;

            return m_CurrentByteCodeInstruction;
        }

        private ConstantValue ReadConstant()
        {
            ConstantValue instruction =
                m_CurrentChunk.Constants[m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24)];

            m_CurrentInstructionPointer += 4;

            return instruction;
        }

        private BiteVmInterpretResult Run()
        {
            while (true)
            {
                if (m_CurrentInstructionPointer < m_CurrentChunk.Code.Length)
                {
#if BITE_VM_DEBUG_TRACE_EXECUTION
                Console.Write("Stack:   ");
                for ( int i = 0; i < m_VmStack.Count; i++ )
                {
                    Console.Write("[" + m_VmStack.Peek(i) + "]");
                }

               
                Console.Write("\n");
                 
                m_CurrentChunk.DissassembleInstruction( m_CurrentInstructionPointer );
#endif

                    BiteVmOpCodes instruction = ReadInstruction();

                    switch (instruction)
                    {
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

                                m_CurrentScope = (BaseScope)m_CurrentScope.resolve(
                                    moduleName,
                                    out int moduleId,
                                    ref depth);

                                int numberOfMembers = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                FastModuleMemorySpace callSpace = new FastModuleMemorySpace(
                                    $"$module_{moduleName}",
                                    m_GlobalMemorySpace,
                                    m_VmStack.Count,
                                    m_CurrentChunk,
                                    m_CurrentInstructionPointer,
                                    numberOfMembers);

                                m_GlobalMemorySpace.AddModule(callSpace);
                                m_CurrentChunk = CompiledChunks[moduleName];
                                m_CurrentInstructionPointer = 0;
                                m_CurrentMemorySpace = callSpace;
                                m_CallStack.Push(callSpace);

                                break;
                            }

                        case BiteVmOpCodes.OpDefineClass:
                            {
                                string className = ReadConstant().StringConstantValue;

                                BiteChunkWrapper chunkWrapper = new BiteChunkWrapper(
                                    CompiledChunks[className]);

                                m_CurrentMemorySpace.Define(
                                    DynamicVariableExtension.ToDynamicVariable(chunkWrapper));

                                break;
                            }

                        case BiteVmOpCodes.OpDefineMethod:
                            {
                                string methodName = ReadConstant().StringConstantValue;

                                BiteChunkWrapper chunkWrapper = new BiteChunkWrapper(
                                    CompiledChunks[methodName]);

                                m_CurrentMemorySpace.Define(
                                    DynamicVariableExtension.ToDynamicVariable(chunkWrapper),
                                    methodName);

                                break;
                            }

                        case BiteVmOpCodes.OpBindToFunction:
                            {
                                int numberOfArguments = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                        (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                        (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                        (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;
                                m_FunctionArguments.Clear();

                                for (int i = 0; i < numberOfArguments; i++)
                                {
                                    m_FunctionArguments.Add(m_VmStack.Pop());
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpCallFunction:
                            {
                                string method = ReadConstant().StringConstantValue;
                                DynamicBiteVariable call = m_CurrentMemorySpace.Get(method);

                                if (call.ObjectData is BiteChunkWrapper function)
                                {
                                    FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();
                                    callSpace.ResetPropertiesArray(m_FunctionArguments.Count);
                                    callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                                    callSpace.CallerChunk = m_CurrentChunk;
                                    callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                                    callSpace.StackCountAtBegin = m_VmStack.Count;
                                    m_CurrentMemorySpace = callSpace;
                                    m_CallStack.Push(callSpace);

                                    for (int i = 0; i < m_FunctionArguments.Count; i++)
                                    {
                                        m_CurrentMemorySpace.Define(m_FunctionArguments[i]);
                                    }

                                    m_CurrentChunk = function.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;
                                }

                                if (call.ObjectData is IBiteVmCallable callable)
                                {
                                    object returnVal = callable.Call(m_FunctionArguments);

                                    if (returnVal != null)
                                    {
                                        m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(returnVal));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpCallMemberFunction:
                            {
                                ConstantValue constant = ReadConstant();
                                DynamicBiteVariable dynamicBiteVariable = m_VmStack.Pop();

                                if (dynamicBiteVariable.ObjectData is StaticWrapper wrapper)
                                {
                                    string methodName = constant.StringConstantValue;
                                    object[] functionArguments = new object[m_FunctionArguments.Count];
                                    Type[] functionArgumentTypes = new Type[m_FunctionArguments.Count];
                                    int it = 0;
                                    m_FunctionArguments.Reverse();

                                    for (int i = 0; i < m_FunctionArguments.Count; i++)
                                    {
                                        functionArguments[it] = m_FunctionArguments[i].ToObject();
                                        functionArgumentTypes[it] = m_FunctionArguments[i].GetType();
                                        it++;
                                    }

                                    object returnVal = wrapper.InvokeMember(
                                        methodName,
                                        functionArguments,
                                        functionArgumentTypes);

                                    if (returnVal != null)
                                    {
                                        m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(returnVal));
                                    }
                                }
                                else if (dynamicBiteVariable.ObjectData is FastMemorySpace fastMemorySpace)
                                {
                                    string methodName = constant.StringConstantValue;
                                    DynamicBiteVariable call = fastMemorySpace.Get(methodName);

                                    m_CurrentMemorySpace = fastMemorySpace;

                                    if (call.ObjectData != null)
                                    {
                                        if (call.ObjectData is BiteChunkWrapper function)
                                        {
                                            FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();
                                            callSpace.ResetPropertiesArray(m_FunctionArguments.Count);
                                            callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                                            callSpace.CallerChunk = m_CurrentChunk;
                                            callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                                            callSpace.StackCountAtBegin = m_VmStack.Count;
                                            m_CurrentMemorySpace = callSpace;
                                            m_CallStack.Push(callSpace);

                                            foreach (var functionArgument in m_FunctionArguments)
                                            {
                                                m_CurrentMemorySpace.Define(functionArgument);
                                            }

                                            m_CurrentChunk = function.ChunkToWrap;
                                            m_CurrentInstructionPointer = 0;
                                        }

                                        if (call.ObjectData is IBiteVmCallable callable)
                                        {
                                            object returnVal = callable.Call(m_FunctionArguments);

                                            if (returnVal != null)
                                            {
                                                m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(returnVal));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Error expected Function, got null!");
                                    }
                                }
                                else if (dynamicBiteVariable.ObjectData is object obj)
                                {
                                    string callString = obj + "." + constant.StringConstantValue;

                                    if (CachedMethods.ContainsKey(callString))
                                    {
                                        object[] functionArguments = new object[m_FunctionArguments.Count];

                                        //Type[] functionArgumentTypes = new Type[m_FunctionArguments.Count];
                                        int it = 0;

                                        foreach (var functionArgument in m_FunctionArguments)
                                        {
                                            functionArguments[it] = functionArgument.ToObject();

                                            //  functionArgumentTypes[it] = functionArgument.GetType();
                                            it++;
                                        }

                                        object returnVal = CachedMethods[callString].
                                            Invoke(dynamicBiteVariable.ObjectData, functionArguments);

                                        if (returnVal != null)
                                        {
                                            m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(returnVal));
                                        }
                                    }
                                    else
                                    {
                                        Type type = obj.GetType();
                                        object[] functionArguments = new object[m_FunctionArguments.Count];
                                        Type[] functionArgumentTypes = new Type[m_FunctionArguments.Count];
                                        int it = 0;

                                        foreach (var functionArgument in m_FunctionArguments)
                                        {
                                            functionArguments[it] = functionArgument.ToObject();
                                            functionArgumentTypes[it] = functionArgument.GetType();
                                            it++;
                                        }

                                        MethodInfo method = type.GetMethod(
                                            constant.StringConstantValue,
                                            functionArgumentTypes);

                                        if (method != null)
                                        {
                                            FastMethodInfo fastMethodInfo = new FastMethodInfo(method);
                                            CachedMethods.Add(callString, fastMethodInfo);

                                            object returnVal = fastMethodInfo.Invoke(
                                                dynamicBiteVariable.ObjectData,
                                                functionArguments);

                                            if (returnVal != null)
                                            {
                                                m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(returnVal));
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception(
                                                "Error Function " + constant.StringConstantValue + " not found!");
                                        }
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpDefineLocalInstance:
                            {
                                int moduleIdClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                    (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                    (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                    (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int depthClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                 (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                 (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                 (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int idClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int classMemberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                       (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                       (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                       (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                BiteVmOpCodes biteVmOpCode = ReadInstruction();
                                string instanceName = ReadConstant().StringConstantValue;
                                
                                if (m_CurrentMemorySpace.Get(moduleIdClass, depthClass, -1, idClass).ObjectData is
                                    BiteChunkWrapper classWrapper)
                                {
                                    FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace(
                                        $"$class_{moduleIdClass}",
                                        m_CurrentMemorySpace,
                                        m_VmStack.Count,
                                        m_CurrentChunk,
                                        m_CurrentInstructionPointer,
                                        classMemberCount);

                                    m_CurrentMemorySpace.Define(
                                        DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace), instanceName);

                                    m_CurrentMemorySpace = classInstanceMemorySpace;
                                    m_CurrentChunk = classWrapper.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;
                                    m_CallStack.Push(m_CurrentMemorySpace);
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace));
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpDefineLocalVar:
                            {
                                BiteVmOpCodes biteVmOpCode = ReadInstruction();
                                string instanceName = ReadConstant().StringConstantValue;
                                m_CurrentMemorySpace.Define(m_VmStack.Pop(), instanceName);

                                break;
                            }

                        case BiteVmOpCodes.OpDeclareLocalVar:
                            {
                                BiteVmOpCodes biteVmOpCode = ReadInstruction();
                                string instanceName = ReadConstant().StringConstantValue;
                                m_CurrentMemorySpace.Define(DynamicVariableExtension.ToDynamicVariable(), instanceName);

                                break;
                            }

                        case BiteVmOpCodes.OpSetLocalInstance:
                            {
                                int moduleIdLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int depthLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int idLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int moduleIdClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                    (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                    (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                    (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int depthClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                 (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                 (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                 (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int idClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int classMemberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                       (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                       (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                       (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                if (m_CurrentMemorySpace.Get(moduleIdClass, depthClass, -1, idClass).ObjectData is
                                    BiteChunkWrapper classWrapper)
                                {
                                    FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace(
                                        $"class_{moduleIdClass}",
                                        m_CurrentMemorySpace,
                                        m_VmStack.Count,
                                        m_CurrentChunk,
                                        m_CurrentInstructionPointer,
                                        classMemberCount);

                                    classInstanceMemorySpace.m_EnclosingSpace = m_CurrentMemorySpace;
                                    classInstanceMemorySpace.CallerChunk = m_CurrentChunk;
                                    classInstanceMemorySpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                                    classInstanceMemorySpace.StackCountAtBegin = m_VmStack.Count;

                                    m_CurrentMemorySpace.Put(
                                        moduleIdLocalInstance,
                                        depthLocalInstance,
                                        -1,
                                        idLocalInstance,
                                        DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace));

                                    m_CurrentMemorySpace = classInstanceMemorySpace;
                                    m_CurrentChunk = classWrapper.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;

                                    m_CallStack.Push(classInstanceMemorySpace);
                                    
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpGetLocalVar:
                            {
                                m_LastGetLocalVarModuleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                m_LastGetLocalVarDepth = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                m_LastGetLocalClassId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                        (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                        (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                        (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                m_LastGetLocalVarId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                m_VmStack.Push(
                                    m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId));

                                break;
                            }

                        case BiteVmOpCodes.OpGetModule:
                            {
                                int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;
                                FastMemorySpace obj = m_GlobalMemorySpace.GetModule(id);
                                m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(obj));

                                break;
                            }

                        case BiteVmOpCodes.OpGetMember:
                            {
                                int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;
                                FastMemorySpace obj = (FastMemorySpace)m_VmStack.Pop().ObjectData;
                                m_VmStack.Push(obj.Get(-1, 0, -1, id));

                                break;
                            }
                        
                        case BiteVmOpCodes.OpGetMemberWithString:
                        {
                            string member = ReadConstant().StringConstantValue;

                            if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                            {
                                FastMemorySpace obj = (FastMemorySpace)m_VmStack.Pop().ObjectData;
                                m_VmStack.Push(obj.Get(member));
                            }
                            else
                            {
                                object obj = m_VmStack.Pop().ObjectData;

                                FieldInfo field = obj.GetType().GetField( member );

                                if ( field != null )
                                {
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(field.GetValue( obj )));
                                }
                                else
                                {
                                    PropertyInfo propertyInfo = obj.GetType().GetProperty( member );
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(propertyInfo.GetValue( obj )));
                                }
                                
                            }

                            break;
                        }

                        case BiteVmOpCodes.OpSetMember:
                            {
                                int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

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

                        case BiteVmOpCodes.OpSetLocalVar:
                            {
                                int moduleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int depth = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                            (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int classId = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                              (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;
                                m_LastGetLocalVarId = id;
                                m_LastGetLocalVarModuleId = moduleId;
                                m_LastGetLocalVarDepth = depth;
                                m_LastGetLocalClassId = classId;

                                m_VmStack.Push(m_CurrentMemorySpace.Get(moduleId, depth, classId, id));

                                break;
                            }

                        case BiteVmOpCodes.OpConstant:
                            {
                                ConstantValue constantValue = ReadConstant();

                                switch (constantValue.ConstantType)
                                {
                                    case ConstantValueType.Integer:
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(constantValue.IntegerConstantValue));

                                        break;

                                    case ConstantValueType.Double:
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(constantValue.DoubleConstantValue));

                                        break;

                                    case ConstantValueType.String:
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(constantValue.StringConstantValue));

                                        break;

                                    case ConstantValueType.Bool:
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(constantValue.BoolConstantValue));

                                        break;

                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpWhileLoop:
                            {
                                int jumpCodeHeaderStart = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                          (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                          (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                          (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                int jumpCodeBodyEnd = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                      (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;
                                ;
                                m_LoopEndJumpCode = jumpCodeBodyEnd + 10;
                                
                                if (m_VmStack.Pop().DynamicType == DynamicVariableType.True)
                                {
                                    m_CurrentChunk.Code[jumpCodeBodyEnd] = (byte)BiteVmOpCodes.OpJump;
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
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;
                                m_CurrentInstructionPointer = jumpCode;

                                break;
                            }

                        case BiteVmOpCodes.OpAssign:
                            {
                                if (m_SetMember && !m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        fastMemorySpace.Put(-1, 0, -1, m_LastGetLocalVarId, m_VmStack.Pop());
                                    }
                                    else
                                    {
                                        fastMemorySpace.Define(m_VmStack.Pop());
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    if ( m_SetMember )
                                    {
                                        FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                        FastMemorySpace m = (FastMemorySpace)fastMemorySpace.Get( -1, 0, -1, m_LastGetLocalVarId ).ObjectData;
                                        
                                        if (m.NamesToProperties.ContainsKey(m_LastElement))
                                        {
                                            m.Put(m_LastElement, m_VmStack.Pop());
                                        }
                                        else
                                        {
                                            m.Define(m_VmStack.Pop(), m_LastElement, false);
                                        }
                                    }
                                    else
                                    {
                                        FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                        if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                        {
                                            fastMemorySpace.Put(m_LastElement, m_VmStack.Pop());
                                        }
                                        else
                                        {
                                            fastMemorySpace.Define(m_VmStack.Pop(), m_LastElement, false);
                                        }

                                    }
                                    m_SetMember = false;
                                    m_SetElement = false;
                                }
                                else if (m_SetMemberWithString)
                                {
                                    if ( m_VmStack.Peek().ObjectData is FastMemorySpace )
                                    {
                                        FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                        if (fastMemorySpace.NamesToProperties.ContainsKey(m_MemberWithStringToSet))
                                        {
                                            fastMemorySpace.Put(m_MemberWithStringToSet, m_VmStack.Pop());
                                        }
                                        else
                                        {
                                            fastMemorySpace.Define(m_VmStack.Pop(), m_MemberWithStringToSet);
                                        }
                                    }
                                    else
                                    {
                                        object obj = m_VmStack.Pop().ObjectData;

                                        FieldInfo field = obj.GetType().GetField( m_MemberWithStringToSet );

                                        if ( field != null )
                                        {
                                            if ( field.FieldType == typeof(Double) )
                                            {
                                                field.SetValue( obj, m_VmStack.Pop().NumberData );
                                            }
                                            else if ( field.FieldType == typeof(Single) )
                                            {
                                                field.SetValue( obj, (float)m_VmStack.Pop().NumberData );
                                            }
                                            else if ( field.FieldType == typeof(int) )
                                            {
                                                field.SetValue( obj, (int)m_VmStack.Pop().NumberData );
                                            }
                                            else if ( field.FieldType == typeof(string) )
                                            {
                                                field.SetValue( obj, m_VmStack.Pop().StringData );
                                            }
                                            else if ( field.FieldType == typeof(bool) )
                                            {
                                                if ( m_VmStack.Peek().DynamicType == DynamicVariableType.True )
                                                {
                                                    field.SetValue( obj, true );
                                                }
                                                if ( m_VmStack.Peek().DynamicType == DynamicVariableType.False )
                                                {
                                                    field.SetValue( obj, false );
                                                }
                                                
                                            }
                                            else
                                            {
                                                field.SetValue( obj, m_VmStack.Pop().ObjectData );
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
                                        m_VmStack.Pop());
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpDivideAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData / m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData / m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                variable.NumberData / m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpMultiplyAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData * m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData * m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                variable.NumberData * m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpPlusAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData + m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData + m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                variable.NumberData + m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpMinusAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData - m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData - m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                variable.NumberData - m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpModuloAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData % m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    variable.NumberData % m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                variable.NumberData % m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseAndAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData & (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData & (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                (int)variable.NumberData & (int)m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseOrAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData | (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData | (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                (int)variable.NumberData | (int)m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseXorAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData ^ (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData ^ (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                (int)variable.NumberData ^ (int)m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseLeftShiftAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData << (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData << (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                (int)variable.NumberData << (int)m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseRightShiftAssign:
                            {
                                if (m_SetMember)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.Get(-1, 0, -1, m_LastGetLocalVarId);

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                -1,
                                                0,
                                                -1,
                                                m_LastGetLocalVarId,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData >> (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetMember = false;
                                }
                                else if (m_SetElement)
                                {
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        DynamicBiteVariable variable = fastMemorySpace.NamesToProperties[m_LastElement];

                                        if (variable.DynamicType < DynamicVariableType.True &&
                                             m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                        {
                                            fastMemorySpace.Put(
                                                m_LastElement,
                                                DynamicVariableExtension.ToDynamicVariable(
                                                    (int)variable.NumberData >> (int)m_VmStack.Pop().NumberData));
                                        }
                                    }

                                    m_SetElement = false;
                                }
                                else
                                {
                                    m_VmStack.Pop();

                                    DynamicBiteVariable variable = m_CurrentMemorySpace.Get(
                                        m_LastGetLocalVarModuleId,
                                        m_LastGetLocalVarDepth,
                                        m_LastGetLocalClassId,
                                        m_LastGetLocalVarId);

                                    if (variable.DynamicType < DynamicVariableType.True &&
                                         m_VmStack.Peek().DynamicType < DynamicVariableType.True)
                                    {
                                        m_CurrentMemorySpace.Put(
                                            m_LastGetLocalVarModuleId,
                                            m_LastGetLocalVarDepth,
                                            m_LastGetLocalClassId,
                                            m_LastGetLocalVarId,
                                            DynamicVariableExtension.ToDynamicVariable(
                                                (int)variable.NumberData >> (int)m_VmStack.Pop().NumberData));
                                    }
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpSetElement:
                            {
                                int elementAccessCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                //string element = "";
                                m_LastElement = "";

                                for (int i = 0; i < elementAccessCount; i++)
                                {
                                    if (m_VmStack.Peek().DynamicType == DynamicVariableType.String)
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
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                         (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                //string element = "";
                                m_LastElement = "";

                                for (int i = 0; i < elementAccessCount; i++)
                                {
                                    if (m_VmStack.Peek().DynamicType == DynamicVariableType.String)
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
                                m_VmStack.Push(fastMemorySpace.Get(m_LastElement));

                                break;
                            }

                        case BiteVmOpCodes.OpUsingStatmentHead:
                            {
                                m_UsingStatementStack.Push(m_CurrentMemorySpace.Get(-1, 0, -1, 0).ObjectData);

                                break;
                            }

                        case BiteVmOpCodes.OpUsingStatmentEnd:
                            {
                                m_UsingStatementStack.Pop();

                                break;
                            }

                        case BiteVmOpCodes.OpTernary:
                            {
                                if (m_VmStack.Pop().DynamicType == DynamicVariableType.True)
                                {
                                    DynamicBiteVariable biteVariable = m_VmStack.Pop();
                                    m_VmStack.Pop();
                                    m_VmStack.Push(biteVariable);
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

                                if ((valueLhs.DynamicType == DynamicVariableType.True) &&
                                     (valueRhs.DynamicType == DynamicVariableType.True))
                                {
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(true));
                                }
                                else
                                {
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(false));
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpOr:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if ((valueLhs.DynamicType == DynamicVariableType.True) ||
                                     (valueRhs.DynamicType == DynamicVariableType.True))
                                {
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(true));
                                }
                                else
                                {
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(false));
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseOr:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            (int)valueLhs.NumberData | (int)valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseXor:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            (int)valueLhs.NumberData ^ (int)valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseAnd:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            (int)valueLhs.NumberData & (int)valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }
                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseLeftShift:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                    valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            (int)valueLhs.NumberData << (int)valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }
                                break;
                            }

                        case BiteVmOpCodes.OpBitwiseRightShift:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                    valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            (int)valueLhs.NumberData >> (int)valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }
                                break;
                            }


                        case BiteVmOpCodes.OpNegate:
                            {
                                var currentStack = m_VmStack.Peek();

                                if (currentStack.DynamicType < DynamicVariableType.True)
                                {
                                    currentStack.NumberData *= -1;
                                }
                                else
                                {
                                    throw new Exception("Can only negate Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpAffirm:
                            {
                                var currentStack = m_VmStack.Peek();

                                if (currentStack.DynamicType < DynamicVariableType.True)
                                {
                                    currentStack.NumberData *= 1;
                                }
                                else
                                {
                                    throw new Exception("Can only negate Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpCompliment:
                            {
                                var currentStack = m_VmStack.Peek();
                                
                                if (currentStack.DynamicType < DynamicVariableType.True)
                                {
                                    currentStack.NumberData = ~(int)currentStack.NumberData;
                                }
                                else
                                {
                                    throw new Exception("Can only complement Integer Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpPrefixDecrement:
                            {
                                var currentStack = m_VmStack.Peek();

                                if (currentStack.DynamicType < DynamicVariableType.True)
                                {
                                    currentStack.NumberData -= 1;
                                }
                                else
                                {
                                    throw new Exception("Can only decrement Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpPrefixIncrement:
                            {
                                var currentStack = m_VmStack.Peek();

                                if (currentStack.DynamicType < DynamicVariableType.True)
                                {
                                    currentStack.NumberData += 1;
                                }
                                else
                                {
                                    throw new Exception("Can only increment Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpPostfixDecrement:
                            {
                                var currentStack = m_VmStack.Pop();

                                if (currentStack.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(currentStack.NumberData));
                                    currentStack.NumberData -= 1;
                                }
                                else
                                {
                                    throw new Exception("Can only decrement Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpPostfixIncrement:
                            {
                                var currentStack = m_VmStack.Pop();

                                if (currentStack.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(DynamicVariableExtension.ToDynamicVariable(currentStack.NumberData));
                                    currentStack.NumberData += 1;
                                }
                                else
                                {
                                    throw new Exception("Can only increment Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpLess:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if ((valueLhs.DynamicType == 0 || valueLhs.DynamicType < DynamicVariableType.True) &&
                                     (valueRhs.DynamicType == 0 || valueRhs.DynamicType < DynamicVariableType.True))
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData < valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpLessOrEqual:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData <= valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpGreater:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData > valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpGreaterEqual:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData >= valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpEqual:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData == valueRhs.NumberData));
                                }
                                else if (valueLhs.DynamicType == DynamicVariableType.True &&
                                        valueRhs.DynamicType == DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(true));
                                }
                                else if (valueLhs.DynamicType == DynamicVariableType.False &&
                                         valueRhs.DynamicType == DynamicVariableType.False)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(true));
                                }
                                else if (valueLhs.DynamicType == DynamicVariableType.String &&
                                         valueRhs.DynamicType == DynamicVariableType.String)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(valueLhs.StringData == valueRhs.StringData));
                                }
                                else if (valueLhs.DynamicType == DynamicVariableType.False &&
                                         valueRhs.DynamicType == DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(false));
                                }
                                else if (valueLhs.DynamicType == DynamicVariableType.True &&
                                         valueRhs.DynamicType == DynamicVariableType.False)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(false));
                                }
                                else
                                {
                                    throw new Exception("Can only check equality with Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpNotEqual:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData != valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only check equality with Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpNot:
                            {
                                DynamicBiteVariable value = m_VmStack.Pop();

                                if (value.DynamicType == DynamicVariableType.False)
                                {
                                    value.DynamicType = DynamicVariableType.True;
                                }
                                else
                                {
                                    value.DynamicType = DynamicVariableType.False;
                                }

                                m_VmStack.Push(value);

                                break;
                            }

                        case BiteVmOpCodes.OpAdd:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType == DynamicVariableType.String ||
                                     valueRhs.DynamicType == DynamicVariableType.String)
                                {
                                    // TODO: String concat rules
                                    if (valueLhs.DynamicType < DynamicVariableType.True)
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                valueLhs.NumberData + valueRhs.StringData));
                                    }
                                    else if (valueRhs.DynamicType < DynamicVariableType.True)
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                valueLhs.StringData + valueRhs.NumberData));
                                    }
                                    else if (valueLhs.DynamicType == DynamicVariableType.String ||
                                              valueRhs.DynamicType == DynamicVariableType.String)
                                    {
                                        m_VmStack.Push(
                                            DynamicVariableExtension.ToDynamicVariable(
                                                valueLhs.StringData + valueRhs.StringData));
                                    }
                                    else
                                    {
                                        throw new Exception(
                                            "Can only concatenate Strings with Integers and Floating Point Numbers!");
                                    }
                                }
                                else if (valueLhs.DynamicType < DynamicVariableType.True &&
                                          valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData + valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only add Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpSubtract:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData - valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only subtract Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpMultiply:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData * valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only multiply Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpDivide:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData / valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only divide Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpModulo:
                            {
                                DynamicBiteVariable valueRhs = m_VmStack.Pop();
                                DynamicBiteVariable valueLhs = m_VmStack.Pop();

                                if (valueLhs.DynamicType < DynamicVariableType.True &&
                                     valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_VmStack.Push(
                                        DynamicVariableExtension.ToDynamicVariable(
                                            valueLhs.NumberData % valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only modulo Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpJumpIfFalse:
                            {
                                int offset = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                             (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                             (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                             (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;

                                if (m_VmStack.Pop().DynamicType == DynamicVariableType.False)
                                {
                                    m_CurrentInstructionPointer = offset;
                                }

                                break;
                            }

                        case BiteVmOpCodes.OpEnterBlock:
                            {
                                int memberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                                  (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                                  (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                                  (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;
                                FastMemorySpace block = m_PoolFastMemoryFastMemory.Get();
                                block.ResetPropertiesArray(memberCount);
                                block.m_EnclosingSpace = m_CurrentMemorySpace;
                                block.StackCountAtBegin = m_VmStack.Count;
                                m_CurrentMemorySpace = block;
                                m_CallStack.Push(m_CurrentMemorySpace);

                                break;
                            }

                        case BiteVmOpCodes.OpExitBlock:
                            {
                                if (m_KeepLastItemOnStackToReturn)
                                {
                                    m_ReturnRegister = m_VmStack.Pop();
                                }

                                if (m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count)
                                {
                                    int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                                    m_VmStack.Count -= stackCounter;
                                }

                                FastMemorySpace fastMemorySpace = m_CallStack.Pop();

                                if (fastMemorySpace.CallerChunk != null)
                                {
                                    m_CurrentChunk = fastMemorySpace.CallerChunk;
                                    m_CurrentInstructionPointer = fastMemorySpace.CallerIntructionPointer;
                                }

                                m_PoolFastMemoryFastMemory.Return(fastMemorySpace);
                                m_CurrentMemorySpace = m_CallStack.Peek();

                                if (m_KeepLastItemOnStackToReturn)
                                {
                                    m_VmStack.Push(m_ReturnRegister);
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
                                if (m_KeepLastItemOnStackToReturn)
                                {
                                    m_ReturnRegister = m_VmStack.Pop();
                                }

                                if (m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count)
                                {
                                    int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                                    m_VmStack.Count -= stackCounter;
                                }

                                if (m_CallStack.Peek().CallerChunk.Code != null &&
                                     m_CallStack.Peek().CallerChunk.Code != m_CurrentChunk.Code)
                                {
                                    m_CurrentChunk = m_CallStack.Peek().CallerChunk;
                                    m_CurrentInstructionPointer = m_CallStack.Peek().CallerIntructionPointer;
                                }

                                m_PoolFastMemoryFastMemory.Return(m_CallStack.Pop());
                                m_CurrentMemorySpace = m_CallStack.Peek();

                                if (m_KeepLastItemOnStackToReturn)
                                {
                                    m_VmStack.Push(m_ReturnRegister);
                                }

                                m_KeepLastItemOnStackToReturn = false;

                                break;
                            }

                        default:
                            throw new ArgumentOutOfRangeException("Instruction : " + instruction);
                    }
                }
                else
                {
                    if (m_CallStack.Count > 0)
                    {
                        if (m_VmStack.Count > 0)
                        {
                            m_ReturnRegister = m_VmStack.Pop();
                        }

                        if (m_CurrentMemorySpace.StackCountAtBegin < m_VmStack.Count)
                        {
                            int stackCounter = m_VmStack.Count - m_CurrentMemorySpace.StackCountAtBegin;
                            m_VmStack.Count -= stackCounter;
                        }

                        if (m_CallStack.Peek().CallerChunk.Code != null &&
                             m_CallStack.Peek().CallerChunk.Code != m_CurrentChunk.Code)
                        {
                            m_CurrentChunk = m_CallStack.Peek().CallerChunk;
                            m_CurrentInstructionPointer = m_CallStack.Peek().CallerIntructionPointer;
                        }

                        if (m_CurrentMemorySpace is FastClassMemorySpace || m_CurrentMemorySpace is FastModuleMemorySpace)
                        {
                            m_CallStack.Pop();
                        }
                        else
                        {
                            m_PoolFastMemoryFastMemory.Return(m_CallStack.Pop());
                        }

                        if (m_CallStack.Count > 0)
                        {
                            m_CurrentMemorySpace = m_CallStack.Peek();
                        }
                    }
                    else
                    {
                        return BiteVmInterpretResult.InterpretOk;
                    }
                }
            }
        }

        public DynamicBiteVariable ReturnValue => m_ReturnRegister;

        public void ShutdownVm()
        {
            //m_VmStack.Clear();
        }
    }

}
