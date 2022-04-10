using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using AntlrBiteParser;
using Bite.Ast;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Bite.Symbols;
using MemoizeSharp;

namespace Bite.Compiler
{

public class BiteCompiler
{
    // TODO: Move somewhere else?
    /// <summary>
    /// System Module Declaration
    /// </summary>
    public string SystemModule = @"module System;

class CSharpInterface {
    var Type;
    var Method;
    var Arguments;
    var ConstructorArguments;
    var ConstructorArgumentsTypes;
    var ObjectInstance;
}

extern callable function CSharpInterfaceCall ( object ) {
}

extern callable function Print ( object ) {
}

extern callable function PrintLine ( object ) {
}";

    #region Public Compilers

    public BiteProgram Compile( IEnumerable < string > modules )
    {
        ProgramNode program = ParseModules( modules );

        return CompileProgramInternal( program );
    }

    public BiteProgram CompileExpression( string expression )
    {
        ExpressionNode expressionNode = ParseExpression( expression );

        return CompileExpressionInternal( expressionNode );
    }

    public BiteProgram CompileStatementsWithSymbolTable( string statements, SymbolTable symbolTable )
    {
        IReadOnlyCollection < StatementNode > statementNodes = ParseStatements( statements );

        return CompileStatementsInternal( statementNodes, null, symbolTable );
    }

    public BiteProgram CompileStatements( string statements, Dictionary < string, object > externalObjects )
    {
        IReadOnlyCollection < StatementNode > statementNodes = ParseStatements( statements );

        return CompileStatementsInternal( statementNodes, externalObjects );
    }


    public BiteProgram Compile( IReadOnlyCollection < Module > modules )
    {
        var moduleStrings = new List < string >();

        foreach ( var module in modules )
        {
            var moduleBuilder = new StringBuilder();
            moduleBuilder.AppendLine( $"module {module.Name};\r\n" );

            foreach ( var import in module.Imports )
            {
                moduleBuilder.AppendLine( $"import {import};" );
                moduleBuilder.AppendLine( $"using {import};" );
            }

            moduleBuilder.AppendLine();
            moduleBuilder.AppendLine( module.Code );

            moduleStrings.Add( moduleBuilder.ToString() );
        }

        ProgramNode program = ParseModules( moduleStrings );

        return CompileProgramInternal( program );
    }

    #endregion

    #region Parsers

    private ProgramNode ParseModules( IEnumerable < string > modules )
    {
        ProgramNode program = new ProgramNode();

        ModuleNode systemModule = ParseModule( SystemModule );
        program.AddModule( systemModule );

        foreach (string biteModule in modules)
        {
            ModuleNode module = ParseModule( biteModule );
            program.AddModule( module );
        }

        return program;
    }

    private ModuleNode ParseModule( string module )
    {
        HeteroAstGenerator gen = new HeteroAstGenerator();

        BITEParser biteParser = CreateBiteParser( module, out var errorListener );

        BITEParser.ModuleContext tree = biteParser.program().module( 0 );

        if ( errorListener.Errors.Count > 0 )
        {
            module = module.Trim();
            int start = module.IndexOf( "module" ) + "module ".Length;
            var moduleName = module.Substring( start, module.IndexOf( ';' ) - start );

            throw new BiteCompilerException(
                $"Error occured while parsing module '{moduleName}' .\r\nError Count: {errorListener.Errors.Count}",
                errorListener.Errors );
        }

        return ( ModuleNode ) gen.VisitModule( tree );
    }

    private ExpressionNode ParseExpression( string expression )
    {
        BITEParser biteParser = CreateBiteParser( expression, out var errorListener );

        BITEParser.ExpressionContext tree = biteParser.expression();

        if ( errorListener.Errors.Count > 0 )
        {
            throw new BiteCompilerException(
                $"Error occured while parsing expression.\r\nError Count: {errorListener.Errors.Count}",
                errorListener.Errors );
        }

        HeteroAstGenerator gen = new HeteroAstGenerator();

        return ( ExpressionNode ) gen.VisitExpression( tree );
    }

    private IReadOnlyCollection < StatementNode > ParseStatements( string statements )
    {
        BITEParser biteParser = CreateBiteParser( statements, out var errorListener );

        BITEParser.StatementsContext tree = biteParser.statements();

        if ( errorListener.Errors.Count > 0 )
        {
            throw new BiteCompilerException(
                $"Error occured while parsing statement.\r\nError Count: {errorListener.Errors.Count}",
                errorListener.Errors );
        }

        HeteroAstGenerator gen = new HeteroAstGenerator();

        DeclarationsNode declarations = ( DeclarationsNode ) gen.VisitStatements( tree );

        return declarations.Statements;
    }

    private BITEParser CreateBiteParser( string input, out BiteCompilerSyntaxErrorListener errorListener )
    {
        AntlrInputStream stream = new AntlrInputStream( input );
        BITELexer lexer = new BITELexer( stream );
        CommonTokenStream tokens = new CommonTokenStream( lexer );
        BITEParser biteParser = new BITEParser( tokens );
        errorListener = new BiteCompilerSyntaxErrorListener();
        biteParser.AddErrorListener( errorListener );
        biteParser.RemoveErrorListener( ConsoleErrorListener < IToken >.Instance );

        return biteParser;
    }

    #endregion

    #region Private Compilers

    private BiteProgram CompileExpressionInternal( ExpressionNode expression )
    {
        ModuleNode module = new ModuleNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ),
            Statements = new List < StatementNode > { new ExpressionStatementNode { Expression = expression } }
        };

        var symbolTable = SymbolTable.Default;

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( module );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        generator.Compile( module );

        biteProgram.Build();

        return biteProgram;
    }

    private BiteProgram CompileProgramInternal( ProgramNode programNode )
    {
        var symbolTable = SymbolTable.Default;

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildProgramSymbolTable( programNode );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        generator.Compile( programNode );

        biteProgram.Build();

        return biteProgram;
    }

    private BiteProgram CompileStatements( ModuleNode module, Dictionary < string, object > externalObjects,
        List < StatementNode > statements )
    {
        module.Statements = statements;

        var symbolTable = SymbolTable.Default; //.WithExternalObjects( externalObjects );

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( module );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        generator.Compile( module );

        biteProgram.Build();

        return biteProgram;
    }

    private BiteProgram CompileStatementsInternal( IReadOnlyCollection < StatementNode > statements,
        Dictionary < string, object > externalObjects, SymbolTable symbolTable = null )
    {

        ModuleNode module = new ModuleNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ),
            Statements = statements,
        };

        if ( symbolTable == null )
        {
            symbolTable = SymbolTable.Default.WithExternalObjects( externalObjects );
        }

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( module );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        // TODO: Don't recompile system module everytime?

        //ModuleNode systemModule = ParseModule( SystemModule );

        //symbolTableBuilder.BuildModuleSymbolTable( systemModule );

        //generator.Compile( systemModule );

        generator.Compile( module );

        biteProgram.Build();

        return biteProgram;
    }

    #endregion

}

}