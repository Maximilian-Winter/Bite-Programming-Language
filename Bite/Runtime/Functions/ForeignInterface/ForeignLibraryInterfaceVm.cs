using System;
using System.Collections.Generic;
using System.Reflection;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class ForeignLibraryInterfaceVm : IBiteVmCallable
{
    #region Public

    public object Call( List < DynamicBiteVariable > arguments )
    {
        if ( arguments.Count > 0 )
        {
            if ( arguments[0].ObjectData is FastMemorySpace fliObject )
            {
                string typeString = fliObject.Get( "Type" ).StringData;

                if ( !string.IsNullOrEmpty( typeString ) )
                {
                    Type type = Type.GetType( typeString );
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

                        MethodInfo method = type.GetMethod( methodString.StringData, argTypes.ToArray() );

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

                                ConstructorInfo constructor = type.GetConstructor( constructorArgTypes.ToArray() );
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
                            fliObject.Put( "ObjectInstance", DynamicVariableExtension.ToDynamicVariable( wrapper ) );

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
                                ConstructorInfo constructor = type.GetConstructor( Type.EmptyTypes );
                                object classObject = constructor.Invoke( new object[] { } );

                                fliObject.Put(
                                    "ObjectInstance",
                                    DynamicVariableExtension.ToDynamicVariable( classObject ) );

                                return classObject;
                            }
                            else
                            {
                                ConstructorInfo constructor = type.GetConstructor( constructorArgTypes.ToArray() );
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

                        if ( constructorArguments != null && constructorArguments.CurrentMemoryPointer > 0 )
                        {
                            for ( int i = 0; i < constructorArguments.CurrentMemoryPointer; i++ )
                            {
                                constructorArgTypes.Add(
                                    Type.GetType( constructorArgumentsTypes.Properties[i].StringData ) );
                            }
                        }
                        else if ( constructorArguments != null && constructorArguments.NamesToProperties.Count > 0 )
                        {
                            foreach ( KeyValuePair < string, DynamicBiteVariable > constructorArgumentsNamesToProperty
                                     in constructorArgumentsTypes.NamesToProperties )
                            {
                                constructorArgTypes.Add(
                                    Type.GetType( constructorArgumentsNamesToProperty.Value.StringData ) );
                            }
                        }

                        List < object > constructorArgs = new List < object >();

                        if ( constructorArguments != null && constructorArguments.CurrentMemoryPointer > 0 )
                        {
                            for ( int i = 0; i < constructorArguments.CurrentMemoryPointer; i++ )
                            {
                                constructorArgs.Add(
                                    Convert.ChangeType(
                                        constructorArguments.Properties[i].ToObject(),
                                        constructorArgTypes[i] ) );
                            }
                        }
                        else if ( constructorArguments != null && constructorArguments.NamesToProperties.Count > 0 )
                        {
                            int i = 0;

                            foreach ( KeyValuePair < string, DynamicBiteVariable > constructorArgumentsNamesToProperty
                                     in constructorArguments.NamesToProperties )
                            {
                                constructorArgs.Add(
                                    Convert.ChangeType(
                                        constructorArgumentsNamesToProperty.Value.ToObject(),
                                        constructorArgTypes[i] ) );

                                i++;
                            }
                        }

                        if ( constructorArgs.Count == 0 )
                        {
                            ConstructorInfo constructor = type.GetConstructor( Type.EmptyTypes );
                            object classObject = constructor.Invoke( new object[] { } );

                            fliObject.Put(
                                "ObjectInstance",
                                DynamicVariableExtension.ToDynamicVariable( classObject ) );

                            return classObject;
                        }
                        else
                        {
                            ConstructorInfo constructor = type.GetConstructor( constructorArgTypes.ToArray() );

                            object classObject = constructor.Invoke( constructorArgs.ToArray() );

                            fliObject.Put(
                                "ObjectInstance",
                                DynamicVariableExtension.ToDynamicVariable( classObject ) );

                            return classObject;
                        }
                    }
                }
            }

            return null;
        }

        return null;
    }

    #endregion
}

}
