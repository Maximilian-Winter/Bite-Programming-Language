using System;
using System.Collections.Generic;
using Bite.Runtime.Bytecode;
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

public class DynamicBiteVariableStack
{
    private DynamicBiteVariable[] m_DynamicVariables = new DynamicBiteVariable[128];

    public int Count { get; set; } = 0;

    #region Public

    public DynamicBiteVariable Peek()
    {
        DynamicBiteVariable dynamicVariable = m_DynamicVariables[Count - 1];

        return dynamicVariable;
    }

    public DynamicBiteVariable Peek( int i )
    {
        DynamicBiteVariable dynamicVariable = m_DynamicVariables[i];

        return dynamicVariable;
    }

    public DynamicBiteVariable Pop()
    {
        DynamicBiteVariable dynamicVariable = m_DynamicVariables[--Count];

        return dynamicVariable;
    }

    public void Push( DynamicBiteVariable dynamicVar )
    {
        if ( Count >= m_DynamicVariables.Length )
        {
            DynamicBiteVariable[] newProperties = new DynamicBiteVariable[m_DynamicVariables.Length * 2];
            Array.Copy( m_DynamicVariables, newProperties, m_DynamicVariables.Length );
            m_DynamicVariables = newProperties;
        }

        m_DynamicVariables[Count] = dynamicVar;

        Count++;
    }

    #endregion
}

public class BytecodeList
{
    public List < ByteCode > ByteCodes = new List < ByteCode >();
}

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

public class UsingStatementStack
{
    private object[] m_UsedObjects = new object[128];

    public int Count { get; set; } = 0;

    #region Public

    public void Pop()
    {
        object usedObject = m_UsedObjects[--Count];
        ( ( IDisposable ) usedObject ).Dispose();
    }

    public void Push( object usedObject )
    {
        if ( Count >= m_UsedObjects.Length )
        {
            object[] newProperties = new object[m_UsedObjects.Length * 2];
            Array.Copy( m_UsedObjects, newProperties, m_UsedObjects.Length );
            m_UsedObjects = newProperties;
        }

        m_UsedObjects[Count] = usedObject;

        if ( Count >= 1023 )
        {
            throw new IndexOutOfRangeException( "Using Statement Overflow" );
        }

        Count++;
    }

    #endregion
}

}
