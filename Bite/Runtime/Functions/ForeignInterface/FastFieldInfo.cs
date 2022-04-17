using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Bite.Runtime.Functions.ForeignInterface
{

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
            !propertyInfo.IsStatic ? propertyInfo.ReflectedType.IsValueType
                ? Expression.Unbox(instanceExpression, propertyInfo.ReflectedType)
                : Expression.Convert(instanceExpression, propertyInfo.ReflectedType) : null,
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

}
