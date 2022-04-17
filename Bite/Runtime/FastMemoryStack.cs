using System;
using Bite.Runtime.Memory;

namespace Bite.Runtime
{

public class FastMemoryStack
{
    private FastMemorySpace[] m_FastMemorySpaces = new FastMemorySpace[128];

    public int Count { get; private set; } = 0;

    #region Public

    public FastMemorySpace Peek()
    {
        FastMemorySpace fastMemorySpace = m_FastMemorySpaces[Count - 1];

        return fastMemorySpace;
    }

    public FastMemorySpace Pop()
    {
        FastMemorySpace fastMemorySpace = m_FastMemorySpaces[--Count];

        return fastMemorySpace;
    }

    public void Push( FastMemorySpace fastMemorySpace )
    {
        if ( Count >= m_FastMemorySpaces.Length )
        {
            FastMemorySpace[] newProperties = new FastMemorySpace[m_FastMemorySpaces.Length * 2];
            Array.Copy( m_FastMemorySpaces, newProperties, m_FastMemorySpaces.Length );
            m_FastMemorySpaces = newProperties;
        }

        m_FastMemorySpaces[Count] = fastMemorySpace;
        Count++;
    }

    #endregion
}

public class BiteFunctionCallStack
{
    private BiteFunctionCall[] m_BiteFunctionCalls = new BiteFunctionCall[128];

    private int m_EnqueuePointer;
    private int m_DequeuePointer;
    private int m_Size;
    public int Count
    {
        get
        {
            return m_DequeuePointer - m_EnqueuePointer;
        }
    }

    
    #region Public

    public BiteFunctionCallStack()
    {
        m_EnqueuePointer = 127;
        m_DequeuePointer = 127;
        m_Size = 128;
    }

    public BiteFunctionCall Peek()
    {
        BiteFunctionCall fastMemorySpace = m_BiteFunctionCalls[m_DequeuePointer];

        return fastMemorySpace;
    }

    public BiteFunctionCall Dequeue()
    {
        BiteFunctionCall fastMemorySpace = m_BiteFunctionCalls[m_DequeuePointer--];

        if ( m_DequeuePointer == m_EnqueuePointer )
        {
            m_EnqueuePointer = m_Size - 1;
            m_DequeuePointer = m_Size - 1;
        }
        return fastMemorySpace;
    }

    public void Enqueue( BiteFunctionCall functionCall )
    {
        if ( Count >= m_BiteFunctionCalls.Length )
        {
            BiteFunctionCall[] newProperties = new BiteFunctionCall[m_BiteFunctionCalls.Length * 2];
            Array.Copy( m_BiteFunctionCalls, newProperties, m_BiteFunctionCalls.Length );
            m_BiteFunctionCalls = newProperties;
        }

        m_BiteFunctionCalls[m_EnqueuePointer++] = functionCall;
    }

    #endregion
}

}
