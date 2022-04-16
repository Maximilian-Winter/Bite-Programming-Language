using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class TypeRegistry
{
    private readonly Dictionary < string, Type > m_RegisteredTypes = new Dictionary < string, Type >();

    private readonly Dictionary < string, MethodInfo > m_MethodCache = new Dictionary < string, MethodInfo >();

    private readonly Dictionary < string, ConstructorInfo > m_ConstructorCache =
        new Dictionary < string, ConstructorInfo >();

    #region Public

    public TypeRegistry()
    {
        RegisterDefaultTypes();
    }

    public ConstructorInfo GetConstructor( Type type )
    {
        string key = $"{type.FullName}.ctor()";

        if ( !m_ConstructorCache.TryGetValue( key, out ConstructorInfo constructorInfo ) )
        {
            constructorInfo = type.GetConstructor( Type.EmptyTypes );
            m_ConstructorCache.Add( key, constructorInfo );
        }

        return constructorInfo;
    }

    public ConstructorInfo GetConstructor( Type type, Type[] argTypes )
    {
        string key = $"{type.FullName}.ctor({GetArgTypeNames( argTypes )})";

        if ( !m_ConstructorCache.TryGetValue( key, out ConstructorInfo constructorInfo ) )
        {
            constructorInfo = type.GetConstructor( argTypes );
            m_ConstructorCache.Add( key, constructorInfo );
        }

        return constructorInfo;
    }

    public MethodInfo GetMethod( Type type, string methodName, Type[] argTypes )
    {
        string key = $"{type.FullName}.{methodName}({GetArgTypeNames( argTypes )})";

        if ( !m_MethodCache.TryGetValue( key, out MethodInfo methodInfo ) )
        {
            methodInfo = type.GetMethod( methodName, argTypes );
            m_MethodCache.Add( key, methodInfo );
        }

        return methodInfo;
    }

    public void RegisterAssemblyTypes( Assembly assembly, Func < Type, bool > filter = null )
    {
        IEnumerable < Type > types = assembly.GetTypes().AsEnumerable();

        if ( filter != null )
        {
            types = types.Where( filter );
        }

        foreach ( Type type in types )
        {
            m_RegisteredTypes.Add( type.Name, type );
        }
    }

    /// <summary>
    ///     Registers the type using the specified type's Name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="alias"></param>
    public void RegisterType < T >( string alias )
    {
        m_RegisteredTypes.Add( alias, typeof( T ) );
    }

    /// <summary>
    ///     Registers the type using the specified type's Name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void RegisterType < T >()
    {
        Type type = typeof( T );
        m_RegisteredTypes.Add( type.Name, type );
    }

    /// <summary>
    ///     Registers the type using the specified alias
    /// </summary>
    /// <param name="type"></param>
    /// <param name="alias"></param>
    public void RegisterType( Type type, string alias )
    {
        m_RegisteredTypes.Add( alias, type );
    }

    /// <summary>
    ///     Registers the type using the specified type's Name
    /// </summary>
    /// <param name="type"></param>
    public void RegisterType( Type type )
    {
        m_RegisteredTypes.Add( type.Name, type );
    }

    /// <summary>
    ///     Registers the types using the specified type's Name
    /// </summary>
    /// <param name="types"></param>
    public void RegisterTypes( IEnumerable < Type > types )
    {
        foreach ( Type type in types )
        {
            m_RegisteredTypes.Add( type.Name, type );
        }
    }

    public bool TryResolveType( string name, out Type type )
    {
        return m_RegisteredTypes.TryGetValue( name, out type );
    }

    #endregion

    #region Private

    private string GetArgTypeNames( Type[] argTypes )
    {
        // if (argTypes == null || argTypes.Length == 0) return "";

        string[] argTypeNames = new string[argTypes.Length];

        for ( int i = 0; i < argTypes.Length; i++ )
        {
            argTypeNames[i] = argTypes[i].FullName;
        }

        return string.Join( ",", argTypeNames );
    }

    private void RegisterDefaultTypes()
    {
        m_RegisteredTypes.Add( "bool", typeof( bool ) );
        m_RegisteredTypes.Add( "byte", typeof( byte ) );
        m_RegisteredTypes.Add( "ushort", typeof( ushort ) );
        m_RegisteredTypes.Add( "short", typeof( short ) );
        m_RegisteredTypes.Add( "uint", typeof( uint ) );
        m_RegisteredTypes.Add( "int", typeof( int ) );
        m_RegisteredTypes.Add( "ulong", typeof( ulong ) );
        m_RegisteredTypes.Add( "long", typeof( long ) );
        m_RegisteredTypes.Add( "float", typeof( float ) );
        m_RegisteredTypes.Add( "double", typeof( double ) );
        m_RegisteredTypes.Add( "string", typeof( string ) );
    }

    #endregion
}

}
