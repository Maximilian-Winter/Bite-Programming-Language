// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Bite.Runtime
{
    /// <summary>
    /// Builder for constructing BiteChunk instances.
    /// Provides a fluent API for emitting bytecode.
    /// </summary>
    public sealed class BiteChunkBuilder
    {
        private readonly List<byte> m_Code;
        private readonly List<BiteConstant> m_Constants;
        private readonly List<int> m_Lines;
        private readonly Dictionary<double, int> m_NumberCache;
        private readonly Dictionary<string, int> m_StringCache;
        private int m_CurrentLine;
        private int m_MaxLocals;
        private int m_NextLocal;

        public string Name { get; set; }

        public BiteChunkBuilder(string name = "<anonymous>")
        {
            Name = name;
            m_Code = new List<byte>(256);
            m_Constants = new List<BiteConstant>(32);
            m_Lines = new List<int>(256);
            m_NumberCache = new Dictionary<double, int>();
            m_StringCache = new Dictionary<string, int>();
            m_CurrentLine = 1;
            m_MaxLocals = 0;
            m_NextLocal = 0;
        }

        #region Opcode Emission

        /// <summary>Set the current line number for debug info</summary>
        public BiteChunkBuilder SetLine(int line)
        {
            m_CurrentLine = line;
            return this;
        }

        /// <summary>Emit a single-byte opcode with no operands</summary>
        private void EmitOp(BiteOpCodes op)
        {
            m_Code.Add((byte)op);
            m_Lines.Add(m_CurrentLine);
        }

        /// <summary>Emit opcode with 1-byte operand</summary>
        private void EmitU8(BiteOpCodes op, byte operand)
        {
            m_Code.Add((byte)op);
            m_Lines.Add(m_CurrentLine);
            m_Code.Add(operand);
            m_Lines.Add(m_CurrentLine);
        }

        /// <summary>Emit opcode with signed 8-bit operand</summary>
        private void EmitI8(BiteOpCodes op, sbyte operand)
        {
            EmitU8(op, (byte)operand);
        }

        /// <summary>Emit opcode with 2-byte unsigned operand (little-endian)</summary>
        private void EmitU16(BiteOpCodes op, ushort operand)
        {
            m_Code.Add((byte)op);
            m_Lines.Add(m_CurrentLine);
            m_Code.Add((byte)(operand & 0xFF));
            m_Lines.Add(m_CurrentLine);
            m_Code.Add((byte)((operand >> 8) & 0xFF));
            m_Lines.Add(m_CurrentLine);
        }

        /// <summary>Emit opcode with 2-byte signed operand (little-endian)</summary>
        private void EmitI16(BiteOpCodes op, short operand)
        {
            EmitU16(op, (ushort)operand);
        }

        /// <summary>Emit opcode with 4-byte signed operand (little-endian)</summary>
        private void EmitI32(BiteOpCodes op, int operand)
        {
            m_Code.Add((byte)op);
            m_Lines.Add(m_CurrentLine);
            m_Code.Add((byte)(operand & 0xFF));
            m_Lines.Add(m_CurrentLine);
            m_Code.Add((byte)((operand >> 8) & 0xFF));
            m_Lines.Add(m_CurrentLine);
            m_Code.Add((byte)((operand >> 16) & 0xFF));
            m_Lines.Add(m_CurrentLine);
            m_Code.Add((byte)((operand >> 24) & 0xFF));
            m_Lines.Add(m_CurrentLine);
        }

        #endregion

        #region Simple Opcodes

        public BiteChunkBuilder Nop() { EmitOp(BiteOpCodes.Nop); return this; }
        public BiteChunkBuilder Pop() { EmitOp(BiteOpCodes.Pop); return this; }
        public BiteChunkBuilder Dup() { EmitOp(BiteOpCodes.Dup); return this; }
        public BiteChunkBuilder Swap() { EmitOp(BiteOpCodes.Swap); return this; }
        public BiteChunkBuilder Peek() { EmitOp(BiteOpCodes.Peek); return this; }

        public BiteChunkBuilder PopN(byte count) { EmitU8(BiteOpCodes.PopN, count); return this; }
        public BiteChunkBuilder DupAt(byte depth) { EmitU8(BiteOpCodes.DupAt, depth); return this; }
        public BiteChunkBuilder Rotate(byte n) { EmitU8(BiteOpCodes.Rotate, n); return this; }

        public BiteChunkBuilder LoadLocal(byte slot) 
        { 
            // Use fast path for slots 0-3
            switch (slot)
            {
                case 0: EmitOp(BiteOpCodes.LoadLocal0); break;
                case 1: EmitOp(BiteOpCodes.LoadLocal1); break;
                case 2: EmitOp(BiteOpCodes.LoadLocal2); break;
                case 3: EmitOp(BiteOpCodes.LoadLocal3); break;
                default: EmitU8(BiteOpCodes.LoadLocal, slot); break;
            }
            return this; 
        }

        public BiteChunkBuilder StoreLocal(byte slot) 
        { 
            switch (slot)
            {
                case 0: EmitOp(BiteOpCodes.StoreLocal0); break;
                case 1: EmitOp(BiteOpCodes.StoreLocal1); break;
                case 2: EmitOp(BiteOpCodes.StoreLocal2); break;
                case 3: EmitOp(BiteOpCodes.StoreLocal3); break;
                default: EmitU8(BiteOpCodes.StoreLocal, slot); break;
            }
            return this; 
        }

        public BiteChunkBuilder LoadGlobal(ushort idx) { EmitU16(BiteOpCodes.LoadGlobal, idx); return this; }
        public BiteChunkBuilder StoreGlobal(ushort idx) { EmitU16(BiteOpCodes.StoreGlobal, idx); return this; }
        public BiteChunkBuilder DefGlobal(ushort idx) { EmitU16(BiteOpCodes.DefGlobal, idx); return this; }

        public BiteChunkBuilder LoadField(byte offset) { EmitU8(BiteOpCodes.LoadField, offset); return this; }
        public BiteChunkBuilder StoreField(byte offset) { EmitU8(BiteOpCodes.StoreField, offset); return this; }
        public BiteChunkBuilder LoadFieldDyn(ushort nameIdx) { EmitU16(BiteOpCodes.LoadFieldDyn, nameIdx); return this; }
        public BiteChunkBuilder StoreFieldDyn(ushort nameIdx) { EmitU16(BiteOpCodes.StoreFieldDyn, nameIdx); return this; }
        public BiteChunkBuilder LoadIndex() { EmitOp(BiteOpCodes.LoadIndex); return this; }
        public BiteChunkBuilder StoreIndex() { EmitOp(BiteOpCodes.StoreIndex); return this; }

        public BiteChunkBuilder Add() { EmitOp(BiteOpCodes.Add); return this; }
        public BiteChunkBuilder Sub() { EmitOp(BiteOpCodes.Sub); return this; }
        public BiteChunkBuilder Mul() { EmitOp(BiteOpCodes.Mul); return this; }
        public BiteChunkBuilder Div() { EmitOp(BiteOpCodes.Div); return this; }
        public BiteChunkBuilder Mod() { EmitOp(BiteOpCodes.Mod); return this; }
        public BiteChunkBuilder Neg() { EmitOp(BiteOpCodes.Neg); return this; }
        public BiteChunkBuilder Inc() { EmitOp(BiteOpCodes.Inc); return this; }
        public BiteChunkBuilder Dec() { EmitOp(BiteOpCodes.Dec); return this; }

        public BiteChunkBuilder BitOr() { EmitOp(BiteOpCodes.BitOr); return this; }
        public BiteChunkBuilder BitAnd() { EmitOp(BiteOpCodes.BitAnd); return this; }
        public BiteChunkBuilder BitXor() { EmitOp(BiteOpCodes.BitXor); return this; }
        public BiteChunkBuilder BitNot() { EmitOp(BiteOpCodes.BitNot); return this; }
        public BiteChunkBuilder LShift() { EmitOp(BiteOpCodes.LShift); return this; }
        public BiteChunkBuilder RShift() { EmitOp(BiteOpCodes.RShift); return this; }

        public BiteChunkBuilder Eq() { EmitOp(BiteOpCodes.Eq); return this; }
        public BiteChunkBuilder Neq() { EmitOp(BiteOpCodes.Neq); return this; }
        public BiteChunkBuilder Lt() { EmitOp(BiteOpCodes.Lt); return this; }
        public BiteChunkBuilder Le() { EmitOp(BiteOpCodes.Le); return this; }
        public BiteChunkBuilder Gt() { EmitOp(BiteOpCodes.Gt); return this; }
        public BiteChunkBuilder Ge() { EmitOp(BiteOpCodes.Ge); return this; }
        public BiteChunkBuilder And() { EmitOp(BiteOpCodes.And); return this; }
        public BiteChunkBuilder Or() { EmitOp(BiteOpCodes.Or); return this; }
        public BiteChunkBuilder Not() { EmitOp(BiteOpCodes.Not); return this; }
        public BiteChunkBuilder IsNull() { EmitOp(BiteOpCodes.IsNull); return this; }

        public BiteChunkBuilder Ret() { EmitOp(BiteOpCodes.Ret); return this; }
        public BiteChunkBuilder RetVal() { EmitOp(BiteOpCodes.RetVal); return this; }
        public BiteChunkBuilder Call(byte argCount) { EmitU8(BiteOpCodes.Call, argCount); return this; }
        public BiteChunkBuilder CallMethod(byte argCount, ushort nameIdx) 
        { 
            EmitU8(BiteOpCodes.CallMethod, argCount);
            EmitU16Raw(nameIdx);
            return this; 
        }
        public BiteChunkBuilder CallExt(byte argCount) { EmitU8(BiteOpCodes.CallExt, argCount); return this; }

        public BiteChunkBuilder New(ushort classIdx) { EmitU16(BiteOpCodes.New, classIdx); return this; }
        public BiteChunkBuilder NewArray(ushort sizeIdx) { EmitU16(BiteOpCodes.NewArray, sizeIdx); return this; }
        public BiteChunkBuilder NewArrayLit(byte size) { EmitU8(BiteOpCodes.NewArrayLit, size); return this; }

        public BiteChunkBuilder IsType(byte typeIdx) { EmitU8(BiteOpCodes.IsType, typeIdx); return this; }
        public BiteChunkBuilder Cast(byte typeIdx) { EmitU8(BiteOpCodes.Cast, typeIdx); return this; }
        public BiteChunkBuilder ToStr() { EmitOp(BiteOpCodes.ToStr); return this; }
        public BiteChunkBuilder ToNum() { EmitOp(BiteOpCodes.ToNum); return this; }

        public BiteChunkBuilder Break() { EmitOp(BiteOpCodes.Break); return this; }
        public BiteChunkBuilder Print() { EmitOp(BiteOpCodes.Print); return this; }
        public BiteChunkBuilder Halt() { EmitOp(BiteOpCodes.Halt); return this; }

        #endregion

        #region Constants

        public BiteChunkBuilder ConstNull() { EmitOp(BiteOpCodes.ConstNull); return this; }
        public BiteChunkBuilder ConstTrue() { EmitOp(BiteOpCodes.ConstTrue); return this; }
        public BiteChunkBuilder ConstFalse() { EmitOp(BiteOpCodes.ConstFalse); return this; }

        public BiteChunkBuilder ConstI8(sbyte value)
        {
            EmitI8(BiteOpCodes.ConstI8, value);
            return this;
        }

        public BiteChunkBuilder ConstI32(int value)
        {
            EmitI32(BiteOpCodes.ConstI32, value);
            return this;
        }

        public BiteChunkBuilder ConstF64(double value)
        {
            // Try to use small int optimization
            if (value >= -128 && value <= 127 && value == Math.Truncate(value))
            {
                return ConstI8((sbyte)value);
            }
            if (value >= int.MinValue && value <= int.MaxValue && value == Math.Truncate(value))
            {
                return ConstI32((int)value);
            }
            
            // Use constant pool
            ushort idx = AddConstantNumber(value);
            EmitU16(BiteOpCodes.ConstF64, idx);
            return this;
        }

        public BiteChunkBuilder ConstStr(string value)
        {
            ushort idx = AddConstantString(value);
            EmitU16(BiteOpCodes.ConstStr, idx);
            return this;
        }

        public BiteChunkBuilder LoadConstant(BiteValue value)
        {
            if (value.IsNull) return ConstNull();
            if (value.IsBool)
            {
                return value.AsBool ? ConstTrue() : ConstFalse();
            }
            if (value.IsNumber)
            {
                double num = value.AsNumber;
                return ConstF64(num);
            }
            if (value.IsString)
            {
                return ConstStr(value.AsString);
            }
            
            // Fallback for objects - add to constant pool
            ushort idx = AddConstantObject(value.AsObject);
            EmitU16(BiteOpCodes.Const, idx);
            return this;
        }

        #endregion

        #region Jumps

        /// <summary>
        /// Emits a jump instruction and returns a handle to patch the offset later.
        /// </summary>
        public JumpHandle Jump(BiteOpCodes jumpOp = BiteOpCodes.Jump)
        {
            int offset = m_Code.Count;
            EmitI16(jumpOp, 0); // Placeholder
            return new JumpHandle(this, offset);
        }

        public JumpHandle JumpIfFalse() => Jump(BiteOpCodes.JumpF);
        public JumpHandle JumpIfTrue() => Jump(BiteOpCodes.JumpT);
        public JumpHandle JumpIfFalseKeep() => Jump(BiteOpCodes.JumpFKeep);
        public JumpHandle JumpIfTrueKeep() => Jump(BiteOpCodes.JumpTKeep);

        // Direct fast-path methods for tests
        public BiteChunkBuilder LoadLocal0() { EmitOp(BiteOpCodes.LoadLocal0); return this; }
        public BiteChunkBuilder LoadLocal1() { EmitOp(BiteOpCodes.LoadLocal1); return this; }
        public BiteChunkBuilder LoadLocal2() { EmitOp(BiteOpCodes.LoadLocal2); return this; }
        public BiteChunkBuilder LoadLocal3() { EmitOp(BiteOpCodes.LoadLocal3); return this; }
        public BiteChunkBuilder StoreLocal0() { EmitOp(BiteOpCodes.StoreLocal0); return this; }
        public BiteChunkBuilder StoreLocal1() { EmitOp(BiteOpCodes.StoreLocal1); return this; }
        public BiteChunkBuilder StoreLocal2() { EmitOp(BiteOpCodes.StoreLocal2); return this; }
        public BiteChunkBuilder StoreLocal3() { EmitOp(BiteOpCodes.StoreLocal3); return this; }

        /// <summary>
        /// Patch a jump to target the current position.
        /// </summary>
        public void PatchJump(JumpHandle handle)
        {
            int jumpOffset = handle.Offset;
            int targetOffset = m_Code.Count;
            int jumpDistance = targetOffset - (jumpOffset + 3); // +3 because IP will be after the instruction

            if (jumpDistance < short.MinValue || jumpDistance > short.MaxValue)
            {
                throw new InvalidOperationException($"Jump too far: {jumpDistance} bytes");
            }

            // Patch the 2-byte offset (little-endian)
            m_Code[jumpOffset + 1] = (byte)(jumpDistance & 0xFF);
            m_Code[jumpOffset + 2] = (byte)((jumpDistance >> 8) & 0xFF);
        }

        /// <summary>
        /// Emit a backward jump to a label position.
        /// </summary>
        public BiteChunkBuilder Loop(int labelOffset)
        {
            int currentOffset = m_Code.Count;
            int jumpDistance = labelOffset - (currentOffset + 3);
            
            if (jumpDistance < short.MinValue)
            {
                throw new InvalidOperationException("Loop jump too far backward");
            }

            EmitI16(BiteOpCodes.Jump, (short)jumpDistance);
            return this;
        }

        #endregion

        #region Local Variable Management

        /// <summary>Allocate a new local variable slot</summary>
        public byte AllocateLocal()
        {
            if (m_NextLocal > 255)
                throw new InvalidOperationException("Too many local variables");
            
            byte slot = (byte)m_NextLocal++;
            if (m_NextLocal > m_MaxLocals)
                m_MaxLocals = m_NextLocal;
            return slot;
        }

        /// <summary>Get the number of locals allocated</summary>
        public int LocalCount => m_NextLocal;

        /// <summary>Get the maximum number of locals needed</summary>
        public int MaxLocals => m_MaxLocals;

        /// <summary>Release locals back to a previous count</summary>
        public void ReleaseLocals(int count)
        {
            if (count < 0 || count > m_NextLocal)
                throw new ArgumentOutOfRangeException(nameof(count));
            m_NextLocal = count;
        }

        #endregion

        #region Constant Pool

        public ushort AddConstantNumber(double value)
        {
            if (m_NumberCache.TryGetValue(value, out int idx))
            {
                return (ushort)idx;
            }

            idx = m_Constants.Count;
            if (idx > ushort.MaxValue)
                throw new InvalidOperationException("Too many constants");
            
            m_Constants.Add(BiteConstant.Number(value));
            m_NumberCache[value] = idx;
            return (ushort)idx;
        }

        public ushort AddConstantString(string value)
        {
            if (value == null) value = string.Empty;
            if (m_StringCache.TryGetValue(value, out int idx))
            {
                return (ushort)idx;
            }

            idx = m_Constants.Count;
            if (idx > ushort.MaxValue)
                throw new InvalidOperationException("Too many constants");
            
            m_Constants.Add(BiteConstant.String(value));
            m_StringCache[value] = idx;
            return (ushort)idx;
        }

        public ushort AddConstantObject(object value)
        {
            int idx = m_Constants.Count;
            if (idx > ushort.MaxValue)
                throw new InvalidOperationException("Too many constants");
            
            m_Constants.Add(BiteConstant.Object(value));
            return (ushort)idx;
        }

        public int ConstantCount => m_Constants.Count;

        #endregion

        #region Helpers

        private void EmitU16Raw(ushort value)
        {
            m_Code.Add((byte)(value & 0xFF));
            m_Lines.Add(m_CurrentLine);
            m_Code.Add((byte)((value >> 8) & 0xFF));
            m_Lines.Add(m_CurrentLine);
        }

        /// <summary>Get current bytecode offset</summary>
        public int CurrentOffset => m_Code.Count;

        /// <summary>Create a label at the current position</summary>
        public int Label() => m_Code.Count;

        #endregion

        #region Build

        public BiteChunk Build()
        {
            return new BiteChunk(
                m_Code.ToArray(),
                m_Constants.ToArray(),
                m_Lines.ToArray(),
                Name,
                m_MaxLocals
            );
        }

        #endregion
    }

    /// <summary>
    /// Handle for patching jump instructions.
    /// </summary>
    public readonly struct JumpHandle
    {
        public BiteChunkBuilder Builder { get; }
        public int Offset { get; }

        public JumpHandle(BiteChunkBuilder builder, int offset)
        {
            Builder = builder;
            Offset = offset;
        }

        public void Patch()
        {
            Builder.PatchJump(this);
        }
    }
}
