using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using AntlrBiteParser;
using Bite.Ast;
using Bite.Runtime.CodeGen;
using MemoizeSharp;

namespace Bite.Runtime
{

public class BiteCompiler
{
    private ProgramNode ParseModules( IEnumerable < string > modules )
    {
        ProgramNode program = new ProgramNode();

        foreach ( string biteModule in modules )
        {
            ModuleNode module = null;
            try
            {
                module = ParseModule( biteModule );
            }
            catch ( BiteCompilerException e )
            {
                Console.WriteLine(e.BiteCompilerExceptionMessage);
                foreach ( BiteCompilerSyntaxError syntaxError in e.SyntaxErrors )
                {
                    Console.WriteLine($"Compiler Error: {syntaxError.Message} at line: {syntaxError.Line} at position: {syntaxError.CharPositionInLine}.");
                }

                return null;
            }
            
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
            throw new BiteCompilerException( $"Error found while compiling!\r\nError Count: {biteCompilerSyntaxErrorListener.Errors.Count}", biteCompilerSyntaxErrorListener.Errors );
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
            throw new BiteCompilerException( $"Error found while compiling!\r\nError Count: {biteCompilerSyntaxErrorListener.Errors.Count}", biteCompilerSyntaxErrorListener.Errors );
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
            throw new BiteCompilerException( $"Error found while compiling!\r\nError Count: {biteCompilerSyntaxErrorListener.Errors.Count}", biteCompilerSyntaxErrorListener.Errors );
        }
        DeclarationsNode declarations = ( DeclarationsNode ) gen.VisitStatements( tree );

        return declarations.Statements;
    }

    public BiteProgram Compile( IEnumerable < string > modules )
    {
        ProgramNode program = new ProgramNode();

        foreach ( string biteModule in modules )
        {
            ModuleNode module = null;
            try
            {
                module = ParseModule( biteModule );
            }
            catch ( BiteCompilerException e )
            {
                Console.WriteLine(e.BiteCompilerExceptionMessage);
                foreach ( BiteCompilerSyntaxError syntaxError in e.SyntaxErrors )
                {
                    Console.WriteLine($"Compiler Error: {syntaxError.Message} at line: {syntaxError.Line} at position: {syntaxError.CharPositionInLine}.");
                }

                return null;
            }
            
            program.AddModule( module );
        }

        CodeGenerator generator = new CodeGenerator();

        return generator.CompileProgram( program );
    }

    public BiteProgram CompileExpression( string expression )
    {
        ExpressionNode expressionNode = null;
        try
        {
            expressionNode = ParseExpression( expression );
        }
        catch ( BiteCompilerException e )
        {
            Console.WriteLine(e.BiteCompilerExceptionMessage);
            foreach ( BiteCompilerSyntaxError syntaxError in e.SyntaxErrors )
            {
                Console.WriteLine($"Compiler Error: {syntaxError.Message} at line: {syntaxError.Line} at position: {syntaxError.CharPositionInLine}.");
            }

            return null;
        }
        CodeGenerator generator = new CodeGenerator();

        return generator.CompileExpression( expressionNode );
    }

    public BiteProgram CompileStatements( string statements, BiteProgram previousBiteProgram = null )
    {
        IReadOnlyCollection < StatementNode > statementNodes = null;
        
        try
        {
            statementNodes = ParseStatements( statements );
        }
        catch ( BiteCompilerException e )
        {
            Console.WriteLine(e.BiteCompilerExceptionMessage);
            foreach ( BiteCompilerSyntaxError syntaxError in e.SyntaxErrors )
            {
                Console.WriteLine($"Compiler Error: {syntaxError.Message} at line: {syntaxError.Line} at position: {syntaxError.CharPositionInLine}.");
            }

            return null;
        }
        
        CodeGenerator generator = new CodeGenerator();

        return generator.CompileStatements( statementNodes, previousBiteProgram );
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

        ProgramNode program = null;
        
        try
        {
            program = ParseModules( moduleStrings );
        }
        catch ( BiteCompilerException e )
        {
            Console.WriteLine(e.BiteCompilerExceptionMessage);
            foreach ( BiteCompilerSyntaxError syntaxError in e.SyntaxErrors )
            {
                Console.WriteLine($"Compiler Error: {syntaxError.Message} at line: {syntaxError.Line} at position: {syntaxError.CharPositionInLine}.");
            }

            return null;
        }
        
        CodeGenerator generator = new CodeGenerator();

        return generator.CompileProgram( program );
    }
}

}