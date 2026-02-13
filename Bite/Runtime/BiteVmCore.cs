// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;

namespace Bite.Runtime
{
    /// <summary>
    /// Core VM execution engine for Bite.
    /// 
    /// Features:
    /// - Stack-based execution with BiteValue
    /// - Flat call frames for O(1) local access
    /// - Single-byte opcode dispatch
    /// - Contiguous memory for cache efficiency
    /// </summary>
    public sealed class BiteVmCore
    {
        // VM Configuration
        public const int DefaultStackSize = 4096;
        public const int DefaultFrameCount = 256;
        public const int DefaultGlobalCount = 256;

        // Execution State
        private BiteValue[] m_Stack;
        private CallFrame[] m_Frames;
        private BiteValue[] m_Globals;
        private BiteChunk m_CurrentChunk;
        
        // Pointers/Indices
        private int m_StackTop;
        private int m_FrameCount;
        private int m_IP; // Instruction pointer (index into current chunk's code)

        // Public state
        public bool Halted { get; private set; }
        public BiteValue LastResult { get; private set; }

        /// <summary>
        /// A call frame represents a function invocation.
        /// Stored in a flat array for O(1) access.
        /// </summary>
        private struct CallFrame
        {
            public BiteChunk Chunk;
            public int IP;
            public int StackBase; // Where this frame's locals start
            public int LocalCount;
            
            public CallFrame(BiteChunk chunk, int stackBase, int localCount)
            {
                Chunk = chunk;
                IP = 0;
                StackBase = stackBase;
                LocalCount = localCount;
            }
        }

        public BiteVmCore(int stackSize = DefaultStackSize, int maxFrames = DefaultFrameCount, int globalCount = DefaultGlobalCount)
        {
            m_Stack = new BiteValue[stackSize];
            m_Frames = new CallFrame[maxFrames];
            m_Globals = new BiteValue[globalCount];
            Reset();
        }

        /// <summary>
        /// Reset the VM to initial state.
        /// </summary>
        public void Reset()
        {
            m_StackTop = 0;
            m_FrameCount = 0;
            m_IP = 0;
            m_CurrentChunk = null;
            Halted = false;
            LastResult = BiteValue.Null;
            
            // Clear globals
            for (int i = 0; i < m_Globals.Length; i++)
                m_Globals[i] = BiteValue.Null;
        }

        /// <summary>
        /// Execute a chunk of bytecode.
        /// </summary>
        public BiteVmResult Execute(BiteChunk chunk)
        {
            Reset();
            
            // Push initial frame
            if (!PushFrame(chunk))
                return BiteVmResult.FrameOverflow;
            
            return Run();
        }

        /// <summary>
        /// Execute with arguments (for function calls).
        /// </summary>
        public BiteVmResult Execute(BiteChunk chunk, params BiteValue[] args)
        {
            Reset();
            
            // Push arguments onto stack
            foreach (var arg in args)
                Push(arg);
            
            if (!PushFrame(chunk))
                return BiteVmResult.FrameOverflow;
            
            return Run();
        }

