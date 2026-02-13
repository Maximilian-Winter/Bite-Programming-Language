// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

namespace Bite.Runtime
{
    /// <summary>
    /// Low-level opcodes for the Bite VM.
    /// 
    /// Design principles:
    /// - Single-byte opcodes for fast dispatch
    /// - Fixed-size operands where possible for simple decoding
    /// - Stack-based operations (no implicit memory access)
    /// - Explicit control flow (no high-level loop constructs)
    /// - Flat addressing for locals (single byte slot index)
    /// </summary>
    public enum BiteOpCodes : byte
    {
        // =========================================================================
        // NOP (0x00)
        // =========================================================================
        
        /// <summary>No operation. 1 byte.</summary>
        Nop = 0x00,

        // =========================================================================
        // Constants (0x01 - 0x0F)
        // =========================================================================
        
        /// <summary>Push constant from pool [Idx: u16]. 3 bytes.</summary>
        Const = 0x01,
        
        /// <summary>Push null. 1 byte.</summary>
        ConstNull = 0x02,
        
        /// <summary>Push true. 1 byte.</summary>
        ConstTrue = 0x03,
        
        /// <summary>Push false. 1 byte.</summary>
        ConstFalse = 0x04,
        
        /// <summary>Push small integer [-128..127] [Val: i8]. 2 bytes.</summary>
        ConstI8 = 0x05,
        
        /// <summary>Push integer [Val: i32]. 5 bytes.</summary>
        ConstI32 = 0x06,
        
        /// <summary>Push double from pool [Idx: u16]. 3 bytes.</summary>
        ConstF64 = 0x07,
        
        /// <summary>Push string from pool [Idx: u16]. 3 bytes.</summary>
        ConstStr = 0x08,

        // =========================================================================
        // Stack Operations (0x10 - 0x1F)
        // =========================================================================
        
        /// <summary>Pop top value (discard). 1 byte.</summary>
        Pop = 0x10,
        
        /// <summary>Pop N values [Count: u8]. 2 bytes.</summary>
        PopN = 0x11,
        
        /// <summary>Duplicate top value. 1 byte.</summary>
        Dup = 0x12,
        
        /// <summary>Duplicate value at depth [Depth: u8]. 2 bytes.</summary>
        DupAt = 0x13,
        
        /// <summary>Swap top two values. 1 byte.</summary>
        Swap = 0x14,
        
        /// <summary>Rotate top N values [N: u8]. 2 bytes.</summary>
        Rotate = 0x15,
        
        /// <summary>Push top value (peek, don't remove). 1 byte.</summary>
        Peek = 0x16,

        // =========================================================================
        // Local Variables (0x20 - 0x2F)
        // =========================================================================
        
        /// <summary>Load local [Slot: u8]. 2 bytes. Pushes local onto stack.</summary>
        LoadLocal = 0x20,
        
        /// <summary>Store to local [Slot: u8]. 2 bytes. Pops value from stack.</summary>
        StoreLocal = 0x21,
        
        /// <summary>Load local 0 (fast path). 1 byte.</summary>
        LoadLocal0 = 0x22,
        
        /// <summary>Load local 1 (fast path). 1 byte.</summary>
        LoadLocal1 = 0x23,
        
        /// <summary>Load local 2 (fast path). 1 byte.</summary>
        LoadLocal2 = 0x24,
        
        /// <summary>Load local 3 (fast path). 1 byte.</summary>
        LoadLocal3 = 0x25,
        
        /// <summary>Store to local 0 (fast path). 1 byte.</summary>
        StoreLocal0 = 0x26,
        
        /// <summary>Store to local 1 (fast path). 1 byte.</summary>
        StoreLocal1 = 0x27,
        
        /// <summary>Store to local 2 (fast path). 1 byte.</summary>
        StoreLocal2 = 0x28,
        
        /// <summary>Store to local 3 (fast path). 1 byte.</summary>
        StoreLocal3 = 0x29,

        // =========================================================================
        // Global Variables (0x30 - 0x3F)
        // =========================================================================
        
        /// <summary>Load global [Idx: u16]. 3 bytes.</summary>
        LoadGlobal = 0x30,
        
        /// <summary>Store to global [Idx: u16]. 3 bytes.</summary>
        StoreGlobal = 0x31,
        
        /// <summary>Define new global [Idx: u16]. 3 bytes.</summary>
        DefGlobal = 0x32,

        // =========================================================================
        // Object/Field Access (0x40 - 0x4F)
        // =========================================================================
        
