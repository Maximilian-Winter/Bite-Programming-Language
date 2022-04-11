using System;

namespace Bite.Runtime.Memory
{

public class ObjectPoolFastMemory
{
    private FastMemorySpace[] m_FastCallMemorySpaces = new FastMemorySpace[128];
    private int m_FastMemorySpacePointer = 0;

    #region Public

    public ObjectPoolFastMemory()
    {
        for ( int i = 0; i < 128; i++ )
        {
            m_FastCallMemorySpaces[i] = new FastMemorySpace( $"$objectpool_{i}", null, 0, null, 0, 0 );
        }
    }

    public FastMemorySpace Get()
    {
        if ( m_FastMemorySpacePointer >= m_FastCallMemorySpaces.Length )
        {
            FastMemorySpace[] newProperties = new FastMemorySpace[m_FastCallMemorySpaces.Length * 2];
            Array.Copy( m_FastCallMemorySpaces, newProperties, m_FastCallMemorySpaces.Length );

            for ( int i = m_FastMemorySpacePointer; i < newProperties.Length; i++ )
            {
                newProperties[i] = new FastMemorySpace( $"$objectpool_{i}", null, 0, null, 0, 0 );
            }
            m_FastCallMemorySpaces = newProperties;
        }
        
        FastMemorySpace fastMemorySpace = m_FastCallMemorySpaces[m_FastMemorySpacePointer];
        fastMemorySpace.Properties = Array.Empty < DynamicBiteVariable >();
        fastMemorySpace.NamesToProperties.Clear();
        fastMemorySpace.CallerChunk = null;
        fastMemorySpace.CallerIntructionPointer = 0;
        fastMemorySpace.StackCountAtBegin = 0;
       

        m_FastMemorySpacePointer++;

        return fastMemorySpace;
    }

    public void Return( FastMemorySpace item )
    {
        if ( item == m_FastCallMemorySpaces[m_FastMemorySpacePointer - 1] )
        {
            m_FastMemorySpacePointer--;
        }
    }

    #endregion
}

}
