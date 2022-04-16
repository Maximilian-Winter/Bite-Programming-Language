using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bite.Runtime
{

public class PropertyCache
{
    private static readonly Dictionary < string, PropertyInfo > m_PropertyCache =
        new Dictionary < string, PropertyInfo >();

    #region Public

    public bool TryGetProperty( Type type, string propertyName, out PropertyInfo propertyInfo )
    {
        string key = $"{type.FullName}.{propertyName}";

        if ( !m_PropertyCache.TryGetValue( key, out propertyInfo ) )
        {
            propertyInfo = type.GetProperty( propertyName );

            if ( propertyInfo != null )
            {
                m_PropertyCache.Add( key, propertyInfo );

                return true;
            }

            return false;
        }

        return true;
    }

    #endregion
}

}
