using System;

namespace Bite.Runtime.Functions.ForeignInterface
{

public class CachedProperty<T>: IFastPropertyInfo
{
    private FastPropertyInfo <T>.ReturnValueDelegate GetterDelegate { get; }

    private FastPropertyInfo <T>.UpdateValueDelegate SetterDelegate { get; }

    public CachedProperty(FastPropertyInfo <T>.ReturnValueDelegate returnValueDelegate,  FastPropertyInfo <T>.UpdateValueDelegate updateValueDelegate)
    {
        GetterDelegate = returnValueDelegate;
        SetterDelegate = updateValueDelegate;
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