        #region Stack Operations

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Push(BiteValue value)
        {
            if (m_StackTop >= m_Stack.Length)
                throw new BiteVmRuntimeException("Stack overflow");
            
            m_Stack[m_StackTop++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BiteValue Pop()
        {
            if (m_StackTop <= 0)
                throw new BiteVmRuntimeException("Stack underflow");
            
            return m_Stack[--m_StackTop];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PopDiscard()
        {
            if (m_StackTop <= 0)
                throw new BiteVmRuntimeException("Stack underflow");
            
            m_StackTop--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BiteValue Peek(int offset = 0)
        {
            int index = m_StackTop - 1 - offset;
            if (index < 0)
                throw new BiteVmRuntimeException("Stack access out of bounds");
            
            return m_Stack[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetTop(int offset, BiteValue value)
        {
            int index = m_StackTop - 1 - offset;
            if (index < 0)
                throw new BiteVmRuntimeException("Stack access out of bounds");
            
            m_Stack[index] = value;
        }

        #endregion

        #region Frame Management

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool PushFrame(BiteChunk chunk)
        {
            if (m_FrameCount >= m_Frames.Length)
                return false;

            // Save current frame state if we have one
            if (m_FrameCount > 0)
            {
                ref var currentFrame = ref m_Frames[m_FrameCount - 1];
                currentFrame.IP = m_IP;
            }

            // Push new frame
            int stackBase = m_StackTop;
            m_Frames[m_FrameCount] = new CallFrame(chunk, stackBase, chunk.LocalCount);
            m_CurrentChunk = chunk;
            m_IP = 0;
            m_FrameCount++;
            
            // Reserve space for locals
            m_StackTop += chunk.LocalCount;
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool PopFrame()
        {
            if (m_FrameCount <= 1)
            {
                // Returning from main - halt
                Halted = true;
                return false;
            }

            // Pop current frame
            m_FrameCount--;
            
            // Restore previous frame
            ref var prevFrame = ref m_Frames[m_FrameCount - 1];
            m_CurrentChunk = prevFrame.Chunk;
            m_IP = prevFrame.IP;
            
            return true;
        }

        private ref CallFrame GetCurrentFrame()
        {
            return ref m_Frames[m_FrameCount - 1];
        }

        #endregion

        #region Local/Global Access

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BiteValue GetLocal(int slot)
        {
            int index = GetCurrentFrame().StackBase + slot;
            if (index < 0 || index >= m_StackTop)
                throw new BiteVmRuntimeException($"Local access out of bounds: {slot}");
            
            return m_Stack[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetLocal(int slot, BiteValue value)
        {
            GetCurrentFrame();
            int index2 = GetCurrentFrame().StackBase + slot;
            if (index2 < 0 || index2 >= m_Stack.Length)
                throw new BiteVmRuntimeException($"Local access out of bounds: {slot}");
            
            m_Stack[index2] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BiteValue GetGlobal(int index)
        {
            if (index < 0 || index >= m_Globals.Length)
                throw new BiteVmRuntimeException($"Global access out of bounds: {index}");
            
            return m_Globals[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetGlobal(int index, BiteValue value)
        {
            if (index < 0 || index >= m_Globals.Length)
                throw new BiteVmRuntimeException($"Global access out of bounds: {index}");
            
            m_Globals[index] = value;
        }

        #endregion

        #region Main Execution Loop

        private BiteVmResult Run()
        {
            try
            {
                while (!Halted)
                {
                    // Safety check
                    if (m_IP < 0 || m_IP >= m_CurrentChunk.CodeSize)
                        return BiteVmResult.InvalidInstruction;

                    byte opcodeByte = m_CurrentChunk.ReadByte(m_IP);
                    m_IP++;

                    switch ((BiteOpCodes)opcodeByte)
                    {
                        // Constants
                        case BiteOpCodes.Nop: break;
                        case BiteOpCodes.ConstNull: Push(BiteValue.Null); break;
                        case BiteOpCodes.ConstTrue: Push(BiteValue.True); break;
                        case BiteOpCodes.ConstFalse: Push(BiteValue.False); break;
                        case BiteOpCodes.ConstI8: Push(new BiteValue(m_CurrentChunk.ReadI8(m_IP++))); break;
                        case BiteOpCodes.ConstI32: Push(new BiteValue(m_CurrentChunk.ReadI32(m_IP))); m_IP += 4; break;
                        case BiteOpCodes.ConstF64: Push(new BiteValue(m_CurrentChunk.GetConstant(m_CurrentChunk.ReadU16(m_IP)).AsNumber())); m_IP += 2; break;
                        case BiteOpCodes.ConstStr: Push(new BiteValue(m_CurrentChunk.GetConstant(m_CurrentChunk.ReadU16(m_IP)).AsString())); m_IP += 2; break;

                        // Stack operations
                        case BiteOpCodes.Pop: PopDiscard(); break;
                        case BiteOpCodes.PopN: { byte n = m_CurrentChunk.ReadU8(m_IP++); m_StackTop -= n; } break;
                        case BiteOpCodes.Dup: Push(Peek()); break;
                        case BiteOpCodes.DupAt: Push(Peek(m_CurrentChunk.ReadU8(m_IP++))); break;
                        case BiteOpCodes.Swap: { var a = Pop(); var b = Pop(); Push(a); Push(b); } break;
                        case BiteOpCodes.Peek: /* No-op, just for debugging */ break;

                        // Locals - fast path
                        case BiteOpCodes.LoadLocal0: Push(GetLocal(0)); break;
                        case BiteOpCodes.LoadLocal1: Push(GetLocal(1)); break;
                        case BiteOpCodes.LoadLocal2: Push(GetLocal(2)); break;
                        case BiteOpCodes.LoadLocal3: Push(GetLocal(3)); break;
                        case BiteOpCodes.StoreLocal0: SetLocal(0, Pop()); break;
                        case BiteOpCodes.StoreLocal1: SetLocal(1, Pop()); break;
                        case BiteOpCodes.StoreLocal2: SetLocal(2, Pop()); break;
                        case BiteOpCodes.StoreLocal3: SetLocal(3, Pop()); break;
                        
                        // Locals - general
                        case BiteOpCodes.LoadLocal: Push(GetLocal(m_CurrentChunk.ReadU8(m_IP++))); break;
                        case BiteOpCodes.StoreLocal: SetLocal(m_CurrentChunk.ReadU8(m_IP++), Pop()); break;

                        // Globals
                        case BiteOpCodes.LoadGlobal: Push(GetGlobal(m_CurrentChunk.ReadU16(m_IP))); m_IP += 2; break;
                        case BiteOpCodes.StoreGlobal: SetGlobal(m_CurrentChunk.ReadU16(m_IP), Pop()); m_IP += 2; break;
                        case BiteOpCodes.DefGlobal: SetGlobal(m_CurrentChunk.ReadU16(m_IP), Pop()); m_IP += 2; break;

                        // Arithmetic
                        case BiteOpCodes.Add: { var b = Pop(); var a = Pop(); Push(a + b); } break;
                        case BiteOpCodes.Sub: { var b = Pop(); var a = Pop(); Push(a - b); } break;
                        case BiteOpCodes.Mul: { var b = Pop(); var a = Pop(); Push(a * b); } break;
                        case BiteOpCodes.Div: { var b = Pop(); var a = Pop(); Push(a / b); } break;
                        case BiteOpCodes.Mod: { var b = Pop(); var a = Pop(); Push(new BiteValue(a.AsNumber % b.AsNumber)); } break;
                        case BiteOpCodes.Neg: { Push(-Pop()); } break;
                        case BiteOpCodes.Inc: { var v = Pop(); Push(new BiteValue(v.AsNumber + 1)); } break;
                        case BiteOpCodes.Dec: { var v = Pop(); Push(new BiteValue(v.AsNumber - 1)); } break;

                        // Bitwise
                        case BiteOpCodes.BitOr: { var b = Pop(); var a = Pop(); Push(new BiteValue((int)a.AsNumber | (int)b.AsNumber)); } break;
                        case BiteOpCodes.BitAnd: { var b = Pop(); var a = Pop(); Push(new BiteValue((int)a.AsNumber & (int)b.AsNumber)); } break;
                        case BiteOpCodes.BitXor: { var b = Pop(); var a = Pop(); Push(new BiteValue((int)a.AsNumber ^ (int)b.AsNumber)); } break;
                        case BiteOpCodes.BitNot: { Push(new BiteValue(~(int)Pop().AsNumber)); } break;
                        case BiteOpCodes.LShift: { var b = Pop(); var a = Pop(); Push(new BiteValue((int)a.AsNumber << (int)b.AsNumber)); } break;
                        case BiteOpCodes.RShift: { var b = Pop(); var a = Pop(); Push(new BiteValue((int)a.AsNumber >> (int)b.AsNumber)); } break;

                        // Comparison
                        case BiteOpCodes.Eq: { var b = Pop(); var a = Pop(); Push(new BiteValue(a == b)); } break;
                        case BiteOpCodes.Neq: { var b = Pop(); var a = Pop(); Push(new BiteValue(a != b)); } break;
                        case BiteOpCodes.Lt: { var b = Pop(); var a = Pop(); Push(new BiteValue(a < b)); } break;
                        case BiteOpCodes.Le: { var b = Pop(); var a = Pop(); Push(new BiteValue(a <= b)); } break;
                        case BiteOpCodes.Gt: { var b = Pop(); var a = Pop(); Push(new BiteValue(a > b)); } break;
                        case BiteOpCodes.Ge: { var b = Pop(); var a = Pop(); Push(new BiteValue(a >= b)); } break;

                        // Logical
                        case BiteOpCodes.And: 
                        { 
                            var b = Pop(); 
                            var a = Pop(); 
                            Push(new BiteValue(a.AsBool && b.AsBool)); 
                        } 
                        break;
                        case BiteOpCodes.Or: 
                        { 
                            var b = Pop(); 
                            var a = Pop(); 
                            Push(new BiteValue(a.AsBool || b.AsBool)); 
                        } 
                        break;
                        case BiteOpCodes.Not: Push(new BiteValue(!Pop().AsBool)); break;
                        case BiteOpCodes.IsNull: Push(new BiteValue(Pop().IsNull)); break;

                        // Control flow
                        case BiteOpCodes.Jump: m_IP += m_CurrentChunk.ReadI16(m_IP) + 2; break;
                        case BiteOpCodes.JumpF:
                        {
                            short offset = m_CurrentChunk.ReadI16(m_IP);
                            m_IP += 2;
                            if (!Pop().AsBool) m_IP += offset;
                        }
                        break;
                        case BiteOpCodes.JumpT:
                        {
                            short offset = m_CurrentChunk.ReadI16(m_IP);
                            m_IP += 2;
                            if (Pop().AsBool) m_IP += offset;
                        }
                        break;
                        case BiteOpCodes.JumpFKeep:
                        {
                            short offset = m_CurrentChunk.ReadI16(m_IP);
                            m_IP += 2;
                            if (!Peek().AsBool) m_IP += offset;
                        }
                        break;
                        case BiteOpCodes.JumpTKeep:
                        {
                            short offset = m_CurrentChunk.ReadI16(m_IP);
                            m_IP += 2;
                            if (Peek().AsBool) m_IP += offset;
                        }
                        break;

                        // Returns
                        case BiteOpCodes.Ret:
                            Push(BiteValue.Null);
                            if (!PopFrame()) return BiteVmResult.Success;
                            break;
                        case BiteOpCodes.RetVal:
                            LastResult = Pop();
                            Push(LastResult);
                            if (!PopFrame()) return BiteVmResult.Success;
                            break;

                        // Debug/Special
                        case BiteOpCodes.Halt:
                            Halted = true;
                            return BiteVmResult.Success;

                        case BiteOpCodes.Print:
                            Console.WriteLine(Pop());
                            break;

                        default:
                            return BiteVmResult.InvalidInstruction;
                    }
                }

                return BiteVmResult.Success;
            }
            catch (BiteVmRuntimeException ex)
            {
                return BiteVmResult.RuntimeError;
            }
            catch (Exception ex)
            {
                return BiteVmResult.RuntimeError;
            }
        }

        #endregion

        /// <summary>
        /// Get the current stack trace for debugging.
        /// </summary>
        public string GetStackTrace()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = m_FrameCount - 1; i >= 0; i--)
            {
                ref var frame = ref m_Frames[i];
                sb.AppendLine($"  at {frame.Chunk.Name} (IP: {frame.IP})");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Result codes from VM execution.
    /// </summary>
    public enum BiteVmResult
    {
        Success,
        StackOverflow,
        StackUnderflow,
        FrameOverflow,
        InvalidInstruction,
        RuntimeError
    }

}
