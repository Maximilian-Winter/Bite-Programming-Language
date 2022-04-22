using System;

namespace Bite.Runtime.Functions.ForeignInterface
{

public abstract class InteropBase
{
    protected readonly TypeRegistry m_TypeRegistry;

    protected InteropBase()
    {
        m_TypeRegistry = new TypeRegistry();
    }

    protected InteropBase( TypeRegistry typeRegistry )
    {
        m_TypeRegistry = typeRegistry;
    }


    public Type ResolveType( string name )
    {
        if (m_TypeRegistry == null || !m_TypeRegistry.TryResolveType( name, out Type type ))
        {
            type = Type.GetType( name );
        }

        return type;
    }

}

}