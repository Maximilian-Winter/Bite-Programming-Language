using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class ForeignLibraryInterfaceVm : IBiteVmCallable
{
    private readonly TypeRegistry m_TypeRegistry;

    #region Public

    public ForeignLibraryInterfaceVm()
    {
        m_TypeRegistry = new TypeRegistry();
    }

    public ForeignLibraryInterfaceVm( TypeRegistry typeRegistry )
    {
        m_TypeRegistry = typeRegistry;
    }

    public object Call( List < DynamicBiteVariable > arguments )
    {
        arguments.Reverse();
        if ( arguments.Count > 0 )
        {
            if ( arguments.Count == 1 && arguments[0].DynamicType == DynamicVariableType.Object )
            {
                if ( arguments[0].ObjectData is FastMemorySpace fliObject )
                {
                    string typeString = fliObject.Get( "Type" ).StringData;

                    if ( !string.IsNullOrEmpty( typeString ) )
                    {
                        Type type = ResolveType( typeString );

                        DynamicBiteVariable returnClassBool = fliObject.Get( "ReturnClass" );

                        if ( returnClassBool.DynamicType == DynamicVariableType.True )
                        {
                            StaticWrapper wrapper = new StaticWrapper( type );
                            fliObject.Put( "ObjectInstance", DynamicVariableExtension.ToDynamicVariable( wrapper ) );

                            return wrapper;
                        }

                        DynamicBiteVariable methodString = fliObject.Get( "Method" );

                        if ( methodString.DynamicType == DynamicVariableType.String &&
                             !string.IsNullOrEmpty( methodString.StringData ) )
                        {
                            List < Type > argTypes = new List < Type >();

                            if ( fliObject.Get( "Arguments" ).ObjectData is FastMemorySpace instanceTypes )
                            {
                                foreach ( DynamicBiteVariable instanceProperty in instanceTypes.Properties )
                                {
                                    argTypes.Add( instanceProperty.GetType() );
                                }
                            }

                            MethodInfo method = m_TypeRegistry.GetMethod(
                                type,
                                methodString.StringData,
                                argTypes.ToArray() );

                            if ( method != null && method.IsStatic )
                            {
                                if ( fliObject.Get( "Arguments" ).ObjectData is FastMemorySpace instance )
                                {
                                    List < object > args = new List < object >();

                                    foreach ( DynamicBiteVariable instanceProperty in instance.Properties )
                                    {
                                        args.Add( instanceProperty.ToObject() );
                                    }

                                    return method.Invoke( null, args.ToArray() );
                                }

                                return method.Invoke( null, null );
                            }

                            if ( method != null && !method.IsStatic )
                            {
                                if ( fliObject.Get( "ObjectInstance" ) is object objectInstance )
                                {
                                    if ( fliObject.Get( "Arguments" ).ObjectData is FastMemorySpace instance )
                                    {
                                        List < object > args = new List < object >();

                                        foreach ( DynamicBiteVariable instanceProperty in instance.Properties )
                                        {
                                            args.Add( instanceProperty.ToObject() );
                                        }

                                        return method.Invoke( objectInstance, args.ToArray() );
                                    }

                                    return method.Invoke( objectInstance, null );
                                }
                                else
                                {
                                    List < Type > constructorArgTypes = new List < Type >();

                                    if ( fliObject.Get( "ConstructorArguments" ).ObjectData is FastMemorySpace
                                        constructorInstanceTypes )
                                    {
                                        foreach ( DynamicBiteVariable instanceProperty in
                                                 constructorInstanceTypes.Properties )
                                        {
                                            constructorArgTypes.Add( instanceProperty.GetType() );
                                        }
                                    }

                                    List < object > constructorArgs = new List < object >();

                                    if ( fliObject.Get( "ConstructorArguments" ).ObjectData is FastMemorySpace
                                        constructorInstance )
                                    {
                                        foreach ( DynamicBiteVariable instanceProperty in constructorInstance.
                                                     Properties )
                                        {
                                            constructorArgs.Add( instanceProperty.ToObject() );
                                        }
                                    }

                                    ConstructorInfo constructor = m_TypeRegistry.GetConstructor(
                                        type,
                                        constructorArgTypes.ToArray() );

                                    object classObject = constructor.Invoke( constructorArgs.ToArray() );

                                    fliObject.Put(
                                        "ObjectInstance",
                                        DynamicVariableExtension.ToDynamicVariable( classObject ) );

                                    if ( fliObject.Get( "Arguments" ).ObjectData is FastMemorySpace instance )
                                    {
                                        List < object > args = new List < object >();

                                        foreach ( DynamicBiteVariable instanceProperty in instance.Properties )
                                        {
                                            args.Add( instanceProperty.ToObject() );
                                        }

                                        return method.Invoke( classObject, args.ToArray() );
                                    }

                                    return method.Invoke( classObject, null );
                                }
                            }

                            if ( type.IsSealed && type.IsAbstract )
                            {
                                StaticWrapper wrapper = new StaticWrapper( type );

                                fliObject.Put(
                                    "ObjectInstance",
                                    DynamicVariableExtension.ToDynamicVariable( wrapper ) );

                                return wrapper;
                            }

                            {
                                List < Type > constructorArgTypes = new List < Type >();

                                if ( fliObject.Get( "ConstructorArguments" ).ObjectData is FastMemorySpace
                                    constructorInstanceTypes )
                                {
                                    foreach ( DynamicBiteVariable instanceProperty in constructorInstanceTypes.
                                                 Properties )
                                    {
                                        constructorArgTypes.Add( instanceProperty.GetType() );
                                    }
                                }

                                List < object > constructorArgs = new List < object >();

                                if ( fliObject.Get( "ConstructorArguments" ).ObjectData is FastMemorySpace
                                    constructorInstance )
                                {
                                    foreach ( DynamicBiteVariable instanceProperty in constructorInstance.
                                                 Properties )
                                    {
                                        constructorArgs.Add( instanceProperty.ToObject() );
                                    }
                                }

                                if ( constructorArgs.Count == 0 )
                                {
                                    ConstructorInfo constructor = m_TypeRegistry.GetConstructor( type );
                                    object classObject = constructor.Invoke( new object[] { } );

                                    fliObject.Put(
                                        "ObjectInstance",
                                        DynamicVariableExtension.ToDynamicVariable( classObject ) );

                                    return classObject;
                                }
                                else
                                {
                                    ConstructorInfo constructor = m_TypeRegistry.GetConstructor(
                                        type,
                                        constructorArgTypes.ToArray() );

                                    object classObject = constructor.Invoke( constructorArgs.ToArray() );

                                    fliObject.Put(
                                        "ObjectInstance",
                                        DynamicVariableExtension.ToDynamicVariable( classObject ) );

                                    return classObject;
                                }
                            }
                        }

                        if ( type.IsSealed && type.IsAbstract )
                        {
                            StaticWrapper wrapper = new StaticWrapper( type );
                            fliObject.Put( "ObjectInstance", DynamicVariableExtension.ToDynamicVariable( wrapper ) );

                            return wrapper;
                        }

                        {
                            List < Type > constructorArgTypes = new List < Type >();

                            FastMemorySpace constructorArguments =
                                ( FastMemorySpace ) fliObject.Get( "ConstructorArguments" ).ObjectData;

                            FastMemorySpace constructorArgumentsTypes =
                                ( FastMemorySpace ) fliObject.Get( "ConstructorArgumentsTypes" ).ObjectData;

                            if ( constructorArguments != null && constructorArguments.NamesToProperties.Count > 0 )
                            {
                                foreach ( KeyValuePair < string, DynamicBiteVariable >
                                             constructorArgumentsNamesToProperty
                                         in constructorArgumentsTypes.NamesToProperties )
                                {
                                    if ( constructorArgumentsNamesToProperty.Key != "this" )
                                    {
                                        constructorArgTypes.Add(
                                            ResolveType( constructorArgumentsNamesToProperty.Value.StringData ) );
                                    }
                                }
                            }

                            List < object > constructorArgs = new List < object >();

                            if ( constructorArguments != null )
                            {
                                int i = 0;

                                foreach ( KeyValuePair < string, DynamicBiteVariable >
                                             constructorArgumentsNamesToProperty
                                         in constructorArguments.NamesToProperties )
                                {
                                    if ( constructorArgumentsNamesToProperty.Key != "this" )
                                    {
                                        constructorArgs.Add(
                                            Convert.ChangeType(
                                                constructorArgumentsNamesToProperty.Value.ToObject(),
                                                constructorArgTypes[i] ) );

                                        i++;
                                    }
                                }
                            }

                            if ( constructorArgs.Count == 0 )
                            {
                                ConstructorInfo constructor = m_TypeRegistry.GetConstructor( type );
                                object classObject = constructor.Invoke( new object[] { } );

                                fliObject.Put(
                                    "ObjectInstance",
                                    DynamicVariableExtension.ToDynamicVariable( classObject ) );

                                return classObject;
                            }
                            else
                            {
                                ConstructorInfo constructor = m_TypeRegistry.GetConstructor(
                                    type,
                                    constructorArgTypes.ToArray() );

                                object classObject = constructor.Invoke( constructorArgs.ToArray() );

                                fliObject.Put(
                                    "ObjectInstance",
                                    DynamicVariableExtension.ToDynamicVariable( classObject ) );

                                return classObject;
                            }
                        }
                    }
                }
            }
            else if (  arguments.Count == 1 && arguments[0].DynamicType == DynamicVariableType.String )
            {
                Type type = ResolveType( arguments[0].StringData );

                StaticWrapper wrapper = new StaticWrapper( type );

                return wrapper;
            }
            else if (  arguments.Count > 1 && arguments[0].DynamicType == DynamicVariableType.String && arguments[1].DynamicType == DynamicVariableType.String )
            {
                Type type = ResolveType( arguments[0].StringData );

                MemberInfo[] memberInfo = type.GetMember( arguments[1].StringData, BindingFlags.Public | BindingFlags.Static );

                if ( memberInfo.Length > 0 )
                {
                    object obj = GetValue( memberInfo[0], null );
                    return obj;
                }
            }

            return null;
        }

        return null;
    }

    public Type ResolveType( string name )
    {
        if ( m_TypeRegistry == null || !m_TypeRegistry.TryResolveType( name, out Type type ) )
        {
            type = Type.GetType( name );
        }

        return type;
    }

    public static object GetValue(MemberInfo memberInfo, object forObject)
    {
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field:
                return ((FieldInfo)memberInfo).GetValue(forObject);
            case MemberTypes.Property:
                return ((PropertyInfo)memberInfo).GetValue(forObject);
            default:
                throw new NotImplementedException();
        }
    } 
    #endregion
}

}
