using System.Collections.Generic;
using Bite.Ast;

namespace Bite.Symbols
{

public class SymbolTable
{
    public SymbolTable( )
    {
    }

    public  void Initialize( Dictionary<string, object> externalObjects = null )
    {
        GlobalScope g = new GlobalScope( null );
        PushScope( g );

        if (externalObjects != null)
        {
            foreach (var module in externalObjects)
            {
                VariableSymbol variableSymbol =
                    new VariableSymbol( module.Key, AccesModifierType.Public, ClassAndMemberModifiers.Static, true );
                CurrentScope.define( variableSymbol );
            }
        }

        ModuleSymbol m = new ModuleSymbol(
            "System",
            new List<ModuleIdentifier>(),
            new List<ModuleIdentifier>() );

        m.EnclosingScope = CurrentScope;
        PushScope( m );
        BiteClassType classType = new BiteClassType( "Object" );

        ClassSymbol classSymbol = new ClassSymbol(
            "Object",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        classSymbol.EnclosingScope = CurrentScope;
        classSymbol.typeIndex = classType.TypeIndex;
        classSymbol.m_DefinitionNode = null;
        CurrentScope.define( classSymbol );

        FunctionSymbol functionSymbol = new FunctionSymbol(
            "Print",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        ParameterSymbol parameterSymbol = new ParameterSymbol( "x" );
        BiteClassType functionType = new BiteClassType( "Object" );

        functionSymbol.Type = functionType;
        functionSymbol.EnclosingScope = CurrentScope;
        FunctionDeclarationNode functionDeclarationNode = new FunctionDeclarationNode();
        functionDeclarationNode.FunctionId = new Identifier( "Print" );
        functionDeclarationNode.Parameters = new ParametersNode();
        functionDeclarationNode.Parameters.Identifiers = new List<Identifier>();
        functionDeclarationNode.Parameters.Identifiers.Add( new Identifier( "x" ) );
        functionSymbol.defNode = functionDeclarationNode;
        PushScope( functionSymbol );

        functionSymbol.define( parameterSymbol );

        PopScope();
        CurrentScope.define( functionSymbol );

        functionSymbol = new FunctionSymbol(
            "PrintLine",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        parameterSymbol = new ParameterSymbol( "x" );
        functionType = new BiteClassType( "Object" );

        functionSymbol.Type = functionType;
        functionSymbol.EnclosingScope = CurrentScope;
        functionDeclarationNode = new FunctionDeclarationNode();
        functionDeclarationNode.FunctionId = new Identifier( "PrintLine" );
        functionDeclarationNode.Parameters = new ParametersNode();
        functionDeclarationNode.Parameters.Identifiers = new List<Identifier>();
        functionDeclarationNode.Parameters.Identifiers.Add( new Identifier( "x" ) );
        functionSymbol.defNode = functionDeclarationNode;
        PushScope( functionSymbol );

        functionSymbol.define( parameterSymbol );

        PopScope();
        CurrentScope.define( functionSymbol );

        functionSymbol = new FunctionSymbol(
            "CSharpInterfaceCall",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        ParameterSymbol parameterCSharpInterfaceSymbol = new ParameterSymbol( "x" );
        BiteClassType functionTypeCSharpInterface = new BiteClassType( "Object" );

        functionSymbol.Type = functionTypeCSharpInterface;
        functionSymbol.EnclosingScope = CurrentScope;
        functionDeclarationNode = new FunctionDeclarationNode();
        functionDeclarationNode.FunctionId = new Identifier( "CSharpInterfaceCall" );
        functionDeclarationNode.Parameters = new ParametersNode();
        functionDeclarationNode.Parameters.Identifiers = new List<Identifier>();
        functionDeclarationNode.Parameters.Identifiers.Add( new Identifier( "x" ) );
        functionSymbol.defNode = functionDeclarationNode;
        PushScope( functionSymbol );

        functionSymbol.define( parameterCSharpInterfaceSymbol );

        PopScope();
        CurrentScope.define( functionSymbol );

        BiteClassType classTypeFli = new BiteClassType( "CSharpInterface" );

        ClassSymbol classSymbolFli = new ClassSymbol(
            "CSharpInterface",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        classSymbolFli.EnclosingScope = CurrentScope;
        classSymbolFli.typeIndex = classTypeFli.TypeIndex;
        classSymbolFli.m_DefinitionNode = null;

        PushScope( classSymbolFli );
        FieldSymbol fieldSymbol = new FieldSymbol( "Type", AccesModifierType.Public, ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = null;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol( "Method", AccesModifierType.Public, ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = null;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol( "Arguments", AccesModifierType.Public, ClassAndMemberModifiers.None );

        ClassInstanceDeclarationNode typeDecl = new ClassInstanceDeclarationNode();
        typeDecl.AstScopeNode = CurrentScope;
        typeDecl.ClassName = new Identifier( "Object" );
        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = typeDecl;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol(
            "ConstructorArguments",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = typeDecl;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol(
            "ConstructorArgumentsTypes",
            AccesModifierType.Public,
            ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = typeDecl;
        CurrentScope.define( fieldSymbol );

        fieldSymbol = new FieldSymbol( "ObjectInstance", AccesModifierType.Public, ClassAndMemberModifiers.None );

        fieldSymbol.Type = new BiteClassType( "Object" );
        fieldSymbol.DefinitionNode = null;
        CurrentScope.define( fieldSymbol );

        PopScope();
        CurrentScope.define( classSymbolFli );
        PopScope();
        CurrentScope.define( m );

        RootScope = CurrentScope as BaseScope;
    }

    public void PopScope()
    {
        CurrentScope = CurrentScope.EnclosingScope;
    }

    public void PushScope( Scope s )
    {
        CurrentScope = s;
    }

    internal Scope CurrentScope { get; private set; }
    
    public BaseScope RootScope { get; private set; }
}

}