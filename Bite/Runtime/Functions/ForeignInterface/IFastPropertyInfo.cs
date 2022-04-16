using System;

namespace Bite.Runtime.Functions.ForeignInterface
{

public interface IFastPropertyInfo
{
    Type PropertyType { get; set; }
    object InvokeGet( object instance, params object[] arguments );

    void InvokeSet( object instance, object value, params object[] arguments );
}

}
