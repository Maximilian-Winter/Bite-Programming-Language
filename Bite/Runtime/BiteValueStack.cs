// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;

namespace Bite.Runtime
{
    /// <summary>
    /// A fast, fixed-capacity stack for BiteValue.
    /// Uses unsafe pointers for maximum performance in the VM hot path.
    /// </summary>
    public sealed class BiteValueStack
    {
        private readonly BiteValue[] m_Storage;
        private int m_Count;

        /// <summary>Current number of items on the stack</summary>
        public int Count => m_Count;

        /// <summary>Maximum capacity of the stack</summary>
        public int Capacity => m_Storage.Length;

        /// <summary>Returns true if stack is empty</summary>
        public bool IsEmpty => m_Count == 0;

        /// <summary>Creates a new stack with specified capacity (default 4096)</summary>
        public BiteValueStack(int capacity = 4096)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be positive", nameof(capacity));
            
            m_Storage = new BiteValue[capacity];
            m_Count = 0;
        }

        /// <summary>Push a value onto the stack</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(BiteValue value)
        {
            if (m_Count >= m_Storage.Length)
                throw new StackOverflowException("BiteValueStack overflow");
            
            m_Storage[m_Count++] = value;
        }

        /// <summary>Push a double (number) directly</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(double value)
        {
            Push(new BiteValue(value));
        }

        /// <summary>Push an int (widened to number) directly</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(int value)
        {
            Push(new BiteValue(value));
        }

        /// <summary>Push a bool directly</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(bool value)
        {
            Push(new BiteValue(value));
        }

        /// <summary>Pop a value from the stack</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiteValue Pop()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Stack underflow");
            
            return m_Storage[--m_Count];
        }

        /// <summary>Pop without returning the value (faster when discarding)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopDiscard()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Stack underflow");
            
            m_Count--;
        }

        /// <summary>Peek at the top value without removing it</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiteValue Peek()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Stack is empty");
            
            return m_Storage[m_Count - 1];
        }

        /// <summary>Peek at value at offset from top (0 = top, 1 = next, etc.)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiteValue Peek(int offsetFromTop)
        {
            int index = m_Count - 1 - offsetFromTop;
            if (index < 0 || index >= m_Count)
                throw new ArgumentOutOfRangeException(nameof(offsetFromTop));
            
            return m_Storage[index];
        }

        /// <summary>Get/set value at absolute index from stack bottom</summary>
        public BiteValue this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= m_Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return m_Storage[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= m_Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                m_Storage[index] = value;
            }
        }

        /// <summary>Get pointer to storage array (for unsafe code)</summary>
        public BiteValue[] GetRawStorage() => m_Storage;

        /// <summary>Get current top index (for use with unsafe pointer arithmetic)</summary>
        public int TopIndex => m_Count - 1;

        /// <summary>Drop N values from the top of the stack</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Drop(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count > m_Count)
                throw new InvalidOperationException("Stack underflow");
            
            m_Count -= count;
        }

        /// <summary>Duplicate the top value (push a copy)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dup()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Stack is empty");
            
            Push(m_Storage[m_Count - 1]);
        }

        /// <summary>Duplicate the value at offset from top (0 = dup top)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dup(int offsetFromTop)
        {
            Push(Peek(offsetFromTop));
        }

        /// <summary>Swap top two values</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Swap()
        {
            if (m_Count < 2)
                throw new InvalidOperationException("Stack has fewer than 2 items");
            
            (m_Storage[m_Count - 1], m_Storage[m_Count - 2]) = 
                (m_Storage[m_Count - 2], m_Storage[m_Count - 1]);
        }

        /// <summary>Rotate top N values (move bottom of N to top)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rotate(int n)
        {
            if (n < 2 || n > m_Count)
                throw new ArgumentOutOfRangeException(nameof(n));
            
            BiteValue temp = m_Storage[m_Count - n];
            for (int i = m_Count - n; i < m_Count - 1; i++)
                m_Storage[i] = m_Storage[i + 1];
            m_Storage[m_Count - 1] = temp;
        }

        /// <summary>Clear the entire stack</summary>
        public void Clear()
        {
            // We need to clean up any GCHandles that might be on the stack
            for (int i = 0; i < m_Count; i++)
            {
                m_Storage[i].FreeHandle();
            }
            m_Count = 0;
        }

        /// <summary>Reset stack pointer without cleanup (use with caution)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            m_Count = 0;
        }

        /// <summary>Get the current stack pointer position</summary>
        public int Position => m_Count;

        /// <summary>Set the stack pointer position (for restoring state)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(int position)
        {
            if (position < 0 || position > m_Storage.Length)
                throw new ArgumentOutOfRangeException(nameof(position));
            m_Count = position;
        }

        /// <summary>Get a copy of current stack contents as array</summary>
        public BiteValue[] ToArray()
        {
            var result = new BiteValue[m_Count];
            System.Array.Copy(m_Storage, result, m_Count);
            return result;
        }

        /// <summary>Copy stack contents to an array</summary>
        public void CopyTo(BiteValue[] destination, int startIndex = 0)
        {
            System.Array.Copy(m_Storage, 0, destination, startIndex, m_Count);
        }
    }
}
