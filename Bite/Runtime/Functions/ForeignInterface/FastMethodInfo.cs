using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class FastMethodInfo
{
    private delegate object ReturnValueDelegate( object instance, object[] arguments );

    private delegate void VoidDelegate( object instance, object[] arguments );

    private ReturnValueDelegate Delegate { get; }

    #region Public

    public FastMethodInfo( MethodInfo methodInfo )
    {
        ParameterExpression instanceExpression = Expression.Parameter( typeof( object ), "instance" );
        ParameterExpression argumentsExpression = Expression.Parameter( typeof( object[] ), "arguments" );
        List < Expression > argumentExpressions = new List < Expression >();
        ParameterInfo[] parameterInfos = methodInfo.GetParameters();

        for ( int i = 0; i < parameterInfos.Length; ++i )
        {
            ParameterInfo parameterInfo = parameterInfos[i];

            argumentExpressions.Add(
                Expression.Convert(
                    Expression.ArrayIndex( argumentsExpression, Expression.Constant( i ) ),
                    parameterInfo.ParameterType ) );
        }

        MethodCallExpression callExpression = Expression.Call(
            !methodInfo.IsStatic ? Expression.Convert( instanceExpression, methodInfo.ReflectedType ) : null,
            methodInfo,
            argumentExpressions );

        if ( callExpression.Type == typeof( void ) )
        {
            VoidDelegate voidDelegate = Expression.Lambda < VoidDelegate >(
                                                       callExpression,
                                                       instanceExpression,
                                                       argumentsExpression ).
                                                   Compile();

            Delegate = ( instance, arguments ) =>
            {
                voidDelegate( instance, arguments );

                return null;
            };
        }
        else
        {
            Delegate = Expression.Lambda < ReturnValueDelegate >(
                                      Expression.Convert( callExpression, typeof( object ) ),
                                      instanceExpression,
                                      argumentsExpression ).
                                  Compile();
        }
    }

    public object Invoke( object instance, params object[] arguments )
    {
        return Delegate( instance, arguments );
    }

    #endregion
}

public class FastPropertyInfo < T >
{
    public delegate object ReturnValueDelegate( T instance, object[] arguments );

    public delegate void UpdateValueDelegate( T instance, object value, object[] arguments );

    public ReturnValueDelegate GetterDelegate { get; }

    public UpdateValueDelegate SetterDelegate { get; }

    #region Public

    public FastPropertyInfo( PropertyInfo propertyInfo )
    {
        ParameterExpression instanceExpression = Expression.Parameter( typeof( T ), "instance" );
        ParameterExpression valueExpression = Expression.Parameter( typeof( object ), "value" );
        ParameterExpression argumentsExpression = Expression.Parameter( typeof( object[] ), "arguments" );
        List < Expression > argumentExpressions = new List < Expression >();
        ParameterInfo[] parameterInfos = propertyInfo.GetIndexParameters();

        for ( int i = 0; i < parameterInfos.Length; ++i )
        {
            ParameterInfo parameterInfo = parameterInfos[i];

            argumentExpressions.Add(
                Expression.Convert(
                    Expression.ArrayIndex( argumentsExpression, Expression.Constant( i ) ),
                    parameterInfo.ParameterType ) );
        }

        IndexExpression callExpression = Expression.Property(
            instanceExpression,
            propertyInfo,
            argumentExpressions );

        BinaryExpression assignExpression = Expression.Assign(
            callExpression,
            Expression.Convert( valueExpression, propertyInfo.PropertyType ) );

        GetterDelegate = Expression.Lambda < ReturnValueDelegate >(
                                        Expression.Convert( callExpression, typeof( object ) ),
                                        instanceExpression,
                                        argumentsExpression ).
                                    Compile();

        SetterDelegate = Expression.Lambda < UpdateValueDelegate >(
                                        assignExpression,
                                        instanceExpression,
                                        valueExpression,
                                        argumentsExpression ).
                                    Compile();
    }

    public object InvokeGet( object instance, params object[] arguments )
    {
        return GetterDelegate( ( T ) instance, arguments );
    }

    public void InvokeSet( object instance, object value, params object[] arguments )
    {
        SetterDelegate( ( T ) instance, value, arguments );
    }

    #endregion
}

public class FastFieldInfo
{
    private delegate object ReturnValueDelegate( object instance );

    private delegate void UpdateValueDelegate( object instance, object value );

    private UpdateValueDelegate SetDelegate { get; }

    private ReturnValueDelegate Delegate { get; }

    #region Public

    public FastFieldInfo( FieldInfo propertyInfo )
    {
        ParameterExpression instanceExpression = Expression.Parameter( typeof( object ), "instance" );
        ParameterExpression valueExpression = Expression.Parameter( typeof( object ), "value" );

        MemberExpression callExpression = Expression.Field(
            !propertyInfo.IsStatic ? Expression.Convert( instanceExpression, propertyInfo.ReflectedType ) : null,
            propertyInfo );

        Delegate = Expression.Lambda < ReturnValueDelegate >(
                                  Expression.Convert( callExpression, typeof( object ) ),
                                  instanceExpression ).
                              Compile();

        BinaryExpression assignExpression = Expression.Assign(
            callExpression,
            Expression.Convert( valueExpression, propertyInfo.FieldType ) );

        SetDelegate = Expression.Lambda < UpdateValueDelegate >(
                                     assignExpression,
                                     instanceExpression,
                                     valueExpression ).
                                 Compile();

        FieldType = propertyInfo.FieldType;
    }

    public Type FieldType { get; set; }
    public object GetField( object instance )
    {
        return Delegate( instance );
    }

    public void SetField( object instance, object value )
    {
        SetDelegate( instance, value );
    }

    #endregion
}

public interface IFastPropertyInfo
{
    Type PropertyType { get; set; }
    object InvokeGet( object instance, params object[] arguments );

    void InvokeSet( object instance, object value, params object[] arguments );
}

public class CachedProperty<T>: IFastPropertyInfo
{
    private FastPropertyInfo <T>.ReturnValueDelegate GetterDelegate { get; }

    private FastPropertyInfo <T>.UpdateValueDelegate SetterDelegate { get; }

    public CachedProperty(FastPropertyInfo <T>.ReturnValueDelegate returnValueDelegate,  FastPropertyInfo <T>.UpdateValueDelegate updateValueDelegate)
    {
        GetterDelegate = returnValueDelegate;
        SetterDelegate = updateValueDelegate;
        PropertyType = typeof( T );
    }

    public Type PropertyType { get; set; }

    public object InvokeGet( object instance, params object[] arguments )
    {
        return GetterDelegate((T)instance, arguments);
    }

    public void InvokeSet( object instance, object value, params object[] arguments )
    {
        SetterDelegate( ( T ) instance, value, arguments );
    }
}
}
