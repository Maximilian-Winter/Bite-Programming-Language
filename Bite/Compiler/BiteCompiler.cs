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

    /// <summary>
    /// Compiles a set of modules
    /// </summary>
    /// <param name="modules"></param>
    /// <returns></returns>
    public BiteProgram Compile( IEnumerable < string > modules )
    {
        ProgramBaseNode programBase = ParseModules( modules );

        SymbolTable symbolTable = new SymbolTable();

        return CompileProgram( symbolTable, programBase );
    }

    /// <summary>
    /// Compiles a single expression. Used for unit tests.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public BiteProgram CompileExpression( string expression )
    {
        ExpressionBaseNode expressionBaseNode = ParseExpression( expression );

        ModuleBaseNode moduleBase = new ModuleBaseNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ),
            Statements = new List < StatementBaseNode >
                { new ExpressionStatementBaseNode { ExpressionBase = expressionBaseNode } }
        };

        SymbolTable symbolTable = new SymbolTable();

        return CompileModule( symbolTable, moduleBase );
    }

    /// <summary>
    /// Compiles a set of statements, with the option to reuse an existing <see cref="SymbolTable"/>.
    /// Primarily used for REPL, where the previous program context should be retained between compilations of new statements
    /// </summary>
    /// <param name="statements"></param>
    /// <param name="symbolTable"></param>
    /// <returns></returns>
    public BiteProgram CompileStatements( string statements, SymbolTable symbolTable = null )
    {
        IReadOnlyCollection < StatementBaseNode > statementNodes = ParseStatements( statements );

        ModuleBaseNode moduleBase = new ModuleBaseNode
        {
            ModuleIdent = new ModuleIdentifier( "MainModule" ),
            Statements = statementNodes,
        };

        if ( symbolTable == null )
        {
            symbolTable = new SymbolTable();
        }

        return CompileModule( symbolTable, moduleBase );
    }

    /// <summary>
    /// Compiles a set of <see cref="Module"/> objects. You only need to use <see cref="Module"/> objects if you want to keep a reference
    /// to module names and imports in your own code without needing to parse a module string
    /// </summary>
    /// <param name="modules"></param>
    /// <returns></returns>
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

        SymbolTable symbolTable = new SymbolTable();

        return CompileProgram( symbolTable, programBase );
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

        foreach ( string biteModule in modules )
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

    private BiteProgram CompileModule( SymbolTable symbolTable, ModuleBaseNode moduleBase )
    {
        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildModuleSymbolTable( moduleBase );

        var context = new BiteCompilationContext( symbolTable );

        CodeGenerator generator = new CodeGenerator( context );

        generator.Compile( moduleBase );

        context.Build();

        return BiteProgram.Create( symbolTable, context.CompiledMainChunk, context.CompiledChunks );
    }

    private BiteProgram CompileProgram( SymbolTable symbolTable, ProgramBaseNode programBase )
    {
        var symbolTableBuilder = new SymbolTableBuilder( symbolTable );

        symbolTableBuilder.BuildProgramSymbolTable( programBase );

        var context = new BiteCompilationContext( symbolTable );

        CodeGenerator generator = new CodeGenerator( context );

        generator.Compile( programBase );

        context.Build();

        return BiteProgram.Create( symbolTable, context.CompiledMainChunk, context.CompiledChunks );
    }

    #endregion

}

}