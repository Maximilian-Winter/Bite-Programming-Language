using System;
using Srsl.Runtime.Memory;

namespace Srsl.Runtime
{
    public class FastMemoryStack
    {
        private FastMemorySpace[] m_FastMemorySpaces = new FastMemorySpace[1024];
        private int m_FastMemoryPointer = 0;

        public int Count => m_FastMemoryPointer;

        public FastMemorySpace Peek()
        {
            FastMemorySpace fastMemorySpace = m_FastMemorySpaces[m_FastMemoryPointer - 1];
            return fastMemorySpace;
        }

        public FastMemorySpace Pop()
        {
            FastMemorySpace fastMemorySpace = m_FastMemorySpaces[--m_FastMemoryPointer];
            return fastMemorySpace;
        }

        public void Push(FastMemorySpace fastMemorySpace)
        {
            m_FastMemorySpaces[m_FastMemoryPointer] = fastMemorySpace;

            if (m_FastMemoryPointer >= 1023)
            {
                throw new IndexOutOfRangeException("Call Stack Overflow");
            }
            m_FastMemoryPointer++;
        }
    }
}