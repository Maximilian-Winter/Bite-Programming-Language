using System;

namespace Bite.Runtime.Functions.ForeignInterface
{

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
