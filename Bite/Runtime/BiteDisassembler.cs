// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;

namespace Bite.Runtime
{
    /// <summary>
    /// Disassembler for Bite bytecode.
    /// Produces human-readable assembly-like output.
    /// </summary>
    public static class BiteDisassembler
    {
        public static string Disassemble(BiteChunk chunk)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {chunk.Name} ===");
            sb.AppendLine($"Locals: {chunk.LocalCount}, Constants: {chunk.Constants.Count}");
            sb.AppendLine();

            int offset = 0;
            while (offset < chunk.CodeSize)
            {
                offset = DisassembleInstruction(chunk, offset, sb);
            }

            return sb.ToString();
        }

        public static int DisassembleInstruction(BiteChunk chunk, int offset, StringBuilder sb = null)
        {
            if (sb == null) sb = new StringBuilder();
            
            int line = chunk.GetLine(offset);
            string lineStr = line >= 0 ? line.ToString().PadLeft(4) : "    ";
            
            sb.Append($"{offset:X4} {lineStr} ");

            byte opcodeByte = chunk.ReadByte(offset);
            if (!Enum.IsDefined(typeof(BiteOpCodes), opcodeByte))
            {
                sb.AppendLine($"Unknown opcode: {opcodeByte:X2}");
                return offset + 1;
            }

            var opcode = (BiteOpCodes)opcodeByte;
            string name = BiteOpCodeInfo.GetName(opcode);
            int size = BiteOpCodeInfo.GetInstructionSize(opcode);

            switch (opcode)
            {
                // Simple 1-byte instructions
                case BiteOpCodes.Nop:
                case BiteOpCodes.ConstNull:
                case BiteOpCodes.ConstTrue:
                case BiteOpCodes.ConstFalse:
                case BiteOpCodes.Pop:
                case BiteOpCodes.Dup:
                case BiteOpCodes.Swap:
                case BiteOpCodes.Peek:
                case BiteOpCodes.LoadLocal0:
                case BiteOpCodes.LoadLocal1:
                case BiteOpCodes.LoadLocal2:
                case BiteOpCodes.LoadLocal3:
                case BiteOpCodes.StoreLocal0:
                case BiteOpCodes.StoreLocal1:
                case BiteOpCodes.StoreLocal2:
                case BiteOpCodes.StoreLocal3:
                case BiteOpCodes.Add:
                case BiteOpCodes.Sub:
                case BiteOpCodes.Mul:
                case BiteOpCodes.Div:
                case BiteOpCodes.Mod:
                case BiteOpCodes.Neg:
                case BiteOpCodes.Inc:
                case BiteOpCodes.Dec:
                case BiteOpCodes.BitOr:
                case BiteOpCodes.BitAnd:
                case BiteOpCodes.BitXor:
                case BiteOpCodes.BitNot:
                case BiteOpCodes.LShift:
                case BiteOpCodes.RShift:
                case BiteOpCodes.Eq:
                case BiteOpCodes.Neq:
                case BiteOpCodes.Lt:
                case BiteOpCodes.Le:
                case BiteOpCodes.Gt:
                case BiteOpCodes.Ge:
                case BiteOpCodes.And:
                case BiteOpCodes.Or:
                case BiteOpCodes.Not:
                case BiteOpCodes.IsNull:
                case BiteOpCodes.LoadIndex:
                case BiteOpCodes.StoreIndex:
                case BiteOpCodes.Ret:
                case BiteOpCodes.RetVal:
                case BiteOpCodes.ToStr:
                case BiteOpCodes.ToNum:
                case BiteOpCodes.Break:
                case BiteOpCodes.Print:
                case BiteOpCodes.Halt:
                    SimpleInstruction(name, sb);
                    break;

                // 8-bit operand
                case BiteOpCodes.PopN:
                case BiteOpCodes.DupAt:
                case BiteOpCodes.Rotate:
                case BiteOpCodes.LoadLocal:
                case BiteOpCodes.StoreLocal:
                case BiteOpCodes.LoadField:
                case BiteOpCodes.StoreField:
                case BiteOpCodes.IsType:
                case BiteOpCodes.Cast:
                case BiteOpCodes.NewArrayLit:
                    ByteOperandInstruction(chunk, offset, name, sb);
                    break;

                // 16-bit operand
                case BiteOpCodes.Const:
                case BiteOpCodes.ConstF64:
                    ConstantInstruction(chunk, offset, name, sb);
                    break;

                case BiteOpCodes.ConstStr:
                    StringConstantInstruction(chunk, offset, name, sb);
                    break;

                case BiteOpCodes.LoadGlobal:
                case BiteOpCodes.StoreGlobal:
                case BiteOpCodes.DefGlobal:
                case BiteOpCodes.LoadFieldDyn:
                case BiteOpCodes.StoreFieldDyn:
                case BiteOpCodes.New:
                case BiteOpCodes.NewArray:
                    U16OperandInstruction(chunk, offset, name, sb);
                    break;

                // Signed 8-bit
                case BiteOpCodes.ConstI8:
                    I8OperandInstruction(chunk, offset, name, sb);
                    break;

                // Signed 32-bit
                case BiteOpCodes.ConstI32:
                    I32OperandInstruction(chunk, offset, name, sb);
                    break;

                // Jump instructions (signed 16-bit offset)
                case BiteOpCodes.Jump:
                case BiteOpCodes.JumpF:
                case BiteOpCodes.JumpT:
                case BiteOpCodes.JumpFKeep:
                case BiteOpCodes.JumpTKeep:
                    JumpInstruction(chunk, offset, name, sb);
                    break;

                // Call instructions
                case BiteOpCodes.Call:
                case BiteOpCodes.CallExt:
                    ByteOperandInstruction(chunk, offset, name, sb);
                    break;

                case BiteOpCodes.CallMethod:
                    CallMethodInstruction(chunk, offset, name, sb);
                    break;

                default:
                    sb.AppendLine($"{name} (unknown format)");
                    break;
            }

            return offset + size;
        }

