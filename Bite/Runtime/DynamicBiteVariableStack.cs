using System;
using Bite.Runtime.Memory;

namespace Bite.Runtime
{

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

}
