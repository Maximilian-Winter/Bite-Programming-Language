using System;
using System.Collections.Generic;
using System.Reflection;
using Bite.Runtime.Functions.ForeignInterface;

namespace Bite.Runtime
{

public class MethodCache
{
    private static readonly Dictionary < string, FastMethodInfo > m_MethodCache =
        new Dictionary < string, FastMethodInfo >();

    #region Public

    public bool TryGetMethod(
        Type type,
        Type[] functionArgumentTypes,
        string methodName,
        out FastMethodInfo fastMethodInfo )
    {
        string key = $"{type.FullName}.{methodName}";

        for ( int i = 0; i < functionArgumentTypes.Length; i++ )
        {
            key += "." + functionArgumentTypes[i].Name;
        }

        if ( !m_MethodCache.TryGetValue( key, out fastMethodInfo ) )
        {
            MethodInfo methodInfo = type.GetMethod( methodName, functionArgumentTypes );

            if ( methodInfo != null )
            {
                fastMethodInfo = new FastMethodInfo( methodInfo );
                m_MethodCache.Add( key, fastMethodInfo );

                return true;
            }

            return false;
        }

        return true;
    }

    #endregion
}

}
