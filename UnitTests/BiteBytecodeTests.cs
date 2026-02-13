// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using Bite.Runtime;
using Xunit;

namespace UnitTests
{
    public class BiteBytecodeTests
    {
        [Fact]
        public void ChunkBuilder_CreateEmpty()
        {
            var builder = new BiteChunkBuilder("test");
            var chunk = builder.Build();

            Assert.Equal("test", chunk.Name);
            Assert.Equal(0, chunk.CodeSize);
            Assert.Equal(0, chunk.Constants.Count);
            Assert.Equal(0, chunk.LocalCount);
        }

        [Fact]
        public void ChunkBuilder_EmitSimpleOps()
        {
            var builder = new BiteChunkBuilder();
            builder.Nop().Pop().Dup().Swap().Add().Sub().Mul().Div();
            
            var chunk = builder.Build();
            
            Assert.Equal(8, chunk.CodeSize);
            Assert.Equal(BiteOpCodes.Nop, chunk.ReadOpCode(0));
            Assert.Equal(BiteOpCodes.Pop, chunk.ReadOpCode(1));
            Assert.Equal(BiteOpCodes.Dup, chunk.ReadOpCode(2));
            Assert.Equal(BiteOpCodes.Swap, chunk.ReadOpCode(3));
            Assert.Equal(BiteOpCodes.Add, chunk.ReadOpCode(4));
            Assert.Equal(BiteOpCodes.Sub, chunk.ReadOpCode(5));
            Assert.Equal(BiteOpCodes.Mul, chunk.ReadOpCode(6));
            Assert.Equal(BiteOpCodes.Div, chunk.ReadOpCode(7));
        }

