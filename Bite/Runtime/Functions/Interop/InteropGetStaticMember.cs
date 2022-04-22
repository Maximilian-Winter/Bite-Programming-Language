using System;
using System.Reflection;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.Interop
{

public class InteropGetStaticMember : InteropBase, IBiteVmCallable
{
    public InteropGetStaticMember()
    {
    }

    public InteropGetStaticMember( TypeRegistry typeRegistry ) : base( typeRegistry )
    {
    }

    public object Call( DynamicBiteVariable[] arguments )
    {
        Type type = ResolveType( arguments[0].StringData );

        if ( type == null )
        {
            throw new BiteVmRuntimeException(
                $"Runtime Error: Type: {arguments[0].StringData} not registered as a type!" );
        }

        MemberInfo[] memberInfo =
            type.GetMember( arguments[1].StringData, BindingFlags.Public | BindingFlags.Static );

        if ( memberInfo.Length > 0 )
        {
            object obj = GetValue( memberInfo[0], null );

            return obj;
        }

        throw new BiteVmRuntimeException(
            $"Runtime Error: member {arguments[0].StringData} not found on type {arguments[0].StringData}" );
    }

    private static object GetValue( MemberInfo memberInfo, object forObject )
    {
        switch ( memberInfo.MemberType )
        {
            case MemberTypes.Field:
                return ( ( FieldInfo ) memberInfo ).GetValue( forObject );

            case MemberTypes.Property:
                return ( ( PropertyInfo ) memberInfo ).GetValue( forObject );

            default:
                throw new NotImplementedException();
        }
    }
}

}