        /// <summary>Load field [Offset: u8]. 2 bytes. Pops object, pushes field.</summary>
        LoadField = 0x40,
        
        /// <summary>Store field [Offset: u8]. 2 bytes. Pops value and object.</summary>
        StoreField = 0x41,
        
        /// <summary>Load field by name [NameIdx: u16]. 3 bytes.</summary>
        LoadFieldDyn = 0x42,
        
        /// <summary>Store field by name [NameIdx: u16]. 3 bytes.</summary>
        StoreFieldDyn = 0x43,
        
        /// <summary>Load array/list element. 1 byte. Pops index and array.</summary>
        LoadIndex = 0x44,
        
        /// <summary>Store array/list element. 1 byte. Pops value, index, array.</summary>
        StoreIndex = 0x45,

        // =========================================================================
        // Arithmetic (0x50 - 0x5F)
        // =========================================================================
        
        /// <summary>Add top two values. 1 byte. Pops two, pushes result.</summary>
        Add = 0x50,
        
        /// <summary>Subtract. 1 byte. Pops b, a, pushes a-b.</summary>
        Sub = 0x51,
        
        /// <summary>Multiply. 1 byte. Pops two, pushes result.</summary>
        Mul = 0x52,
        
        /// <summary>Divide. 1 byte. Pops b, a, pushes a/b.</summary>
        Div = 0x53,
        
        /// <summary>Modulo. 1 byte. Pops b, a, pushes a%b.</summary>
        Mod = 0x54,
        
        /// <summary>Negate. 1 byte. Pops one, pushes -value.</summary>
        Neg = 0x55,
        
        /// <summary>Increment by 1. 1 byte.</summary>
        Inc = 0x56,
        
        /// <summary>Decrement by 1. 1 byte.</summary>
        Dec = 0x57,
        
        /// <summary>Bitwise OR. 1 byte.</summary>
        BitOr = 0x58,
        
        /// <summary>Bitwise AND. 1 byte.</summary>
        BitAnd = 0x59,
        
        /// <summary>Bitwise XOR. 1 byte.</summary>
        BitXor = 0x5A,
        
        /// <summary>Bitwise NOT. 1 byte.</summary>
        BitNot = 0x5B,
        
        /// <summary>Left shift. 1 byte. Pops b, a, pushes a&lt;&lt;b.</summary>
        LShift = 0x5C,
        
        /// <summary>Right shift. 1 byte. Pops b, a, pushes a&gt;&gt;b.</summary>
        RShift = 0x5D,

        // =========================================================================
        // Comparison (0x60 - 0x6F)
        // =========================================================================
        
        /// <summary>Equal. 1 byte. Pops two, pushes bool.</summary>
        Eq = 0x60,
        
        /// <summary>Not equal. 1 byte. Pops two, pushes bool.</summary>
        Neq = 0x61,
        
        /// <summary>Less than. 1 byte. Pops b, a, pushes a&lt;b.</summary>
        Lt = 0x62,
        
        /// <summary>Less or equal. 1 byte. Pops b, a, pushes a&lt;=b.</summary>
        Le = 0x63,
        
        /// <summary>Greater than. 1 byte. Pops b, a, pushes a&gt;b.</summary>
        Gt = 0x64,
        
        /// <summary>Greater or equal. 1 byte. Pops b, a, pushes a&gt;=b.</summary>
        Ge = 0x65,
        
        /// <summary>Logical AND. 1 byte. Short-circuit.</summary>
        And = 0x66,
        
        /// <summary>Logical OR. 1 byte. Short-circuit.</summary>
        Or = 0x67,
        
        /// <summary>Logical NOT. 1 byte.</summary>
        Not = 0x68,
        
        /// <summary>IsNull check. 1 byte. Pops one, pushes bool.</summary>
        IsNull = 0x69,

        // =========================================================================
        // Control Flow (0x70 - 0x7F)
        // =========================================================================
        
        /// <summary>Jump [Offset: i16]. 3 bytes.</summary>
        Jump = 0x70,
        
        /// <summary>Jump if false [Offset: i16]. 3 bytes. Pops condition.</summary>
        JumpF = 0x71,
        
        /// <summary>Jump if true [Offset: i16]. 3 bytes. Pops condition.</summary>
        JumpT = 0x72,
        
        /// <summary>Jump if false (keep value) [Offset: i16]. 3 bytes.</summary>
        JumpFKeep = 0x73,
        
        /// <summary>Jump if true (keep value) [Offset: i16]. 3 bytes.</summary>
        JumpTKeep = 0x74,
        
