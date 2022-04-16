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
    private BiteFunctionCall[] m_FastMemorySpaces = new BiteFunctionCall[128];

    public int Count { get; private set; } = 0;

    #region Public

    public BiteFunctionCall Peek()
    {
        BiteFunctionCall fastMemorySpace = m_FastMemorySpaces[Count - 1];

        return fastMemorySpace;
    }

    public BiteFunctionCall Pop()
    {
        BiteFunctionCall fastMemorySpace = m_FastMemorySpaces[--Count];

        return fastMemorySpace;
    }

    public void Push( BiteFunctionCall fastMemorySpace )
    {
        if ( Count >= m_FastMemorySpaces.Length )
        {
            BiteFunctionCall[] newProperties = new BiteFunctionCall[m_FastMemorySpaces.Length * 2];
            Array.Copy( m_FastMemorySpaces, newProperties, m_FastMemorySpaces.Length );
            m_FastMemorySpaces = newProperties;
        }

        m_FastMemorySpaces[Count] = fastMemorySpace;
        Count++;
    }

    #endregion
}

}
