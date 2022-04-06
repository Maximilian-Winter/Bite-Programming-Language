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

public class BITECompiler
{
    private ProgramNode ParseModules( string mainModule, IEnumerable < string > modules )
    {
        ProgramNode program = new ProgramNode( mainModule );

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
        BITEParser.ModuleContext tree = biteParser.program().module( 0 );

        return ( ModuleNode ) gen.VisitModule( tree );
    }

    private ExpressionNode ParseExpression( string expression )
    {
        HeteroAstGenerator gen = new HeteroAstGenerator();
        AntlrInputStream input = new AntlrInputStream( expression );
        BITELexer lexer = new BITELexer( input );
        CommonTokenStream tokens = new CommonTokenStream( lexer );
        BITEParser biteParser = new BITEParser( tokens );
        BITEParser.ExpressionContext tree = biteParser.expression();

        return ( ExpressionNode ) gen.VisitExpression( tree );
    }

    private IReadOnlyCollection < StatementNode > ParseStatements( string statements )
    {
        HeteroAstGenerator gen = new HeteroAstGenerator();
        AntlrInputStream input = new AntlrInputStream( statements );
        BITELexer lexer = new BITELexer( input );
        CommonTokenStream tokens = new CommonTokenStream( lexer );
        BITEParser biteParser = new BITEParser( tokens );
        BITEParser.StatementsContext tree = biteParser.statements();
        DeclarationsNode declarations = ( DeclarationsNode ) gen.VisitStatements( tree );

        return declarations.Statements;
    }

    public BiteProgram Compile( string mainModule, IEnumerable < string > modules )
    {
        ProgramNode program = new ProgramNode( mainModule );

        foreach ( string biteModule in modules )
        {
            ModuleNode module = ParseModule( biteModule );
            program.AddModule( module );
        }

        CodeGenerator generator = new CodeGenerator();

        return generator.CompileProgram( program );
    }

    public BiteProgram CompileExpression( string expression )
    {
        var expressionNode = ParseExpression( expression );

        CodeGenerator generator = new CodeGenerator();

        return generator.CompileExpression( expressionNode );
    }

    public BiteProgram CompileStatements( string statements, BiteProgram program = null )
    {
        var statementNodes = ParseStatements( statements );

        CodeGenerator generator = new CodeGenerator();

        return generator.CompileStatements( statementNodes, program );
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

        var program = ParseModules( modules.Single( m => m.MainModule ).Name, moduleStrings );

        CodeGenerator generator = new CodeGenerator();

        return generator.CompileProgram( program );
    }
}

}