        /// <summary>Call function [ArgCount: u8]. 2 bytes. Pops args + function.</summary>
        Call = 0x75,
        
        /// <summary>Call method [ArgCount: u8, NameIdx: u16]. 4 bytes.</summary>
        CallMethod = 0x76,
        
        /// <summary>Call external (C# interop) [ArgCount: u8]. 2 bytes.</summary>
        CallExt = 0x77,
        
        /// <summary>Return from function. 1 byte.</summary>
        Ret = 0x78,
        
        /// <summary>Return with value. 1 byte. Pops return value.</summary>
        RetVal = 0x79,

        // =========================================================================
        // Object Creation (0x80 - 0x8F)
        // =========================================================================
        
        /// <summary>Create new object [ClassIdx: u16]. 3 bytes. Pushes instance.</summary>
        New = 0x80,
        
        /// <summary>Create array [SizeIdx: u16]. 3 bytes. Pops size, pushes array.</summary>
        NewArray = 0x81,
        
        /// <summary>Create array with literal size [Size: u8]. 2 bytes.</summary>
        NewArrayLit = 0x82,

        // =========================================================================
        // Type Operations (0x90 - 0x9F)
        // =========================================================================
        
        /// <summary>Type check [TypeIdx: u8]. 2 bytes. Pushes bool.</summary>
        IsType = 0x90,
        
        /// <summary>Cast [TypeIdx: u8]. 2 bytes. May throw.</summary>
        Cast = 0x91,
        
        /// <summary>Convert to string. 1 byte.</summary>
        ToStr = 0x92,
        
        /// <summary>Convert to number. 1 byte.</summary>
        ToNum = 0x93,

        // =========================================================================
        // Debug/Special (0xF0 - 0xFF)
        // =========================================================================
        
        /// <summary>Breakpoint. 1 byte.</summary>
        Break = 0xF0,
        
        /// <summary>Print top of stack (debug). 1 byte.</summary>
        Print = 0xF1,
        
        /// <summary>Halt execution. 1 byte.</summary>
        Halt = 0xFF,
    }

    /// <summary>
    /// Instruction format metadata for each opcode.
    /// </summary>
    public static class BiteOpCodeInfo
    {
        public static int GetOperandCount(BiteOpCodes op)
        {
            switch (op)
            {
                // 0 operands
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
                case BiteOpCodes.Ret:
                case BiteOpCodes.RetVal:
                case BiteOpCodes.LoadIndex:
                case BiteOpCodes.StoreIndex:
                case BiteOpCodes.ToStr:
                case BiteOpCodes.ToNum:
                case BiteOpCodes.Break:
                case BiteOpCodes.Print:
                case BiteOpCodes.Halt:
                    return 0;

                // 1 byte operand
                case BiteOpCodes.PopN:
                case BiteOpCodes.DupAt:
                case BiteOpCodes.Rotate:
                case BiteOpCodes.LoadLocal:
                case BiteOpCodes.StoreLocal:
                case BiteOpCodes.LoadField:
                case BiteOpCodes.StoreField:
                case BiteOpCodes.ConstI8:
                case BiteOpCodes.Call:
                case BiteOpCodes.CallExt:
                case BiteOpCodes.NewArrayLit:
                case BiteOpCodes.IsType:
                case BiteOpCodes.Cast:
                    return 1;

                // 2 byte operands
                case BiteOpCodes.Const:
                case BiteOpCodes.ConstF64:
                case BiteOpCodes.ConstStr:
                case BiteOpCodes.LoadGlobal:
                case BiteOpCodes.StoreGlobal:
                case BiteOpCodes.DefGlobal:
                case BiteOpCodes.LoadFieldDyn:
                case BiteOpCodes.StoreFieldDyn:
                case BiteOpCodes.Jump:
                case BiteOpCodes.JumpF:
                case BiteOpCodes.JumpT:
                case BiteOpCodes.JumpFKeep:
                case BiteOpCodes.JumpTKeep:
                case BiteOpCodes.New:
                case BiteOpCodes.NewArray:
                    return 2;

                // 4 byte operands
                case BiteOpCodes.ConstI32:
                    return 4;

                // 3 byte operands
                case BiteOpCodes.CallMethod:
                    return 3;

                default:
                    return 0;
            }
        }

        public static int GetInstructionSize(BiteOpCodes op)
        {
            return 1 + GetOperandCount(op); // Opcode byte + operands
        }

        public static string GetName(BiteOpCodes op)
        {
            return op.ToString().ToUpperInvariant();
        }
    }
}