        private static void SimpleInstruction(string name, StringBuilder sb)
        {
            sb.AppendLine(name);
        }

        private static void ByteOperandInstruction(BiteChunk chunk, int offset, string name, StringBuilder sb)
        {
            byte operand = chunk.ReadU8(offset + 1);
            sb.AppendLine($"{name,-12} {operand}");
        }

        private static void U16OperandInstruction(BiteChunk chunk, int offset, string name, StringBuilder sb)
        {
            ushort operand = chunk.ReadU16(offset + 1);
            sb.AppendLine($"{name,-12} {operand}");
        }

        private static void I8OperandInstruction(BiteChunk chunk, int offset, string name, StringBuilder sb)
        {
            sbyte operand = chunk.ReadI8(offset + 1);
            sb.AppendLine($"{name,-12} {operand}");
        }

        private static void I32OperandInstruction(BiteChunk chunk, int offset, string name, StringBuilder sb)
        {
            int operand = chunk.ReadI32(offset + 1);
            sb.AppendLine($"{name,-12} {operand}");
        }

        private static void ConstantInstruction(BiteChunk chunk, int offset, string name, StringBuilder sb)
        {
            ushort idx = chunk.ReadU16(offset + 1);
            var constant = idx < chunk.Constants.Count ? chunk.Constants[idx].ToString() : "?";
            sb.AppendLine($"{name,-12} #{idx,-4} ({constant})");
        }

        private static void StringConstantInstruction(BiteChunk chunk, int offset, string name, StringBuilder sb)
        {
            ushort idx = chunk.ReadU16(offset + 1);
            var constant = idx < chunk.Constants.Count ? chunk.Constants[idx].ToString() : "?";
            sb.AppendLine($"{name,-12} #{idx,-4} ({constant})");
        }

        private static void JumpInstruction(BiteChunk chunk, int offset, string name, StringBuilder sb)
        {
            short jumpOffset = chunk.ReadI16(offset + 1);
            int target = offset + 3 + jumpOffset;
            sb.AppendLine($"{name,-12} {jumpOffset:+0;-0} -> {target:X4}");
        }

        private static void CallMethodInstruction(BiteChunk chunk, int offset, string name, StringBuilder sb)
        {
            byte argCount = chunk.ReadU8(offset + 1);
            ushort nameIdx = chunk.ReadU16(offset + 2);
            var methodName = nameIdx < chunk.Constants.Count ? chunk.Constants[nameIdx].ToString() : "?";
            sb.AppendLine($"{name,-12} {argCount} #{nameIdx} ({methodName})");
        }
    }
}
