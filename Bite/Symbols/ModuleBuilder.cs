using System.Collections.Generic;
using System.Linq;
using Bite.Ast;

namespace Bite.Symbols
{

/// <summary>
/// Contains helper methods for building module members
/// </summary>
public abstract class ModuleBuilder
{
    protected FunctionSymbol CreateFunction( string name,
        AccesModifierType accesModifier,
        ClassAndMemberModifiers memberModifiers, 
        bool isExtern, 
        bool isCallable, 
        string type,
        IReadOnlyCollection < string > parameters )
    {
        var functionSymbol = new FunctionSymbol(
            name,
            accesModifier,
            memberModifiers, 
            isExtern,
            isCallable
            )
        {
            Type = new BiteClassType( type ),
            m_DefBaseNode = new FunctionDeclarationBaseNode
            {
                FunctionId = new Identifier( name ),
                ParametersBase = new ParametersBaseNode
                {
                    Identifiers = parameters.Select( p => new Identifier( p ) ).ToList()
                }
            }
        };

        foreach ( var parameter in parameters )
        {
            functionSymbol.define( new ParameterSymbol( parameter ) );
        }

        return functionSymbol;
    }

    protected FieldSymbol CreatePublicField( string name, string type )
    {
        return new FieldSymbol(
            name,
            AccesModifierType.Public,
            ClassAndMemberModifiers.None )
        {
            Type = new BiteClassType( type ),
            DefinitionBaseNode = null
        };
    }

    protected FieldSymbol CreatePublicField( string name, string type, AstBaseNode declaringType )
    {
        return new FieldSymbol(
            name,
            AccesModifierType.Public,
            ClassAndMemberModifiers.None )
        {
            Type = new BiteClassType( type ),
            DefinitionBaseNode = declaringType
        };
    }
}

}