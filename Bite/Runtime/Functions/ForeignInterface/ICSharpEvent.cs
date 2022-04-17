using System.Collections.Generic;
using System.Reflection;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.ForeignInterface
{

public interface ICSharpEvent
{
    bool TryGetEventInfo( string name, out EventInfo eventInfo );
    object GetEventHolder();
    void Invoke( string name, List < DynamicBiteVariable > m_FunctionArguments );
    bool TryAddEventHandler( string name, BiteChunkWrapper eventHandlerFunction, BiteVm biteVm );
}

}
