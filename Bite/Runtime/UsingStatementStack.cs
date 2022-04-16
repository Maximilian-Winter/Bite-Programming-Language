using System;

namespace Bite.Runtime
{

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
