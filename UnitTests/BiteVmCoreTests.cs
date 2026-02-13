// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using Bite.Runtime;
using Xunit;

namespace UnitTests
{
    public class BiteVmCoreTests
    {
        [Fact]
        public void VmCore_CreateAndReset()
        {
            var vm = new BiteVmCore();
            Assert.NotNull(vm);
            Assert.False(vm.Halted);
        }

        [Fact]
        public void VmCore_EmptyChunk()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder("empty");
            builder.Halt();
            
            var result = vm.Execute(builder.Build());
            
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.Halted);
        }

        [Fact]
        public void VmCore_Constants()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstNull();
            builder.ConstTrue();
            builder.ConstFalse();
            builder.ConstI8(42);
            builder.Halt();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
        }

        [Fact]
        public void VmCore_Arithmetic_Add()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(10);
            builder.ConstI8(32);
            builder.Add();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(42.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Arithmetic_Sub()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(50);
            builder.ConstI8(8);
            builder.Sub();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(42.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Arithmetic_Mul()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(6);
            builder.ConstI8(7);
            builder.Mul();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(42.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Arithmetic_Div()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(84);
            builder.ConstI8(2);
            builder.Div();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(42.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Arithmetic_ComplexExpression()
        {
            // (10 + 20) * (5 - 2) = 30 * 3 = 90
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(10);
            builder.ConstI8(20);
            builder.Add();
            builder.ConstI8(5);
            builder.ConstI8(2);
            builder.Sub();
            builder.Mul();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(90.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Comparison_Equal()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(42);
            builder.ConstI8(42);
            builder.Eq();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.LastResult.AsBool);
        }

        [Fact]
        public void VmCore_Comparison_NotEqual()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(42);
            builder.ConstI8(24);
            builder.Neq();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.LastResult.AsBool);
        }

        [Fact]
        public void VmCore_Comparison_LessThan()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(10);
            builder.ConstI8(20);
            builder.Lt();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.LastResult.AsBool);
        }

        [Fact]
        public void VmCore_Logical_And()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstTrue();
            builder.ConstTrue();
            builder.And();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.LastResult.AsBool);
        }

        [Fact]
        public void VmCore_Logical_Or()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstFalse();
            builder.ConstTrue();
            builder.Or();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.LastResult.AsBool);
        }

        [Fact]
        public void VmCore_Logical_Not()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstFalse();
            builder.Not();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.LastResult.AsBool);
        }

        [Fact]
        public void VmCore_Locals_LoadStore()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            byte slot0 = builder.AllocateLocal();
            byte slot1 = builder.AllocateLocal();
            
            // slot0 = 10
            builder.ConstI8(10);
            builder.StoreLocal(slot0);
            
            // slot1 = 20
            builder.ConstI8(20);
            builder.StoreLocal(slot1);
            
            // return slot0 + slot1
            builder.LoadLocal(slot0);
            builder.LoadLocal(slot1);
            builder.Add();
            builder.RetVal();
            
            var chunk = builder.Build();
            Assert.Equal(2, chunk.LocalCount);
            
            var result = vm.Execute(chunk);
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(30.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Locals_FastPath()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            byte slot0 = builder.AllocateLocal();
            byte slot1 = builder.AllocateLocal();
            
            // Use fast path opcodes
            builder.ConstI8(5);
            builder.StoreLocal0(); // slot 0
            builder.ConstI8(7);
            builder.StoreLocal1(); // slot 1
            
            builder.LoadLocal0();
            builder.LoadLocal1();
            builder.Mul();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(35.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Globals()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            // global[0] = 100
            builder.ConstI8(100);
            builder.DefGlobal(0);
            
            // return global[0] + 5
            builder.LoadGlobal(0);
            builder.ConstI8(5);
            builder.Add();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(105.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Stack_Dup()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(5);
            builder.Dup();
            builder.Mul();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(25.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Stack_Swap()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(3);  // Push 3
            builder.ConstI8(10); // Push 10
            builder.Swap();      // Swap: now 10, 3
            builder.Sub();       // 10 - 3 = 7
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(7.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Jump_Forward()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            var jump = builder.Jump();
            builder.ConstI32(999); // Should be skipped
            jump.Patch();
            builder.ConstI8(42);
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(42.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_JumpIfFalse_True()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstFalse();
            var jump = builder.JumpIfFalse();
            builder.ConstI8(1); // Should be skipped
            jump.Patch();
            builder.ConstI8(42);
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(42.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_JumpIfFalse_False()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstTrue();
            var jump = builder.JumpIfFalse();
            builder.ConstI32(999); // Should NOT be skipped
            jump.Patch();
            builder.ConstI8(42);
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            // Result is 42 because we fall through to it
            Assert.Equal(42.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Loop()
        {
            // Sum 1 to 5 using a loop
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            byte sum = builder.AllocateLocal();
            byte i = builder.AllocateLocal();
            
            // sum = 0
            builder.ConstI8(0);
            builder.StoreLocal(sum);
            
            // i = 1
            builder.ConstI8(1);
            builder.StoreLocal(i);
            
            // Loop start
            int loopStart = builder.Label();
            
            // sum = sum + i
            builder.LoadLocal(sum);
            builder.LoadLocal(i);
            builder.Add();
            builder.StoreLocal(sum);
            
            // i = i + 1
            builder.LoadLocal(i);
            builder.ConstI8(1);
            builder.Add();
            builder.StoreLocal(i);
            
            // if i < 6 goto loop (continue while i < 6)
            builder.LoadLocal(i);
            builder.ConstI8(6);
            builder.Lt();
            var exitJump = builder.JumpIfFalse();
            builder.Loop(loopStart);
            exitJump.Patch();
            
            // return sum
            builder.LoadLocal(sum);
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(15.0, vm.LastResult.AsNumber); // 1+2+3+4+5 = 15
        }

        [Fact]
        public void VmCore_Negate()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(42);
            builder.Neg();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(-42.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_Modulo()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI8(17);
            builder.ConstI8(5);
            builder.Mod();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(2.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_IsNull()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstNull();
            builder.IsNull();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.LastResult.AsBool);
        }

        [Fact]
        public void VmCore_Bitwise()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            // 6 & 3 = 2
            builder.ConstI8(6);
            builder.ConstI8(3);
            builder.BitAnd();
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(2.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_ReturnNull()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.Ret();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(vm.LastResult.IsNull);
        }

        [Fact]
        public void VmCore_ConstI32_LargeNumber()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstI32(100000);
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(100000.0, vm.LastResult.AsNumber);
        }

        [Fact]
        public void VmCore_ConstF64_Precision()
        {
            var vm = new BiteVmCore();
            var builder = new BiteChunkBuilder();
            
            builder.ConstF64(3.14159);
            builder.RetVal();
            
            var result = vm.Execute(builder.Build());
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(3.14159, vm.LastResult.AsNumber, 5);
        }
    }
}
