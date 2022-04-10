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
            defNode = new FunctionDeclarationNode
            {
                FunctionId = new Identifier( name ),
                Parameters = new ParametersNode
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
            DefinitionNode = null
        };
    }

    protected FieldSymbol CreatePublicField( string name, string type, HeteroAstNode declaringType )
    {
        return new FieldSymbol(
            name,
            AccesModifierType.Public,
            ClassAndMemberModifiers.None )
        {
            Type = new BiteClassType( type ),
            DefinitionNode = declaringType
        };
    }
}

}