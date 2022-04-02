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
        private bool m_KeepLastItemOnStackToReturn = false;
        private DynamicBiteVariable m_ReturnRegister = null;
        private SrslVmOpCodes m_CurrentByteCodeInstruction = SrslVmOpCodes.OpNone;

        public Dictionary<string, BinaryChunk> CompiledChunks => m_CompiledChunks;

        protected void InitMemorySpaces()
        {
            m_CurrentMemorySpace = m_GlobalMemorySpace;
            string moduleName = "System";

            FastMemorySpace callSpace = new FastMemorySpace(
                "",
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
                new FastGlobalMemorySpace((context.SymbolTableBuilder.CurrentScope as BaseScope).NumberOfSymbols);

            m_CurrentChunk = context.CompiledMainChunk;
            m_CompiledChunks = context.CompiledChunks;
            m_CurrentInstructionPointer = 0;
            m_CurrentScope = context.SymbolTableBuilder.CurrentScope;

            InitMemorySpaces();

            return Run();
        }

        private SrslVmOpCodes ReadInstruction()
        {
            m_CurrentByteCodeInstruction = (SrslVmOpCodes)m_CurrentChunk.Code[m_CurrentInstructionPointer];
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

                        case SrslVmOpCodes.OpBreak:
                            {
                                m_CurrentInstructionPointer = m_LoopEndJumpCode;

                                break;
                            }

                        case SrslVmOpCodes.OpDefineModule:
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
                                    "",
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

                        case SrslVmOpCodes.OpDefineClass:
                            {
                                string className = ReadConstant().StringConstantValue;

                                BiteChunkWrapper chunkWrapper = new BiteChunkWrapper(
                                    CompiledChunks[className]);

                                m_CurrentMemorySpace.Define(
                                    DynamicVariableExtension.ToDynamicVariable(chunkWrapper));

                                break;
                            }

                        case SrslVmOpCodes.OpDefineMethod:
                            {
                                string methodName = ReadConstant().StringConstantValue;

                                BiteChunkWrapper chunkWrapper = new BiteChunkWrapper(
                                    CompiledChunks[methodName]);

                                m_CurrentMemorySpace.Define(
                                    DynamicVariableExtension.ToDynamicVariable(chunkWrapper),
                                    methodName);

                                break;
                            }

                        case SrslVmOpCodes.OpBindToFunction:
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

                        case SrslVmOpCodes.OpCallFunction:
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

                        case SrslVmOpCodes.OpCallMemberFunction:
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

                        case SrslVmOpCodes.OpDefineLocalInstance:
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

                                if (m_CurrentMemorySpace.Get(moduleIdClass, depthClass, -1, idClass).ObjectData is
                                    BiteChunkWrapper classWrapper)
                                {
                                    FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace(
                                        "",
                                        m_CurrentMemorySpace,
                                        m_VmStack.Count,
                                        m_CurrentChunk,
                                        m_CurrentInstructionPointer,
                                        classMemberCount);

                                    m_CurrentMemorySpace.Define(
                                        DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace));

                                    m_CurrentMemorySpace = classInstanceMemorySpace;
                                    m_CurrentChunk = classWrapper.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;
                                    m_CallStack.Push(m_CurrentMemorySpace);
                                }

                                break;
                            }

                        case SrslVmOpCodes.OpDefineLocalVar:
                            {
                                m_CurrentMemorySpace.Define(m_VmStack.Pop());

                                break;
                            }

                        case SrslVmOpCodes.OpDeclareLocalVar:
                            {
                                m_CurrentMemorySpace.Define(DynamicVariableExtension.ToDynamicVariable());

                                break;
                            }

                        case SrslVmOpCodes.OpSetLocalInstance:
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
                                        "",
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

                        case SrslVmOpCodes.OpGetLocalVar:
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

                        case SrslVmOpCodes.OpGetModule:
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

                        case SrslVmOpCodes.OpGetMember:
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

                        case SrslVmOpCodes.OpSetMember:
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

                        case SrslVmOpCodes.OpSetLocalVar:
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

                        case SrslVmOpCodes.OpConstant:
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

                        case SrslVmOpCodes.OpWhileLoop:
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
                                int jumpCode = m_CurrentChunk.Code[m_CurrentInstructionPointer] |
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) |
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) |
                                               (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);

                                m_CurrentInstructionPointer += 4;
                                m_CurrentInstructionPointer = jumpCode;

                                break;
                            }

                        case SrslVmOpCodes.OpAssign:
                            {
                                if (m_SetMember)
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
                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_VmStack.Pop().ObjectData;

                                    if (fastMemorySpace.NamesToProperties.ContainsKey(m_LastElement))
                                    {
                                        fastMemorySpace.Put(m_LastElement, m_VmStack.Pop());
                                    }
                                    else
                                    {
                                        fastMemorySpace.Define(m_VmStack.Pop(), m_LastElement, false);
                                    }

                                    m_SetElement = false;
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

                        case SrslVmOpCodes.OpDivideAssign:
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

                        case SrslVmOpCodes.OpMultiplyAssign:
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

                        case SrslVmOpCodes.OpPlusAssign:
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

                        case SrslVmOpCodes.OpMinusAssign:
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

                        case SrslVmOpCodes.OpModuloAssign:
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

                        case SrslVmOpCodes.OpBitwiseAndAssign:
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

                        case SrslVmOpCodes.OpBitwiseOrAssign:
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

                        case SrslVmOpCodes.OpBitwiseXorAssign:
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

                        case SrslVmOpCodes.OpBitwiseLeftShiftAssign:
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

                        case SrslVmOpCodes.OpBitwiseRightShiftAssign:
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

                        case SrslVmOpCodes.OpSetElement:
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

                        case SrslVmOpCodes.OpGetElement:
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

                        case SrslVmOpCodes.OpUsingStatmentHead:
                            {
                                m_UsingStatementStack.Push(m_CurrentMemorySpace.Get(-1, 0, -1, 0).ObjectData);

                                break;
                            }

                        case SrslVmOpCodes.OpUsingStatmentEnd:
                            {
                                m_UsingStatementStack.Pop();

                                break;
                            }

                        case SrslVmOpCodes.OpTernary:
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
                        case SrslVmOpCodes.OpAnd:
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

                        case SrslVmOpCodes.OpOr:
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

                        case SrslVmOpCodes.OpBitwiseOr:
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

                        case SrslVmOpCodes.OpBitwiseXor:
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

                        case SrslVmOpCodes.OpBitwiseAnd:
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

                        case SrslVmOpCodes.OpBitwiseLeftShift:
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

                        case SrslVmOpCodes.OpBitwiseRightShift:
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


                        case SrslVmOpCodes.OpNegate:
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

                        case SrslVmOpCodes.OpAffirm:
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

                        case SrslVmOpCodes.OpCompliment:
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

                        case SrslVmOpCodes.OpPrefixDecrement:
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

                        case SrslVmOpCodes.OpPrefixIncrement:
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

                        case SrslVmOpCodes.OpPostfixDecrement:
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

                        case SrslVmOpCodes.OpPostfixIncrement:
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

                        case SrslVmOpCodes.OpSmaller:
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

                        case SrslVmOpCodes.OpSmallerEqual:
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

                        case SrslVmOpCodes.OpGreater:
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

                        case SrslVmOpCodes.OpGreaterEqual:
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

                        case SrslVmOpCodes.OpEqual:
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
                                else
                                {
                                    throw new Exception("Can only check equality with Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case SrslVmOpCodes.OpNotEqual:
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

                        case SrslVmOpCodes.OpNot:
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

                        case SrslVmOpCodes.OpAdd:
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

                        case SrslVmOpCodes.OpSubtract:
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

                        case SrslVmOpCodes.OpMultiply:
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

                        case SrslVmOpCodes.OpDivide:
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

                        case SrslVmOpCodes.OpModulo:
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

                        case SrslVmOpCodes.OpJumpIfFalse:
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

                        case SrslVmOpCodes.OpEnterBlock:
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

                        case SrslVmOpCodes.OpExitBlock:
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

                        case SrslVmOpCodes.OpKeepLastItemOnStack:
                            {
                                m_KeepLastItemOnStackToReturn = true;

                                break;
                            }

                        case SrslVmOpCodes.OpReturn:
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
