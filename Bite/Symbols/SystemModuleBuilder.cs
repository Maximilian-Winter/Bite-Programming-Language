using Bite.Ast;

namespace Bite.Symbols
{

/// <summary>
/// Builds the built-in System Module
///// </summary>
//public class SystemModuleBuilder : ModuleBuilder
//{

//    public ModuleSymbol BuildSystemModule()
//    {
//        ModuleSymbol moduleSymbol = new ModuleSymbol( "System" );

//        moduleSymbol.DefineClass( BuildObjectClass() );

//        moduleSymbol.DefineFunction( BuildPrintFunction() );
//        moduleSymbol.DefineFunction( BuildPrintLineFunction() );
//        moduleSymbol.DefineFunction( BuildCSharpInterfaceFunction() );

//        moduleSymbol.DefineClass( BuildCSharpInterfaceClass() );

//        return moduleSymbol;
//    }

//    private ClassSymbol BuildObjectClass()
//    {
//        BiteClassType classType = new BiteClassType( "Object" );

//        ClassSymbol classSymbol = new ClassSymbol(
//            "Object",
//            AccesModifierType.Public,
//            ClassAndMemberModifiers.None );

//        classSymbol.typeIndex = classType.TypeIndex;
//        classSymbol.m_DefinitionNode = null;

//        return classSymbol;
//    }

//    private FunctionSymbol BuildPrintFunction()
//    {
//        return CreateFunction( "Print",
//            AccesModifierType.Public,
//            ClassAndMemberModifiers.None,
//            "Object",
//            new[] { "x" } );
//    }

//    private FunctionSymbol BuildPrintLineFunction()
//    {
//        return CreateFunction( "PrintLine",
//            AccesModifierType.Public,
//            ClassAndMemberModifiers.None,
//            "Object",
//            new[] { "x" } );
//    }


//    private FunctionSymbol BuildCSharpInterfaceFunction()
//    {
//        return CreateFunction( "CSharpInterfaceCall",
//            AccesModifierType.Public,
//            ClassAndMemberModifiers.None,
//            "Object",
//            new[] { "x" } );
//    }

//    private ClassSymbol BuildCSharpInterfaceClass()
//    {
//        BiteClassType classTypeFli = new BiteClassType( "CSharpInterface" );

//        ClassSymbol classSymbolFli = new ClassSymbol(
//            "CSharpInterface",
//            AccesModifierType.Public,
//            ClassAndMemberModifiers.None )
//        {
//            typeIndex = classTypeFli.TypeIndex,
//            m_DefinitionNode = null
//        };

//        var typeDecl = new ClassInstanceDeclarationNode
//        {
//            AstScopeNode = classSymbolFli,
//            ClassName = new Identifier( "Object" ),
//        };

//        var typeFieldSymbol = CreatePublicField( "Type", "Object" );
//        var methodFieldSymbol = CreatePublicField( "Method", "Object" );
//        var argumentsFieldSymbol = CreatePublicField( "Arguments", "Object", typeDecl );
//        var constructorArgumentsFieldSymbol = CreatePublicField( "ConstructorArguments", "Object", typeDecl );

//        var constructorArgumentsTypesFieldSymbol =
//            CreatePublicField( "ConstructorArgumentsTypes", "Object", typeDecl );

//        var objectInstanceFieldSymbol = CreatePublicField( "ObjectInstance", "Object" );

//        classSymbolFli.DefineField( typeFieldSymbol );
//        classSymbolFli.DefineField( methodFieldSymbol );
//        classSymbolFli.DefineField( argumentsFieldSymbol );
//        classSymbolFli.DefineField( constructorArgumentsFieldSymbol );
//        classSymbolFli.DefineField( constructorArgumentsTypesFieldSymbol );
//        classSymbolFli.DefineField( objectInstanceFieldSymbol );

//        return classSymbolFli;
//    }

//}

}