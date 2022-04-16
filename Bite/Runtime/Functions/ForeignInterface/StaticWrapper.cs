using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class StaticWrapper
{
    private readonly Dictionary < string, FastMethodInfo > CachedStaticMethods =
        new Dictionary < string, FastMethodInfo >();

    public Type StaticWrapperType { get; }

    #region Public

    public StaticWrapper( Type type )
    {
        StaticWrapperType = type;
    }

    public object InvokeMember( string name, object[] args, Type[] argsTypes )
    {
        if ( !CachedStaticMethods.ContainsKey( name ) )
        {
            MethodInfo method = StaticWrapperType.GetMethod(
                name,
                BindingFlags.Static | BindingFlags.Public,
                null,
                argsTypes,
                null );

            FastMethodInfo fastMethodInfo = new FastMethodInfo( method );

            CachedStaticMethods.Add( name, fastMethodInfo );

            return fastMethodInfo.Invoke( null, args );
        }

        return CachedStaticMethods[name].Invoke( null, args );
    }

    #endregion
}

}
