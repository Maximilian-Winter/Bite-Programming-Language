using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Bite.Runtime.Functions.ForeignInterface
{

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

}
