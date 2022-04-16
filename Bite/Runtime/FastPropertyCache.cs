using System;
using System.Collections.Generic;
using Bite.Runtime.Functions.ForeignInterface;

namespace Bite.Runtime
{

public class FastPropertyCache
{
    private static Dictionary < string, IFastPropertyInfo > m_PropertyCache = new Dictionary < string, IFastPropertyInfo >();

    #region Public

    public static void AddPropertyToCache<T>(string propertyName)
    {
        string key = $"{typeof(T).FullName}.{propertyName}";
        var type = typeof(T);
        var pi = type.GetProperty(propertyName);
        var fp = new FastPropertyInfo<T>(pi);

        IFastPropertyInfo fastPropertyInfo = new CachedProperty < T >(fp.GetterDelegate, fp.SetterDelegate);
        fastPropertyInfo.PropertyType = pi.PropertyType;
        m_PropertyCache.Add( key, fastPropertyInfo );
    }
    public bool TryGetProperty( Type type, string propertyName, out IFastPropertyInfo propertyInfo )
    {
        string key = $"{type.FullName}.{propertyName}";

        if ( !m_PropertyCache.TryGetValue( key, out propertyInfo ) )
        {
            return false;
        }

        return true;
    }

    #endregion
}

}
