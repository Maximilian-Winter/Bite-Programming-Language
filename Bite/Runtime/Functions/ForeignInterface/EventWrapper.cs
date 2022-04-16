using System;
using System.Collections.Generic;
using System.Reflection;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class EventWrapper < V, T, K >
{
    private readonly Dictionary < string, EventInfo > m_EventInfos = new Dictionary < string, EventInfo >();

    public object EventHolder { get; set; }

    #region Public

    public EventWrapper( object obj )
    {
        EventHolder = obj;
        EventInfo[] eventInfos = obj.GetType().GetEvents();

        foreach ( EventInfo eventInfo in eventInfos )
        {
            m_EventInfos.Add( eventInfo.Name, eventInfo );
        }
    }

    public bool TryAddEventHandler( string name, BiteChunkWrapper eventHandlerFunction, BiteVm biteVm )
    {
        if ( m_EventInfos.TryGetValue( name, out EventInfo eventInfo ) )
        {
            MethodInfo methodInfo = eventInfo.EventHandlerType.GetMethod( "Invoke" );
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            if ( parameterInfos.Length == 0 )
            {
                if ( methodInfo.ReturnParameter != null )
                {
                    Func < object > action = delegate
                    {
                        DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[0];

                        BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                            eventHandlerFunction,
                            dynamicBiteVariables );

                        biteVm.CallBiteFunction( biteFunctionCall );

                        return null;
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
                else
                {
                    Action action = delegate
                    {
                        DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[0];

                        BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                            eventHandlerFunction,
                            dynamicBiteVariables );

                        biteVm.CallBiteFunction( biteFunctionCall );
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
            }
            else if ( parameterInfos.Length == 1 )
            {
                if ( methodInfo.ReturnParameter != null )
                {
                    Func < T, object > action = delegate( T arg1 )
                    {
                        DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[1];

                        dynamicBiteVariables[0] = DynamicVariableExtension.ToDynamicVariable( arg1 );

                        BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                            eventHandlerFunction,
                            dynamicBiteVariables );

                        biteVm.CallBiteFunction( biteFunctionCall );

                        return null;
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
                else
                {
                    Action < T > action = delegate( T arg1 )
                    {
                        DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[1];

                        dynamicBiteVariables[0] = DynamicVariableExtension.ToDynamicVariable( arg1 );

                        BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                            eventHandlerFunction,
                            dynamicBiteVariables );

                        biteVm.CallBiteFunction( biteFunctionCall );
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
            }
            else if ( parameterInfos.Length == 2 )
            {
                if ( methodInfo.ReturnParameter != null )
                {
                    Func < T, K, object > action = delegate( T arg1, K k )
                    {
                        DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[2];

                        dynamicBiteVariables[0] = DynamicVariableExtension.ToDynamicVariable( arg1 );
                        dynamicBiteVariables[1] = DynamicVariableExtension.ToDynamicVariable( k );

                        BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                            eventHandlerFunction,
                            dynamicBiteVariables );

                        biteVm.CallBiteFunction( biteFunctionCall );

                        return null;
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
                else
                {
                    Action < T, K > action = delegate( T arg1, K k )
                    {
                        DynamicBiteVariable[] dynamicBiteVariables = new DynamicBiteVariable[2];

                        dynamicBiteVariables[0] = DynamicVariableExtension.ToDynamicVariable( arg1 );
                        dynamicBiteVariables[1] = DynamicVariableExtension.ToDynamicVariable( k );

                        BiteFunctionCall biteFunctionCall = new BiteFunctionCall(
                            eventHandlerFunction,
                            dynamicBiteVariables );

                        biteVm.CallBiteFunction( biteFunctionCall );
                    };

                    Delegate del = action;
                    eventInfo.AddEventHandler( EventHolder, DelegateUtility.Cast( del, typeof( V ) ) );
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public bool TryGetEventInfo( string name, out EventInfo eventInfo )
    {
        return m_EventInfos.TryGetValue( name, out eventInfo );
    }

    public void Invoke( string name, List < DynamicBiteVariable > functionArguments )
    {
         if ( m_EventInfos.TryGetValue( name, out EventInfo eventInfo ) )
        {
            MethodInfo methodInfo = eventInfo.EventHandlerType.GetMethod( "Invoke" );
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            
            if ( functionArguments.Count != parameterInfos.Length )
            {
                return;
            }
            else
            {
                functionArguments.Reverse();
                object[] eventArgs = new object[functionArguments.Count];
                
                for ( int i = 0; i < functionArguments.Count; i++ )
                {
                    object arg = functionArguments[i].ToObject();

                    if ( arg is FastClassMemorySpace )
                    {
                        eventArgs[i] = arg;
                    }
                    else
                    {
                        eventArgs[i] = Convert.ChangeType( arg, parameterInfos[i].ParameterType );
                    }
                   
                }

                RaiseEventViaReflection( EventHolder, name, eventArgs );
                return;
            }
        }
    }
        
    private void RaiseEventViaReflection(object source, string eventName, object[] eventArgs)
    {
        ((Delegate)source
                   .GetType()
                   .GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
                   .GetValue(source))
            .DynamicInvoke(eventArgs[0], eventArgs[1]);
    }
    private void RaiseEventViaReflection(object source, string eventName)
    {
        ((Delegate)source
                   .GetType()
                   .GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
                   .GetValue(source))
            .DynamicInvoke(source, EventArgs.Empty);
    }
    #endregion
}

public interface ICSharpEvent
{
    bool TryGetEventInfo( string name, out EventInfo eventInfo );
    object GetEventHolder();
    void Invoke( string name, List < DynamicBiteVariable > m_FunctionArguments );
    bool TryAddEventHandler( string name, BiteChunkWrapper eventHandlerFunction, BiteVm biteVm );
}

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

    public void Invoke( string name, List < DynamicBiteVariable > m_FunctionArguments )
    {
        m_EventWrapper.Invoke( name, m_FunctionArguments );
    }

    public bool TryAddEventHandler( string name, BiteChunkWrapper eventHandlerFunction, BiteVm biteVm )
    {
        return m_EventWrapper.TryAddEventHandler( name, eventHandlerFunction, biteVm );
    }
    
   

    #endregion
}

public static class DelegateUtility
{
    #region Public

    public static T Cast < T >( Delegate source ) where T : class
    {
        return Cast( source, typeof( T ) ) as T;
    }

    public static Delegate Cast( Delegate source, Type type )
    {
        if ( source == null )
        {
            return null;
        }

        Delegate[] delegates = source.GetInvocationList();

        if ( delegates.Length == 1 )
        {
            return Delegate.CreateDelegate(
                type,
                delegates[0].Target,
                delegates[0].Method );
        }

        Delegate[] delegatesDest = new Delegate[delegates.Length];

        for ( int nDelegate = 0; nDelegate < delegates.Length; nDelegate++ )
        {
            delegatesDest[nDelegate] = Delegate.CreateDelegate(
                type,
                delegates[nDelegate].Target,
                delegates[nDelegate].Method );
        }

        return Delegate.Combine( delegatesDest );
    }

    #endregion
}



}
