using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class EventWrapper
{
    private Dictionary < string, EventInfo > m_EventInfos = new Dictionary < string, EventInfo >();
    private object m_EventHolder;
    private delegate void VoidDelegateNone();
    
    private delegate void VoidDelegateOne( object argument );
    private delegate void VoidDelegateTwo( object sender, object argument );
    private delegate void VoidDelegateThree( object sender, object argument, object argumentTwo );
    private delegate void VoidDelegateFour( object instance, object argument, object argumentTwo, object argumentThree );

    public EventWrapper(object obj)
    {
        EventHolder = obj;
        EventInfo[] eventInfos = obj.GetType().GetEvents();

        foreach ( EventInfo eventInfo in eventInfos )
        {
            m_EventInfos.Add( eventInfo.Name, eventInfo );
        }
    }

    public object EventHolder
    {
        get => m_EventHolder;
        set => m_EventHolder = value;
    }

    public bool TryGetEventInfo( string name, out EventInfo eventInfo )
    {
        return m_EventInfos.TryGetValue( name, out eventInfo ) ;
    }
    
    public bool TryAddEventHandler( string name, BiteChunkWrapper eventHandlerFunction, BiteVm biteVm )
    {
        if ( m_EventInfos.TryGetValue( name, out EventInfo eventInfo ) )
        {
            MethodInfo methodInfo = eventInfo.EventHandlerType.GetMethod("Invoke");
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            if ( parameterInfos.Length == 0 )
            {
                VoidDelegateNone Delegate =() =>
                {
                    DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[0];

                    BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                        eventHandlerFunction,
                        dynamicBiteVariables );
                    
                    biteVm.CallBiteFunction( biteFunctionCall );
                };
            
                eventInfo.AddEventHandler( biteVm, Delegate );
            }
            else if ( parameterInfos.Length == 1 )
            {
                VoidDelegateOne Delegate =(sender) =>
                {
                    DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[1];

                    dynamicBiteVariables[0] = DynamicVariableExtension.ToDynamicVariable(sender);
                    BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                        eventHandlerFunction,
                        dynamicBiteVariables );
                    
                    biteVm.CallBiteFunction( biteFunctionCall );
                };
            
                eventInfo.AddEventHandler( biteVm, Delegate );
            }
            else if ( parameterInfos.Length == 2 )
            {
                VoidDelegateTwo Delegate =( instance, arguments ) =>
                {
                    DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[2];

                    dynamicBiteVariables[0] = DynamicVariableExtension.ToDynamicVariable(instance);
                    dynamicBiteVariables[1] = DynamicVariableExtension.ToDynamicVariable(arguments);
                    BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                        eventHandlerFunction,
                        dynamicBiteVariables );
                    
                    biteVm.CallBiteFunction( biteFunctionCall );
                };
            
                eventInfo.AddEventHandler( biteVm, Delegate );
            }
            else if ( parameterInfos.Length == 3 )
            {
                VoidDelegateThree Delegate =( instance, argOne, argTwo ) =>
                {
                    DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[3];

                    dynamicBiteVariables[0] = DynamicVariableExtension.ToDynamicVariable(instance);
                    dynamicBiteVariables[1] = DynamicVariableExtension.ToDynamicVariable(argOne);
                    dynamicBiteVariables[2] = DynamicVariableExtension.ToDynamicVariable(argTwo);
                    BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                        eventHandlerFunction,
                        dynamicBiteVariables );
                    
                    biteVm.CallBiteFunction( biteFunctionCall );
                };
            
                eventInfo.AddEventHandler( biteVm, Delegate );
            }
            else if ( parameterInfos.Length == 4 )
            {
                VoidDelegateFour Delegate =( instance, argOne, argTwo, argThree ) =>
                {
                    DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[4];

                    dynamicBiteVariables[0] = DynamicVariableExtension.ToDynamicVariable(instance);
                    dynamicBiteVariables[1] = DynamicVariableExtension.ToDynamicVariable(argOne);
                    dynamicBiteVariables[2] = DynamicVariableExtension.ToDynamicVariable(argTwo);
                    dynamicBiteVariables[3] = DynamicVariableExtension.ToDynamicVariable(argThree);
                    BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                        eventHandlerFunction,
                        dynamicBiteVariables );
                    
                    biteVm.CallBiteFunction( biteFunctionCall );
                };
            
                eventInfo.AddEventHandler( biteVm, Delegate );
            }
            
            return true;
        }

        return false ;
    }
}

}
