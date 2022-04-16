using System;
using System.Collections.Generic;
using System.Reflection;
using Bite.Runtime.Functions.ForeignInterface;

namespace Bite.Runtime
{

public class FieldCache
{
    private static readonly Dictionary < string, FastFieldInfo > m_FieldCache = new Dictionary < string, FastFieldInfo >();

    #region Public

    public bool TryGetField( Type type, string fieldName, out FastFieldInfo fieldInfo )
    {
        string key = $"{type.FullName}.{fieldName}";

        if ( !m_FieldCache.TryGetValue( key, out fieldInfo ) )
        {
            FieldInfo fi = type.GetField( fieldName );
            
            if ( fi != null )
            {
                FastFieldInfo ffi = new FastFieldInfo( fi );
                m_FieldCache.Add( key, ffi );
                fieldInfo = ffi;
                return true;
            }

            return false;
        }

        return true;
    }

    #endregion
}

}
