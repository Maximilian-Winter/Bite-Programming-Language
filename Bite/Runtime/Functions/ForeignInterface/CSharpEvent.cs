using System.Collections.Generic;
using System.Reflection;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class CSharpEvent < V, T, K > : ICSharpEvent
{
    private readonly EventWrapper < V, T, K > m_EventWrapper;

    #region Public

    public CSharpEvent( object obj )
    {
        m_EventWrapper = new EventWrapper < V, T, K >( obj );
    }

    public bool TryGetEventInfo( string name, out EventInfo eventInfo )
    {
        return m_EventWrapper.TryGetEventInfo( name, out eventInfo );
    }

    public object GetEventHolder()
    {
        return m_EventWrapper.EventHolder;
    }

    public void Invoke( string name, DynamicBiteVariable[] m_FunctionArguments )
    {
        m_EventWrapper.Invoke( name, m_FunctionArguments );
    }

    public bool TryAddEventHandler( string name, BiteChunkWrapper eventHandlerFunction, BiteVm biteVm )
    {
        return m_EventWrapper.TryAddEventHandler( name, eventHandlerFunction, biteVm );
    }
    
   

    #endregion
}

}
