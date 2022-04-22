using System;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.Interop
{

public class InteropGetStaticClass : InteropBase, IBiteVmCallable
{
    public InteropGetStaticClass()
    {
    }

    public InteropGetStaticClass( TypeRegistry typeRegistry ) : base( typeRegistry )
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

        StaticWrapper wrapper = new StaticWrapper( type );

        return wrapper;
    }
}

}