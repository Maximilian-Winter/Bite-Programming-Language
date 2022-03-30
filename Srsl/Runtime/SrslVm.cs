﻿//#define SRSL_VM_DEBUG_TRACE_EXECUTION

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Srsl.Runtime.Bytecode;
using Srsl.Runtime.CodeGen;
using Srsl.Runtime.Functions;
using Srsl.Runtime.Functions.ForeignInterface;
using Srsl.Runtime.Memory;
using Srsl.SymbolTable;
using Type = System.Type;

namespace Srsl.Runtime
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
        private DynamicSrslVariable[] m_VmStack;
        private int m_StackPointer;
        private Dictionary<string, BinaryChunk> m_CompiledChunks;

        private DynamicSrslVariable m_TopMostStackItem;

        private List<DynamicSrslVariable> m_FunctionArguments = new List<DynamicSrslVariable>();
        private ObjectPoolFastMemory m_PoolFastMemoryFastMemory;
        private FastGlobalMemorySpace m_GlobalMemorySpace;
        private Scope m_CurrentScope;
        private FastMemorySpace m_CurrentMemorySpace;
        private FastMemorySpace m_LastMemorySpaceDebug;
        private FastMemoryStack m_CallStack = new FastMemoryStack();
        private Dictionary<string, FastMethodInfo> CachedMethods = new Dictionary<string, FastMethodInfo>();
        private int m_LastGetLocalVarId = -1;
        private int m_LastGetLocalVarModuleId = -1;
        private int m_LastGetLocalVarDepth = -1;
        private int m_LastGetLocalClassId = -1;
        private bool m_SetMember = false;
        private bool m_KeepLastItemOnStackToReturn = false;
        private SrslVmOpCodes m_CurrentByteCodeInstruction = SrslVmOpCodes.OpNone;
        public Dictionary<string, BinaryChunk> CompiledChunks => m_CompiledChunks;


        protected void InitMemorySpaces()
        {
            m_CurrentMemorySpace = m_GlobalMemorySpace;
            string moduleName = "System";
            FastMemorySpace callSpace = new FastMemorySpace("", m_GlobalMemorySpace, 0, m_CurrentChunk, m_CurrentInstructionPointer, 4);
            m_GlobalMemorySpace.Modules.Add(callSpace);
            callSpace.Define(
                DynamicVariableExtension.ToDynamicVariable(new SrslChunkWrapper(new BinaryChunk())), "System.Object");

            callSpace.Define(
                DynamicVariableExtension.ToDynamicVariable(new PrintFunctionVm()), "System.Print");
            callSpace.Define(
                DynamicVariableExtension.ToDynamicVariable(new ForeignLibraryInterfaceVm()), "System.CSharpInterfaceCall");

            if (m_CompiledChunks.TryGetValue("System.CSharpInterface", out var chunk))
            {
                callSpace.Define(
                    DynamicVariableExtension.ToDynamicVariable(new SrslChunkWrapper(chunk)), "System.CSharpInterface");
            }
        }

        private Dictionary<string, object> m_ExternalObjects = new Dictionary<string, object>();

        public void RegisterGlobalObject(string varName, object data)
        {
            m_ExternalObjects.Add(varName, data);
        }

        public SrslVmInterpretResult Interpret(SrslProgram context)
        {
            m_VmStack = new DynamicSrslVariable[512];
            m_StackPointer = 0;
            m_CurrentChunk = context.CompiledMainChunk;
            m_CompiledChunks = context.CompiledChunks;
            m_CurrentInstructionPointer = 0;
            m_CurrentScope = context.SymbolTableBuilder.CurrentScope;
            m_GlobalMemorySpace = new FastGlobalMemorySpace((context.SymbolTableBuilder.CurrentScope as BaseScope).NumberOfSymbols);
            m_CallStack = new FastMemoryStack();
            m_PoolFastMemoryFastMemory = new ObjectPoolFastMemory();
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
                m_CurrentChunk.Constants[m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24)];
            m_CurrentInstructionPointer += 4;
            return instruction;
        }

        private SrslVmInterpretResult Run()
        {

            while (true)
            {

                if (m_CurrentInstructionPointer < m_CurrentChunk.Code.Length)
                {
#if SRSL_VM_DEBUG_TRACE_EXECUTION

               /* if ( m_LastMemorySpaceDebug != m_CurrentMemorySpace )
                {
                    Console.WriteLine("New Memory Space: {0}", m_CurrentMemorySpace.Name);
                    m_LastMemorySpaceDebug = m_CurrentMemorySpace;
                }
               
                Console.Write("Stack:   ");
                if ( m_TopMostStackItem != null )
                {
                    Console.Write("[" + m_TopMostStackItem + "]");
                }
                for ( int i = 0; i < m_StackPointer; i++ )
                {
                    Console.Write("[" + m_VmStack[i] + "]");
                }

               
                Console.Write("\n");
                 */
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
                                m_StackPointer--;
                                break;
                            }
                        case SrslVmOpCodes.OpDefineModule:
                            {
                                string moduleName = ReadConstant().StringConstantValue;
                                int depth = 0;
                                m_CurrentScope = (BaseScope)m_CurrentScope.resolve(moduleName, out int moduleId, ref depth);
                                int numberOfMembers = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24);
                                m_CurrentInstructionPointer += 4;
                                FastModuleMemorySpace callSpace = new FastModuleMemorySpace("", m_GlobalMemorySpace, m_StackPointer, m_CurrentChunk, m_CurrentInstructionPointer, numberOfMembers);
                                m_GlobalMemorySpace.Modules.Add(callSpace);
                                m_CurrentChunk = CompiledChunks[moduleName];
                                m_CurrentInstructionPointer = 0;
                                m_CurrentMemorySpace = callSpace;
                                m_CallStack.Push(callSpace);
                                break;
                            }
                        case SrslVmOpCodes.OpDefineClass:
                            {
                                string className = ReadConstant().StringConstantValue;
                                SrslChunkWrapper chunkWrapper = new SrslChunkWrapper(
                                    CompiledChunks[className]);

                                m_CurrentMemorySpace.Define(
                                    DynamicVariableExtension.ToDynamicVariable(chunkWrapper));
                                break;
                            }
                        case SrslVmOpCodes.OpDefineMethod:
                            {
                                string methodName = ReadConstant().StringConstantValue;
                                SrslChunkWrapper chunkWrapper = new SrslChunkWrapper(
                                    CompiledChunks[methodName]);

                                m_CurrentMemorySpace.Define(
                                    DynamicVariableExtension.ToDynamicVariable(chunkWrapper), methodName);

                                break;
                            }
                        case SrslVmOpCodes.OpBindToFunction:
                            {
                                int numberOfArguments = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                m_FunctionArguments.Clear();
                                for (int i = 0; i < numberOfArguments; i++)
                                {
                                    if (m_TopMostStackItem != null)
                                    {
                                        m_FunctionArguments.Add(m_TopMostStackItem);
                                        m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                        continue;
                                    }
                                    m_FunctionArguments.Add(m_VmStack[m_StackPointer]);
                                    m_StackPointer--;
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpCallFunction:
                            {
                                string method = ReadConstant().StringConstantValue;
                                DynamicSrslVariable call = m_CurrentMemorySpace.Get(method);

                                if (call.ObjectData is SrslChunkWrapper function)
                                {
                                    FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();
                                    callSpace.ResetPropertiesArray(m_FunctionArguments.Count);
                                    callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                                    callSpace.CallerChunk = m_CurrentChunk;
                                    callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                                    callSpace.StackCountAtBegin = m_StackPointer;
                                    m_CurrentMemorySpace = callSpace;
                                    m_CallStack.Push(callSpace);

                                    for (int i = 0; i < m_FunctionArguments.Count; i++)
                                    {
                                        m_CurrentMemorySpace.Define(m_FunctionArguments[i]);
                                    }
                                    m_CurrentChunk = function.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;
                                }
                                if (call.ObjectData is ISrslVmCallable callable)
                                {
                                    object returnVal = callable.Call(m_FunctionArguments);

                                    if (returnVal != null)
                                    {
                                        m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(returnVal);
                                    }
                                }

                                break;
                            }
                        case SrslVmOpCodes.OpCallMemberFunction:
                            {
                                ConstantValue constant = ReadConstant();
                                if (m_TopMostStackItem == null)
                                {
                                    m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                }

                                if (m_TopMostStackItem.ObjectData is StaticWrapper wrapper)
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
                                    object returnVal = wrapper.InvokeMember(methodName, functionArguments, functionArgumentTypes);
                                    if (returnVal != null)
                                    {
                                        m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(returnVal);
                                    }
                                }
                                else if (m_TopMostStackItem.ObjectData is FastMemorySpace fastMemorySpace)
                                {
                                    string methodName = constant.StringConstantValue;
                                    DynamicSrslVariable call = fastMemorySpace.Get(methodName);

                                    m_CurrentMemorySpace = fastMemorySpace;
                                    if (call.ObjectData != null)
                                    {
                                        if (call.ObjectData is SrslChunkWrapper function)
                                        {
                                            FastMemorySpace callSpace = m_PoolFastMemoryFastMemory.Get();
                                            callSpace.ResetPropertiesArray(m_FunctionArguments.Count);
                                            callSpace.m_EnclosingSpace = m_CurrentMemorySpace;
                                            callSpace.CallerChunk = m_CurrentChunk;
                                            callSpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                                            callSpace.StackCountAtBegin = m_StackPointer;
                                            m_CurrentMemorySpace = callSpace;
                                            m_CallStack.Push(callSpace);
                                            foreach (var functionArgument in m_FunctionArguments)
                                            {
                                                m_CurrentMemorySpace.Define(functionArgument);
                                            }
                                            m_CurrentChunk = function.ChunkToWrap;
                                            m_CurrentInstructionPointer = 0;
                                        }
                                        if (call.ObjectData is ISrslVmCallable callable)
                                        {
                                            object returnVal = callable.Call(m_FunctionArguments);

                                            if (returnVal != null)
                                            {
                                                m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(returnVal);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Error expected Function, got null!");
                                    }
                                }
                                else if (m_TopMostStackItem.ObjectData is object obj)
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
                                        object returnVal = CachedMethods[callString].Invoke(m_TopMostStackItem.ObjectData, functionArguments);
                                        if (returnVal != null)
                                        {
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(returnVal);
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
                                        MethodInfo method = type.GetMethod(constant.StringConstantValue, functionArgumentTypes);
                                        if (method != null)
                                        {
                                            FastMethodInfo fastMethodInfo = new FastMethodInfo(method);
                                            CachedMethods.Add(callString, fastMethodInfo);
                                            object returnVal = fastMethodInfo.Invoke(m_TopMostStackItem.ObjectData, functionArguments);
                                            if (returnVal != null)
                                            {
                                                m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(returnVal);
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Error Function " + constant.StringConstantValue + " not found!");
                                        }
                                    }

                                }
                                break;
                            }
                        case SrslVmOpCodes.OpDefineLocalInstance:
                            {
                                int moduleIdClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int depthClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int idClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int classMemberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;

                                if (m_CurrentMemorySpace.Get(moduleIdClass, depthClass, -1, idClass).ObjectData is SrslChunkWrapper classWrapper)
                                {
                                    FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace("", m_CurrentMemorySpace, m_StackPointer, m_CurrentChunk, m_CurrentInstructionPointer, classMemberCount);
                                    m_CurrentMemorySpace.Define(DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace));
                                    m_CurrentMemorySpace = classInstanceMemorySpace;
                                    m_CurrentChunk = classWrapper.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;
                                    m_CallStack.Push(m_CurrentMemorySpace);
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpDefineLocalVar:
                            {
                                m_CurrentMemorySpace.Define(m_TopMostStackItem);

                                if (m_StackPointer > -1)
                                {
                                    if (m_StackPointer > 0)
                                    {
                                        // TODO: Allow returning value of local var definition?
                                        //m_TopMostStackItem = m_VmStack[m_StackPointer - 1];
                                        m_StackPointer--;
                                    }
                                    else
                                    {
                                        // TODO: Allow returning value of local var definition?
                                        //m_TopMostStackItem = m_VmStack[m_StackPointer];
                                    }


                                }
                                break;
                            }
                        case SrslVmOpCodes.OpDeclareLocalVar:
                            {
                                m_CurrentMemorySpace.Define(DynamicVariableExtension.ToDynamicVariable());
                                break;
                            }
                        case SrslVmOpCodes.OpSetLocalInstance:
                            {
                                int moduleIdLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int depthLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int idLocalInstance = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;

                                int moduleIdClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int depthClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int idClass = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int classMemberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                if (m_CurrentMemorySpace.Get(moduleIdClass, depthClass, -1, idClass).ObjectData is SrslChunkWrapper classWrapper)
                                {
                                    FastClassMemorySpace classInstanceMemorySpace = new FastClassMemorySpace("", m_CurrentMemorySpace, m_StackPointer, m_CurrentChunk, m_CurrentInstructionPointer, classMemberCount);
                                    classInstanceMemorySpace.m_EnclosingSpace = m_CurrentMemorySpace;
                                    classInstanceMemorySpace.CallerChunk = m_CurrentChunk;
                                    classInstanceMemorySpace.CallerIntructionPointer = m_CurrentInstructionPointer;
                                    classInstanceMemorySpace.StackCountAtBegin = m_StackPointer;
                                    m_CurrentMemorySpace.Put(moduleIdLocalInstance, depthLocalInstance, -1, idLocalInstance, DynamicVariableExtension.ToDynamicVariable(classInstanceMemorySpace));
                                    m_CurrentMemorySpace = classInstanceMemorySpace;
                                    m_CurrentChunk = classWrapper.ChunkToWrap;
                                    m_CurrentInstructionPointer = 0;

                                    m_CallStack.Push(classInstanceMemorySpace);
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpGetLocalVar:
                            {
                                m_LastGetLocalVarModuleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                m_LastGetLocalVarDepth = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                m_LastGetLocalClassId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                m_LastGetLocalVarId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                if (m_TopMostStackItem == null)
                                {
                                    m_TopMostStackItem = m_CurrentMemorySpace.Get(m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId);
                                }
                                else
                                {
                                    m_VmStack[m_StackPointer] = m_TopMostStackItem; m_StackPointer++;
                                    m_TopMostStackItem = m_CurrentMemorySpace.Get(m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId);
                                }

                                break;
                            }
                        case SrslVmOpCodes.OpGetModule:
                            {
                                int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                FastMemorySpace obj = m_GlobalMemorySpace.Modules[id];
                                if (m_TopMostStackItem == null)
                                {
                                    m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(obj);
                                }
                                else
                                {
                                    m_VmStack[m_StackPointer] = m_TopMostStackItem; m_StackPointer++;
                                    m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(obj);
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpGetMember:
                            {
                                int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                FastMemorySpace obj = (FastMemorySpace)m_VmStack[m_StackPointer].ObjectData;
                                if (m_TopMostStackItem == null)
                                {
                                    m_TopMostStackItem = obj.Get(-1, 0, -1, id);
                                }
                                else
                                {
                                    m_VmStack[m_StackPointer] = m_TopMostStackItem; m_StackPointer++;
                                    m_TopMostStackItem = obj.Get(-1, 0, -1, id);
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpSetMember:
                            {
                                int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                m_SetMember = true;
                                m_LastGetLocalVarId = id;
                                break;
                            }
                        case SrslVmOpCodes.OpSetLocalVar:
                            {

                                int moduleId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int depth = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int classId = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int id = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                m_LastGetLocalVarId = id;
                                m_LastGetLocalVarModuleId = moduleId;
                                m_LastGetLocalVarDepth = depth;
                                m_LastGetLocalClassId = classId;

                                if (m_TopMostStackItem == null)
                                {
                                    m_TopMostStackItem = m_CurrentMemorySpace.Get(moduleId, depth, classId, id);
                                }
                                else
                                {
                                    m_VmStack[m_StackPointer] = m_TopMostStackItem; m_StackPointer++;
                                    m_TopMostStackItem = m_CurrentMemorySpace.Get(moduleId, depth, classId, id);
                                }
                                SrslVmOpCodes nextOpCode = ReadInstruction();

                                m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                m_CurrentMemorySpace.Put(m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, m_TopMostStackItem);
                                break;
                            }
                        case SrslVmOpCodes.OpConstant:
                            {
                                ConstantValue constantValue = ReadConstant();

                                switch (constantValue.ConstantType)
                                {
                                    case ConstantValueType.Integer:
                                        if (m_TopMostStackItem == null)
                                        {
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(constantValue.IntegerConstantValue);
                                        }
                                        else
                                        {
                                            m_VmStack[m_StackPointer] = m_TopMostStackItem; m_StackPointer++;
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(constantValue.IntegerConstantValue);
                                        }
                                        break;

                                    case ConstantValueType.Double:
                                        if (m_TopMostStackItem == null)
                                        {
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(constantValue.DoubleConstantValue);
                                        }
                                        else
                                        {
                                            m_VmStack[m_StackPointer] = m_TopMostStackItem; m_StackPointer++;
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(constantValue.DoubleConstantValue);
                                        }
                                        break;

                                    case ConstantValueType.String:
                                        if (m_TopMostStackItem == null)
                                        {
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(constantValue.StringConstantValue);
                                        }
                                        else
                                        {
                                            m_VmStack[m_StackPointer] = m_TopMostStackItem; m_StackPointer++;
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(constantValue.StringConstantValue);
                                        }
                                        break;

                                    case ConstantValueType.Bool:
                                        if (m_TopMostStackItem == null)
                                        {
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(constantValue.BoolConstantValue);
                                        }
                                        else
                                        {
                                            m_VmStack[m_StackPointer] = m_TopMostStackItem; m_StackPointer++;
                                            m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(constantValue.BoolConstantValue);
                                        }
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpWhileLoop:
                            {
                                int jumpCodeHeaderStart = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                int jumpCodeBodyEnd = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4; ;

                                if (m_TopMostStackItem == null)
                                {
                                    m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                }



                                if (m_TopMostStackItem.DynamicType == DynamicVariableType.True)
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

                                m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                break;
                            }
                        case SrslVmOpCodes.OpJump:
                            {
                                int jumpCode = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                m_CurrentInstructionPointer = jumpCode;
                                break;
                            }
                        case SrslVmOpCodes.OpAssign:
                            {
                                if (m_SetMember)
                                {
                                    if (m_TopMostStackItem == null)
                                    {
                                        m_TopMostStackItem = m_VmStack[m_StackPointer - 1];
                                    }

                                    FastMemorySpace fastMemorySpace = (FastMemorySpace)m_TopMostStackItem.ObjectData;

                                    if (fastMemorySpace.Exist(-1, 0, -1, m_LastGetLocalVarId))
                                    {
                                        fastMemorySpace.Put(-1, 0, -1, m_LastGetLocalVarId, m_VmStack[m_StackPointer - 1]);
                                        m_StackPointer--;
                                    }
                                    else
                                    {
                                        fastMemorySpace.Define(m_VmStack[m_StackPointer - 1]);
                                        m_StackPointer--;
                                    }

                                    m_SetMember = false;
                                }
                                else
                                {
                                    if (m_TopMostStackItem == null)
                                    {
                                        m_StackPointer--;
                                        m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                    }
                                    else
                                    {
                                        m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                    }
                                    m_CurrentMemorySpace.Put(m_LastGetLocalVarModuleId, m_LastGetLocalVarDepth, m_LastGetLocalClassId, m_LastGetLocalVarId, m_TopMostStackItem);
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpNegate:
                            {
                                if (m_TopMostStackItem == null)
                                {
                                    m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                }

                                if (m_TopMostStackItem.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem.NumberData = m_TopMostStackItem.NumberData *= -1;
                                }
                                else
                                {
                                    throw new Exception("Can only negate Integers and Floating Point Numbers!");
                                }

                                break;
                            }
                        case SrslVmOpCodes.OpAffirm:
                            {
                                if (m_TopMostStackItem == null)
                                {
                                    m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                }

                                if (m_TopMostStackItem.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem.NumberData = m_TopMostStackItem.NumberData *= 1;
                                }
                                else
                                {
                                    throw new Exception("Can only negate Integers and Floating Point Numbers!");
                                }

                                break;
                            }
                        case SrslVmOpCodes.OpCompliment:
                            {
                                break;
                            }
                        case SrslVmOpCodes.OpPrefixDecrement:
                            {
                                if (m_TopMostStackItem == null)
                                {
                                    m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                }

                                if (m_TopMostStackItem.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem.NumberData = m_TopMostStackItem.NumberData -= 1;
                                }
                                else
                                {
                                    throw new Exception("Can only negate Integers and Floating Point Numbers!");
                                }

                                break;
                            }
                        case SrslVmOpCodes.OpPrefixIncrement:
                            {
                                if (m_TopMostStackItem == null)
                                {
                                    m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                }

                                if (m_TopMostStackItem.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem.NumberData = m_TopMostStackItem.NumberData += 1;
                                }
                                else
                                {
                                    throw new Exception("Can only negate Integers and Floating Point Numbers!");
                                }

                                break;
                            }
                        case SrslVmOpCodes.OpPostfixDecrement:
                            {
                                if (m_TopMostStackItem == null)
                                {
                                    m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                }

                                if (m_TopMostStackItem.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem.NumberData = m_TopMostStackItem.NumberData -= 1;
                                }
                                else
                                {
                                    throw new Exception("Can only negate Integers and Floating Point Numbers!");
                                }

                                break;
                            }
                        case SrslVmOpCodes.OpPostfixIncrement:
                            {
                                if (m_TopMostStackItem == null)
                                {
                                    m_StackPointer--; m_TopMostStackItem = m_VmStack[m_StackPointer];
                                }

                                if (m_TopMostStackItem.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem.NumberData = m_TopMostStackItem.NumberData += 1;
                                }
                                else
                                {
                                    throw new Exception("Can only negate Integers and Floating Point Numbers!");
                                }

                                break;
                            }
                        case SrslVmOpCodes.OpSmaller:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if ((valueLhs.DynamicType == 0 || valueLhs.DynamicType < DynamicVariableType.True) && (valueRhs.DynamicType == 0 || valueRhs.DynamicType < DynamicVariableType.True))
                                {
                                    m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData < valueRhs.NumberData);
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }


                                break;
                            }
                        case SrslVmOpCodes.OpSmallerEqual:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData <= valueRhs.NumberData);
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpGreater:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData > valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpGreaterEqual:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData >= valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only compare Integers and Floating Point Numbers!");
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpEqual:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData == valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only check equality with Integers and Floating Point Numbers!");
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpNotEqual:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData != valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only check equality with Integers and Floating Point Numbers!");
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpNot:
                            {
                                DynamicSrslVariable value = m_TopMostStackItem;

                                if (value.DynamicType == DynamicVariableType.True)
                                {
                                    value.DynamicType = DynamicVariableType.False;
                                }
                                else
                                {
                                    value.DynamicType = DynamicVariableType.True;
                                }

                                m_TopMostStackItem = value;
                                break;
                            }
                        case SrslVmOpCodes.OpAdd:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType == DynamicVariableType.String || valueRhs.DynamicType == DynamicVariableType.String)
                                {
                                    // TODO: String concat rules
                                    if (valueLhs.DynamicType < DynamicVariableType.True)
                                    {
                                        m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData + valueRhs.StringData));
                                    }
                                    else if (valueRhs.DynamicType < DynamicVariableType.True)
                                    {
                                        m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.StringData + valueRhs.NumberData));
                                    }
                                    else if (valueLhs.DynamicType == DynamicVariableType.String || valueRhs.DynamicType == DynamicVariableType.String)
                                    {
                                        m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.StringData + valueRhs.StringData));
                                    }
                                    else
                                    {
                                        throw new Exception("Can only concatenate Strings with Integers and Floating Point Numbers!");
                                    }
                                }
                                else if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData + valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only add Integers and Floating Point Numbers!");
                                }

                                break;
                            }

                        case SrslVmOpCodes.OpSubtract:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData - valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only subtract Integers and Floating Point Numbers!");
                                }
                                break;
                            }

                        case SrslVmOpCodes.OpMultiply:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData * valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only multiply Integers and Floating Point Numbers!");
                                }
                                break;
                            }

                        case SrslVmOpCodes.OpDivide:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData / valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only divide Integers and Floating Point Numbers!");
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpModulo:
                            {
                                DynamicSrslVariable valueRhs = m_TopMostStackItem;
                                m_StackPointer--; DynamicSrslVariable valueLhs = m_VmStack[m_StackPointer];

                                if (valueLhs.DynamicType < DynamicVariableType.True && valueRhs.DynamicType < DynamicVariableType.True)
                                {
                                    m_TopMostStackItem = (DynamicVariableExtension.ToDynamicVariable(valueLhs.NumberData % valueRhs.NumberData));
                                }
                                else
                                {
                                    throw new Exception("Can only modulo Integers and Floating Point Numbers!");
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpJumpIfFalse:
                            {
                                int offset = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;

                                if (m_TopMostStackItem.DynamicType == DynamicVariableType.False)
                                {
                                    m_CurrentInstructionPointer = offset;
                                }
                                break;
                            }
                        case SrslVmOpCodes.OpEnterBlock:
                            {
                                int memberCount = m_CurrentChunk.Code[m_CurrentInstructionPointer] | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 1] << 8) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 2] << 16) | (m_CurrentChunk.Code[m_CurrentInstructionPointer + 3] << 24); m_CurrentInstructionPointer += 4;
                                FastMemorySpace block = m_PoolFastMemoryFastMemory.Get();
                                block.ResetPropertiesArray(memberCount);
                                block.m_EnclosingSpace = m_CurrentMemorySpace;
                                block.StackCountAtBegin = m_StackPointer;
                                m_CurrentMemorySpace = block;
                                m_CallStack.Push(m_CurrentMemorySpace);
                                break;
                            }

                        case SrslVmOpCodes.OpExitBlock:
                            {
                                if (m_CurrentMemorySpace.StackCountAtBegin < m_StackPointer)
                                {
                                    int stackCounter = m_StackPointer - m_CurrentMemorySpace.StackCountAtBegin;
                                    m_StackPointer -= stackCounter;
                                }

                                m_PoolFastMemoryFastMemory.Return(m_CallStack.Pop());
                                m_CurrentMemorySpace = m_CallStack.Peek();
                                break;
                            }
                        case SrslVmOpCodes.OpKeepLastItemOnStack:
                            {
                                m_KeepLastItemOnStackToReturn = true;
                                break;
                            }
                        case SrslVmOpCodes.OpReturn:
                            {
                                if (m_CurrentMemorySpace.StackCountAtBegin < m_StackPointer)
                                {
                                    int stackCounter = m_StackPointer - m_CurrentMemorySpace.StackCountAtBegin;
                                    for (int i = 0; i < stackCounter; i++)
                                    {
                                        m_StackPointer--;
                                    }
                                }

                                if (m_CallStack.Peek().CallerChunk.Code != null &&
                                     m_CallStack.Peek().CallerChunk.Code != m_CurrentChunk.Code)
                                {
                                    m_CurrentChunk = m_CallStack.Peek().CallerChunk;
                                    m_CurrentInstructionPointer = m_CallStack.Peek().CallerIntructionPointer;
                                }
                                m_PoolFastMemoryFastMemory.Return(m_CallStack.Pop());
                                m_CurrentMemorySpace = m_CallStack.Peek();
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
                        if (m_CurrentMemorySpace.StackCountAtBegin < m_StackPointer)
                        {
                            int stackCounter = m_StackPointer - m_CurrentMemorySpace.StackCountAtBegin;
                            for (int i = 0; i < stackCounter; i++)
                            {
                                m_StackPointer--;
                            }
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
                        return SrslVmInterpretResult.InterpretOk;
                    }
                }
            }

        }

        public DynamicSrslVariable RetVal => m_TopMostStackItem;

        public void ShutdownVm()
        {
            //m_VmStack.Clear();
        }
    }

}
