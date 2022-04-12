using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class TypeRegistry
{
    private readonly Dictionary < string, Type > m_RegisteredTypes = new Dictionary < string, Type >();


    public void RegisterAssemblyTypes( Assembly assembly, Func < Type, bool > filter = null )
    {
        var types = assembly.GetTypes().AsEnumerable();

        if ( filter != null )
        {
            types = types.Where( filter );
        }

        foreach ( var type in types )
        {
            m_RegisteredTypes.Add( type.Name, type );
        }
    }

    /// <summary>
    /// Registers the type using the specified type's Name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="alias"></param>
    public void RegisterType<T>( string alias )
    {
        m_RegisteredTypes.Add( alias, typeof( T ) );
    }

    /// <summary>
    /// Registers the type using the specified type's Name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void RegisterType<T>()
    {
        Type type = typeof( T );
        m_RegisteredTypes.Add( type.Name, type );
    }

    /// <summary>
    /// Registers the type using the specified alias
    /// </summary>
    /// <param name="type"></param>
    /// <param name="alias"></param>
    public void RegisterType( Type type, string alias )
    {
        m_RegisteredTypes.Add( alias, type );
    }

    /// <summary>
    /// Registers the type using the specified type's Name
    /// </summary>
    /// <param name="type"></param>
    public void RegisterType( Type type )
    {
        m_RegisteredTypes.Add( type.Name, type );
    }

    /// <summary>
    /// Registers the types using the specified type's Name
    /// </summary>
    /// <param name="types"></param>
    public void RegisterTypes( IEnumerable < Type > types )
    {
        foreach ( var type in types )
        {
            m_RegisteredTypes.Add( type.Name, type );
        }
    }

    public bool TryResolveType( string name, out Type type )
    {
        return m_RegisteredTypes.TryGetValue( name, out type );
    }

}

}