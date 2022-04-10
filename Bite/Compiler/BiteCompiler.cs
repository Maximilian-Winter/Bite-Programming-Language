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
    private ProgramNode ParseModules( IEnumerable < string > modules )
    {
        ProgramNode program = new ProgramNode();

        foreach ( string biteModule in modules )
        {
            ModuleNode module = ParseModule( biteModule );
            program.AddModule( module );
        }

        return program;
    }

    private ModuleNode ParseModule( string module )
    {
        HeteroAstGenerator gen = new HeteroAstGenerator();
        AntlrInputStream input = new AntlrInputStream( module );
        BITELexer lexer = new BITELexer( input );
        CommonTokenStream tokens = new CommonTokenStream( lexer );
        BITEParser biteParser = new BITEParser( tokens );
        BiteCompilerSyntaxErrorListener biteCompilerSyntaxErrorListener = new BiteCompilerSyntaxErrorListener();
        biteParser.AddErrorListener( biteCompilerSyntaxErrorListener );
        biteParser.RemoveErrorListener( ConsoleErrorListener < IToken >.Instance );
        BITEParser.ModuleContext tree = biteParser.program().module( 0 );

        if ( biteCompilerSyntaxErrorListener.Errors.Count > 0 )
        {
            module = module.Trim();
            int start = module.IndexOf( "module" ) + "module ".Length;
            var moduleName = module.Substring( start, module.IndexOf( ';' ) - start );

            throw new BiteCompilerException(
                $"Error occured while parsing module '{moduleName}' .\r\nError Count: {biteCompilerSyntaxErrorListener.Errors.Count}",
                biteCompilerSyntaxErrorListener.Errors );
        }

        return ( ModuleNode ) gen.VisitModule( tree );
    }

    private ExpressionNode ParseExpression( string expression )
    {
        HeteroAstGenerator gen = new HeteroAstGenerator();
        AntlrInputStream input = new AntlrInputStream( expression );
        BITELexer lexer = new BITELexer( input );
        CommonTokenStream tokens = new CommonTokenStream( lexer );
        BITEParser biteParser = new BITEParser( tokens );
        BiteCompilerSyntaxErrorListener biteCompilerSyntaxErrorListener = new BiteCompilerSyntaxErrorListener();
        biteParser.AddErrorListener( biteCompilerSyntaxErrorListener );
        biteParser.RemoveErrorListener( ConsoleErrorListener < IToken >.Instance );
        BITEParser.ExpressionContext tree = biteParser.expression();

        if ( biteCompilerSyntaxErrorListener.Errors.Count > 0 )
        {
            throw new BiteCompilerException(
                $"Error occured while parsing expression.\r\nError Count: {biteCompilerSyntaxErrorListener.Errors.Count}",
                biteCompilerSyntaxErrorListener.Errors );
        }

        return ( ExpressionNode ) gen.VisitExpression( tree );
    }

    private IReadOnlyCollection < StatementNode > ParseStatements( string statements )
    {
        HeteroAstGenerator gen = new HeteroAstGenerator();
        AntlrInputStream input = new AntlrInputStream( statements );
        BITELexer lexer = new BITELexer( input );
        CommonTokenStream tokens = new CommonTokenStream( lexer );
        BITEParser biteParser = new BITEParser( tokens );
        BiteCompilerSyntaxErrorListener biteCompilerSyntaxErrorListener = new BiteCompilerSyntaxErrorListener();
        biteParser.AddErrorListener( biteCompilerSyntaxErrorListener );
        biteParser.RemoveErrorListener( ConsoleErrorListener < IToken >.Instance );
        BITEParser.StatementsContext tree = biteParser.statements();

        if ( biteCompilerSyntaxErrorListener.Errors.Count > 0 )
        {
            throw new BiteCompilerException(
                $"Error occured while parsing statement.\r\nError Count: {biteCompilerSyntaxErrorListener.Errors.Count}",
                biteCompilerSyntaxErrorListener.Errors );
        }

        DeclarationsNode declarations = ( DeclarationsNode ) gen.VisitStatements( tree );

        return declarations.Statements;
    }

    public BiteProgram Compile( IEnumerable < string > modules )
    {
        ProgramNode program = new ProgramNode();

        foreach ( string biteModule in modules )
        {
            ModuleNode module = ParseModule( biteModule );
            program.AddModule( module );
        }

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

    private BiteProgram CompileExpressionInternal( ExpressionNode expression )
    {
        ModuleNode module = new ModuleNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ),
            Statements = new List < StatementNode > { new ExpressionStatementNode { Expression = expression } }
        };

        var symbolTable = new SymbolTable();

        symbolTable.Initialize();

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( module );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        module.Accept( generator );

        biteProgram.Build();

        return biteProgram;
    }

    private BiteProgram CompileProgramInternal( ProgramNode programNode )
    {
        var symbolTable = new SymbolTable();

        symbolTable.Initialize();

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

        var symbolTable = new SymbolTable();

        symbolTable.Initialize( externalObjects );

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
            symbolTable = new SymbolTable();
            symbolTable.Initialize( externalObjects );
        }

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( module );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        generator.Compile( module );

        biteProgram.Build();

        return biteProgram;
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

}

}