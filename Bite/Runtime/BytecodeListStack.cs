using System;

namespace Bite.Runtime
{

public class BytecodeListStack
{
    private readonly BytecodeList[] m_BytecodeLists = new BytecodeList[1024];

    public int Count { get; set; } = 0;

    #region Public

    public BytecodeList Peek()
    {
        BytecodeList bytecodeList = m_BytecodeLists[Count - 1];

        return bytecodeList;
    }

    public BytecodeList Peek( int i )
    {
        BytecodeList bytecodeList = m_BytecodeLists[i];

        return bytecodeList;
    }

    public BytecodeList Pop()
    {
        BytecodeList bytecodeList = m_BytecodeLists[--Count];

        return bytecodeList;
    }

    public void Push( BytecodeList dynamicVar )
    {
        m_BytecodeLists[Count] = dynamicVar;

        if ( Count >= 1023 )
        {
            throw new IndexOutOfRangeException( "Stack Overflow" );
        }

        Count++;
    }

    #endregion
}

}