        [Fact]
        public void ChunkBuilder_Constants()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstNull().ConstTrue().ConstFalse();
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.ConstNull, chunk.ReadOpCode(0));
            Assert.Equal(BiteOpCodes.ConstTrue, chunk.ReadOpCode(1));
            Assert.Equal(BiteOpCodes.ConstFalse, chunk.ReadOpCode(2));
        }

        [Fact]
        public void ChunkBuilder_ConstI8()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstI8(0)
                   .ConstI8(127)
                   .ConstI8(-128)
                   .ConstI8(-1);
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.ConstI8, chunk.ReadOpCode(0));
            Assert.Equal(0, chunk.ReadI8(1));
            Assert.Equal(BiteOpCodes.ConstI8, chunk.ReadOpCode(2));
            Assert.Equal(127, chunk.ReadI8(3));
            Assert.Equal(BiteOpCodes.ConstI8, chunk.ReadOpCode(4));
            Assert.Equal(-128, chunk.ReadI8(5));
            Assert.Equal(BiteOpCodes.ConstI8, chunk.ReadOpCode(6));
            Assert.Equal(-1, chunk.ReadI8(7));
        }

        [Fact]
        public void ChunkBuilder_ConstI32()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstI32(0)
                   .ConstI32(int.MaxValue)
                   .ConstI32(int.MinValue)
                   .ConstI32(-12345);
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.ConstI32, chunk.ReadOpCode(0));
            Assert.Equal(0, chunk.ReadI32(1));
            Assert.Equal(BiteOpCodes.ConstI32, chunk.ReadOpCode(5));
            Assert.Equal(int.MaxValue, chunk.ReadI32(6));
            Assert.Equal(BiteOpCodes.ConstI32, chunk.ReadOpCode(10));
            Assert.Equal(int.MinValue, chunk.ReadI32(11));
            Assert.Equal(BiteOpCodes.ConstI32, chunk.ReadOpCode(15));
            Assert.Equal(-12345, chunk.ReadI32(16));
        }

        [Fact]
        public void ChunkBuilder_ConstF64_UsesConstantPool()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstF64(3.14159);
            
            var chunk = builder.Build();
            
            // Should use ConstF64 opcode with constant pool index
            Assert.Equal(BiteOpCodes.ConstF64, chunk.ReadOpCode(0));
            ushort idx = chunk.ReadU16(1);
            Assert.True(idx < chunk.Constants.Count);
            Assert.Equal(3.14159, chunk.Constants[idx].AsNumber(), 5);
        }

        [Fact]
        public void ChunkBuilder_ConstF64_SmallIntOptimization()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstF64(42.0);  // Should use ConstI8
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.ConstI8, chunk.ReadOpCode(0));
            Assert.Equal(42, chunk.ReadI8(1));
        }

        [Fact]
        public void ChunkBuilder_ConstStr()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstStr("hello world");
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.ConstStr, chunk.ReadOpCode(0));
            ushort idx = chunk.ReadU16(1);
            Assert.True(idx < chunk.Constants.Count);
            Assert.Equal("hello world", chunk.Constants[idx].AsString());
        }

        [Fact]
        public void ChunkBuilder_LoadStoreLocal_FastPath()
        {
            var builder = new BiteChunkBuilder();
            builder.LoadLocal(0)
                   .LoadLocal(1)
                   .LoadLocal(2)
                   .LoadLocal(3)
                   .LoadLocal(4);
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.LoadLocal0, chunk.ReadOpCode(0));
            Assert.Equal(BiteOpCodes.LoadLocal1, chunk.ReadOpCode(1));
            Assert.Equal(BiteOpCodes.LoadLocal2, chunk.ReadOpCode(2));
            Assert.Equal(BiteOpCodes.LoadLocal3, chunk.ReadOpCode(3));
            Assert.Equal(BiteOpCodes.LoadLocal, chunk.ReadOpCode(4));
            Assert.Equal(4, chunk.ReadU8(5));
        }

        [Fact]
        public void ChunkBuilder_LocalManagement()
        {
            var builder = new BiteChunkBuilder();
            
            byte slot0 = builder.AllocateLocal();
            byte slot1 = builder.AllocateLocal();
            byte slot2 = builder.AllocateLocal();
            
            Assert.Equal(0, slot0);
            Assert.Equal(1, slot1);
            Assert.Equal(2, slot2);
            Assert.Equal(3, builder.LocalCount);
            
            var chunk = builder.Build();
            Assert.Equal(3, chunk.LocalCount);
        }

        [Fact]
        public void ChunkBuilder_Jump()
        {
            var builder = new BiteChunkBuilder();
            
            var jump = builder.Jump();
            builder.ConstTrue();
            jump.Patch();
            builder.ConstFalse();
            
            var chunk = builder.Build();
            
            // Jump should skip over ConstTrue
            Assert.Equal(BiteOpCodes.Jump, chunk.ReadOpCode(0));
            short offset = chunk.ReadI16(1);
            Assert.Equal(1, offset); // Jump past the 1-byte ConstTrue instruction
        }

        [Fact]
        public void ChunkBuilder_JumpIfFalse()
        {
            var builder = new BiteChunkBuilder();
            
            builder.ConstTrue();
            var jump = builder.JumpIfFalse();
            builder.ConstNull();
            jump.Patch();
            builder.ConstFalse();
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.ConstTrue, chunk.ReadOpCode(0));
            Assert.Equal(BiteOpCodes.JumpF, chunk.ReadOpCode(1));
        }

        [Fact]
        public void ChunkBuilder_Loop()
        {
            var builder = new BiteChunkBuilder();
            
            int loopStart = builder.Label();
            builder.ConstI8(1);
            builder.Loop(loopStart);
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.ConstI8, chunk.ReadOpCode(0));
            Assert.Equal(1, chunk.ReadI8(1));
            Assert.Equal(BiteOpCodes.Jump, chunk.ReadOpCode(2));
            // Jump offset should be negative (backward)
            short offset = chunk.ReadI16(3);
            Assert.True(offset < 0);
        }

        [Fact]
        public void ChunkBuilder_Call()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstNull(); // function - 1 byte
            builder.ConstI8(1);  // arg 1 - 2 bytes
            builder.ConstI8(2);  // arg 2 - 2 bytes
            builder.Call(2);     // call with 2 args - 2 bytes
            
            var chunk = builder.Build();
            
            // Offset: 0 (ConstNull) + 1 (ConstI8) + 2 (ConstI8) = 5
            Assert.Equal(BiteOpCodes.Call, chunk.ReadOpCode(5));
            Assert.Equal(2, chunk.ReadU8(6));
        }

        [Fact]
        public void ChunkBuilder_CallMethod()
        {
            var builder = new BiteChunkBuilder();
            ushort methodIdx = builder.AddConstantString("DoSomething");
            builder.CallMethod(2, methodIdx);
            
            var chunk = builder.Build();
            
            Assert.Equal(BiteOpCodes.CallMethod, chunk.ReadOpCode(0));
            Assert.Equal(2, chunk.ReadU8(1));
            Assert.Equal(methodIdx, chunk.ReadU16(2));
        }

        [Fact]
        public void Disassembler_Simple()
        {
            var builder = new BiteChunkBuilder("test");
            builder.SetLine(10);
            builder.ConstI8(42);
            builder.SetLine(11);
            builder.ConstStr("hello");
            builder.SetLine(12);
            builder.Add();
            builder.SetLine(13);
            builder.Ret();
            
            var chunk = builder.Build();
            string disasm = BiteDisassembler.Disassemble(chunk);
            
            Assert.Contains("=== test ===", disasm);
            Assert.Contains("CONSTI8", disasm);
            Assert.Contains("CONSTSTR", disasm);
            Assert.Contains("ADD", disasm);
            Assert.Contains("RET", disasm);
            Assert.Contains("42", disasm);
            Assert.Contains("\"hello\"", disasm);
        }

        [Fact]
        public void Disassembler_Jump()
        {
            var builder = new BiteChunkBuilder();
            var jump = builder.Jump();
            builder.ConstTrue();
            jump.Patch();
            
            var chunk = builder.Build();
            string disasm = BiteDisassembler.Disassemble(chunk);
            
            Assert.Contains("JUMP", disasm);
            // Jump should skip past ConstTrue (1 byte)
            // Jump instruction is 3 bytes, so offset = +1
            Assert.Contains("+1", disasm);
        }

        [Fact]
        public void ConstantPool_Deduplication_Numbers()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstF64(3.14);
            builder.ConstF64(3.14);
            builder.ConstF64(3.14);
            
            var chunk = builder.Build();
            
            // Should only have one constant
            Assert.Equal(1, chunk.Constants.Count);
            Assert.Equal(3.14, chunk.Constants[0].AsNumber(), 2);
        }

        [Fact]
        public void ConstantPool_Deduplication_Strings()
        {
            var builder = new BiteChunkBuilder();
            builder.ConstStr("shared");
            builder.ConstStr("shared");
            builder.ConstStr("shared");
            
            var chunk = builder.Build();
            
            Assert.Equal(1, chunk.Constants.Count);
            Assert.Equal("shared", chunk.Constants[0].AsString());
        }

        [Fact]
        public void InstructionSizes_Correct()
        {
            Assert.Equal(1, BiteOpCodeInfo.GetInstructionSize(BiteOpCodes.Nop));
            Assert.Equal(1, BiteOpCodeInfo.GetInstructionSize(BiteOpCodes.Add));
            Assert.Equal(2, BiteOpCodeInfo.GetInstructionSize(BiteOpCodes.LoadLocal));
            Assert.Equal(2, BiteOpCodeInfo.GetInstructionSize(BiteOpCodes.ConstI8));
            Assert.Equal(5, BiteOpCodeInfo.GetInstructionSize(BiteOpCodes.ConstI32));
            Assert.Equal(3, BiteOpCodeInfo.GetInstructionSize(BiteOpCodes.Jump));
            Assert.Equal(3, BiteOpCodeInfo.GetInstructionSize(BiteOpCodes.ConstF64));
            Assert.Equal(4, BiteOpCodeInfo.GetInstructionSize(BiteOpCodes.CallMethod));
        }

        [Fact]
        public void ChunkBuilder_ComplexExpression()
        {
            // (a + b) * (c - d)
            // where a=1, b=2, c=10, d=3
            var builder = new BiteChunkBuilder("expr");
            
            byte a = builder.AllocateLocal();
            byte b = builder.AllocateLocal();
            byte c = builder.AllocateLocal();
            byte d = builder.AllocateLocal();
            
            // Initialize locals
            builder.ConstI8(1).StoreLocal(a);
            builder.ConstI8(2).StoreLocal(b);
            builder.ConstI8(10).StoreLocal(c);
            builder.ConstI8(3).StoreLocal(d);
            
            // (a + b)
            builder.LoadLocal(a).LoadLocal(b).Add();
            
            // (c - d)
            builder.LoadLocal(c).LoadLocal(d).Sub();
            
            // Multiply
            builder.Mul();
            
            // Result should be (1+2) * (10-3) = 3 * 7 = 21
            
            var chunk = builder.Build();
            
            Assert.Equal(4, chunk.LocalCount);
            Assert.True(chunk.CodeSize > 15);
            
            // Verify we can disassemble without errors
            string disasm = BiteDisassembler.Disassemble(chunk);
            Assert.Contains("expr", disasm);
        }
    }
}
