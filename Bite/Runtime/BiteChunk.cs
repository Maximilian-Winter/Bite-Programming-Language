// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Bite.Runtime
{
    /// <summary>
    /// A compiled chunk of bytecode for the Bite VM.
    /// Contains the bytecode, constant pool, and debug information.
    /// </summary>
    public sealed class BiteChunk
    {
        /// <summary>The bytecode array</summary>
        public byte[] Code { get; }
        
        /// <summary>Constant pool - strings, doubles, objects referenced by index</summary>
        public IReadOnlyList<BiteConstant> Constants { get; }
        
        /// <summary>Line number for each bytecode offset (for debugging)</summary>
        public IReadOnlyList<int> Lines { get; }
        
        /// <summary>Name of this chunk (function name, "main", etc.)</summary>
        public string Name { get; }
        
        /// <summary>Number of local variables needed</summary>
        public int LocalCount { get; }
        
        /// <summary>Size of the bytecode</summary>
        public int CodeSize => Code?.Length ?? 0;

        public BiteChunk(byte[] code, BiteConstant[] constants, int[] lines, string name, int localCount)
        {
            Code = code ?? Array.Empty<byte>();
            Constants = constants ?? Array.Empty<BiteConstant>();
            Lines = lines ?? Array.Empty<int>();
            Name = name ?? "<anonymous>";
            LocalCount = localCount;
        }

        /// <summary>Read a byte at the given offset</summary>
        public byte ReadByte(int offset)
        {
            if (offset < 0 || offset >= Code.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            return Code[offset];
        }

        /// <summary>Read opcode at the given offset</summary>
        public BiteOpCodes ReadOpCode(int offset)
        {
            return (BiteOpCodes)ReadByte(offset);
        }

        /// <summary>Read signed 8-bit operand</summary>
        public sbyte ReadI8(int offset)
        {
            return (sbyte)ReadByte(offset);
        }

        /// <summary>Read unsigned 8-bit operand</summary>
        public byte ReadU8(int offset)
        {
            return ReadByte(offset);
        }

        /// <summary>Read unsigned 16-bit operand (little-endian)</summary>
        public ushort ReadU16(int offset)
        {
            if (offset < 0 || offset + 1 >= Code.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            return (ushort)(Code[offset] | (Code[offset + 1] << 8));
        }

        /// <summary>Read signed 16-bit operand (little-endian)</summary>
        public short ReadI16(int offset)
        {
            return (short)ReadU16(offset);
        }

        /// <summary>Read signed 32-bit operand (little-endian)</summary>
        public int ReadI32(int offset)
        {
            if (offset < 0 || offset + 3 >= Code.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            return Code[offset] 
                | (Code[offset + 1] << 8) 
                | (Code[offset + 2] << 16) 
                | (Code[offset + 3] << 24);
        }

        /// <summary>Get constant at index</summary>
        public BiteConstant GetConstant(int index)
        {
            if (index < 0 || index >= Constants.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return Constants[index];
        }

        /// <summary>Get line number for bytecode offset</summary>
        public int GetLine(int offset)
        {
            if (offset < 0 || offset >= Lines.Count)
                return -1;
            return Lines[offset];
        }
    }

    /// <summary>
    /// A constant value stored in the chunk's constant pool.
    /// </summary>
    public readonly struct BiteConstant
    {
        public BiteConstantType Type { get; }
        
        // Union-like storage
        private readonly double m_Number;
        private readonly string m_String;
        private readonly object m_Object;

        private BiteConstant(BiteConstantType type, double number, string str, object obj)
        {
            Type = type;
            m_Number = number;
            m_String = str;
            m_Object = obj;
        }

        public static BiteConstant Number(double value)
        {
            return new BiteConstant(BiteConstantType.Number, value, null, null);
        }

        public static BiteConstant String(string value)
        {
            return new BiteConstant(BiteConstantType.String, 0, value ?? string.Empty, null);
        }

        public static BiteConstant Object(object value)
        {
            return new BiteConstant(BiteConstantType.Object, 0, null, value);
        }

        public double AsNumber()
        {
            if (Type != BiteConstantType.Number)
                throw new InvalidOperationException($"Constant is not a number, it's {Type}");
            return m_Number;
        }

        public string AsString()
        {
            if (Type != BiteConstantType.String)
                throw new InvalidOperationException($"Constant is not a string, it's {Type}");
            return m_String;
        }

        public object AsObject()
        {
            if (Type != BiteConstantType.Object)
                throw new InvalidOperationException($"Constant is not an object, it's {Type}");
            return m_Object;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case BiteConstantType.Number: return m_Number.ToString();
                case BiteConstantType.String: return "\"" + m_String + "\"";
                case BiteConstantType.Object: return m_Object?.GetType().Name ?? "null";
                default: return "unknown";
            }
        }
    }

    public enum BiteConstantType : byte
    {
        Number,
        String,
        Object
    }
}
