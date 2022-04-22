using System;
using System.Reflection;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.Interop
{

public class InteropGetConstructor : InteropBase, IBiteVmCallable
    {

    #region Public

    public InteropGetConstructor()
    {
    }

    public InteropGetConstructor( TypeRegistry typeRegistry ) : base( typeRegistry )
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

        Type[] constructorArgTypes = new Type[arguments.Length - 1];

        int counter = 0;

        for ( int i = 1; i < arguments.Length; i++ )
        {
            if ( arguments[i].DynamicType == DynamicVariableType.String )
            {
                Type argType = ResolveType( arguments[i].StringData );

                if ( argType == null )
                {
                    throw new BiteVmRuntimeException(
                        $"Runtime Error: Type: {arguments[i].StringData} not registered as a type!" );
                }

                constructorArgTypes[counter] = argType;

            }
            else
            {
                throw new BiteVmRuntimeException( "Expected string" );
            }

            counter++;
        }

        ConstructorInfo constructorInfo = m_TypeRegistry.GetConstructor( type, constructorArgTypes );

        return new ConstructorInvoker( constructorInfo );
    }
    

    #endregion
}

}