using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using AntlrBiteParser;
using Bite.Ast;
using Bite.Modules;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Bite.Symbols;
using MemoizeSharp;

namespace Bite.Compiler
{

public class BiteCompiler
{
    #region Public Compilers

    public BiteProgram Compile( IEnumerable < string > modules )
    {
        ProgramBaseNode programBase = ParseModules( modules );

        return CompileProgramInternal( programBase );
    }

    public BiteProgram CompileExpression( string expression )
    {
        ExpressionBaseNode expressionBaseNode = ParseExpression( expression );

        return CompileExpressionInternal( expressionBaseNode );
    }

    public BiteProgram CompileStatements( string statements, SymbolTable symbolTable = null )
    {
        IReadOnlyCollection < StatementBaseNode > statementNodes = ParseStatements( statements );

        return CompileStatementsInternal( statementNodes, symbolTable );
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

        ProgramBaseNode programBase = ParseModules( moduleStrings );

        return CompileProgramInternal( programBase );
    }

    #endregion

    #region Parsers

    private static string m_SystemModule;

    private string GetSystemModule()
    {
        // Memoize system module so we don't load it from the assembly resource every time we compile
        if ( m_SystemModule == null )
        {
            m_SystemModule = ModuleLoader.LoadModule( "System" );
        }
        return m_SystemModule;
    }

    private ProgramBaseNode ParseModules( IEnumerable < string > modules )
    {
        ProgramBaseNode programBase = new ProgramBaseNode();


        ModuleBaseNode systemModuleBase = ParseModule( GetSystemModule() );
        programBase.AddModule( systemModuleBase );

        foreach (string biteModule in modules)
        {
            ModuleBaseNode moduleBase = ParseModule( biteModule );
            programBase.AddModule( moduleBase );
        }

        return programBase;
    }

    private ModuleBaseNode ParseModule( string module )
    {
        BiteAstGenerator gen = new BiteAstGenerator();

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

        return ( ModuleBaseNode ) gen.VisitModule( tree );
    }

    private ExpressionBaseNode ParseExpression( string expression )
    {
        BITEParser biteParser = CreateBiteParser( expression, out var errorListener );

        BITEParser.ExpressionContext tree = biteParser.expression();

        if ( errorListener.Errors.Count > 0 )
        {
            throw new BiteCompilerException(
                $"Error occured while parsing expression.\r\nError Count: {errorListener.Errors.Count}",
                errorListener.Errors );
        }

        BiteAstGenerator gen = new BiteAstGenerator();

        return ( ExpressionBaseNode ) gen.VisitExpression( tree );
    }

    private IReadOnlyCollection < StatementBaseNode > ParseStatements( string statements )
    {
        BITEParser biteParser = CreateBiteParser( statements, out var errorListener );

        BITEParser.StatementsContext tree = biteParser.statements();

        if ( errorListener.Errors.Count > 0 )
        {
            throw new BiteCompilerException(
                $"Error occured while parsing statement.\r\nError Count: {errorListener.Errors.Count}",
                errorListener.Errors );
        }

        BiteAstGenerator gen = new BiteAstGenerator();

        DeclarationsBaseNode declarationsBase = ( DeclarationsBaseNode ) gen.VisitStatements( tree );

        return declarationsBase.Statements;
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

    private BiteProgram CompileExpressionInternal( ExpressionBaseNode expressionBase )
    {
        ModuleBaseNode moduleBase = new ModuleBaseNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ),
            Statements = new List < StatementBaseNode > { new ExpressionStatementBaseNode { ExpressionBase = expressionBase } }
        };

        var symbolTable = SymbolTable.Default;

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( moduleBase );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        generator.Compile( moduleBase );

        biteProgram.Build();

        return biteProgram;
    }

    private BiteProgram CompileProgramInternal( ProgramBaseNode programBaseNode )
    {
        var symbolTable = SymbolTable.Default;

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildProgramSymbolTable( programBaseNode );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        generator.Compile( programBaseNode );

        biteProgram.Build();

        return biteProgram;
    }

    private BiteProgram CompileStatements( ModuleBaseNode moduleBase, Dictionary < string, object > externalObjects,
        List < StatementBaseNode > statements )
    {
        moduleBase.Statements = statements;

        var symbolTable = SymbolTable.Default; //.WithExternalObjects( externalObjects );

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( moduleBase );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        generator.Compile( moduleBase );

        biteProgram.Build();

        return biteProgram;
    }

    private BiteProgram CompileStatementsInternal( IReadOnlyCollection < StatementBaseNode > statements, SymbolTable symbolTable = null )
    {

        ModuleBaseNode moduleBase = new ModuleBaseNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ),
            Statements = statements,
        };

        if ( symbolTable == null )
        {
            symbolTable = SymbolTable.Default; //.WithExternalObjects( externalObjects );
        }

        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( moduleBase );

        var biteProgram = new BiteProgram( symbolTable );

        CodeGenerator generator = new CodeGenerator( biteProgram );

        // TODO: Don't recompile system module everytime?

        //ModuleNode systemModule = ParseModule( SystemModule );

        //symbolTableBuilder.BuildModuleSymbolTable( systemModule );

        //generator.Compile( systemModule );

        generator.Compile( moduleBase );

        biteProgram.Build();

        return biteProgram;
    }

    #endregion

}

}