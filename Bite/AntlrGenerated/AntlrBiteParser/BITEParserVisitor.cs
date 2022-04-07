//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:/Language Dev 3/Bite Programming Language/Bite/Grammar\BITEParser.g4 by ANTLR 4.9.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace AntlrBiteParser {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="BITEParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.2")]
[System.CLSCompliant(false)]
public interface IBITEParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProgram([NotNull] BITEParser.ProgramContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.module"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitModule([NotNull] BITEParser.ModuleContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.moduleDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitModuleDeclaration([NotNull] BITEParser.ModuleDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.importDirective"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitImportDirective([NotNull] BITEParser.ImportDirectiveContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.usingDirective"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUsingDirective([NotNull] BITEParser.UsingDirectiveContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclaration([NotNull] BITEParser.DeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.classDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassDeclaration([NotNull] BITEParser.ClassDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.structDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructDeclaration([NotNull] BITEParser.StructDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.functionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionDeclaration([NotNull] BITEParser.FunctionDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.classInstanceDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassInstanceDeclaration([NotNull] BITEParser.ClassInstanceDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.variableDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariableDeclaration([NotNull] BITEParser.VariableDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.statements"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatements([NotNull] BITEParser.StatementsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement([NotNull] BITEParser.StatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.exprStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExprStatement([NotNull] BITEParser.ExprStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.localVarDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLocalVarDeclaration([NotNull] BITEParser.LocalVarDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.localVarInitializer"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLocalVarInitializer([NotNull] BITEParser.LocalVarInitializerContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.forInitializer"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForInitializer([NotNull] BITEParser.ForInitializerContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.forIterator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForIterator([NotNull] BITEParser.ForIteratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.forStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForStatement([NotNull] BITEParser.ForStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.ifStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfStatement([NotNull] BITEParser.IfStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.returnStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStatement([NotNull] BITEParser.ReturnStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.breakStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBreakStatement([NotNull] BITEParser.BreakStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.usingStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUsingStatement([NotNull] BITEParser.UsingStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.whileStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileStatement([NotNull] BITEParser.WhileStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlock([NotNull] BITEParser.BlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression([NotNull] BITEParser.ExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.assignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignment([NotNull] BITEParser.AssignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.lambdaExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLambdaExpression([NotNull] BITEParser.LambdaExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.ternary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTernary([NotNull] BITEParser.TernaryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.logicOr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicOr([NotNull] BITEParser.LogicOrContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.logicAnd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicAnd([NotNull] BITEParser.LogicAndContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.bitwiseOr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwiseOr([NotNull] BITEParser.BitwiseOrContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.bitwiseXor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwiseXor([NotNull] BITEParser.BitwiseXorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.bitwiseAnd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBitwiseAnd([NotNull] BITEParser.BitwiseAndContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.equality"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEquality([NotNull] BITEParser.EqualityContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.relational"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRelational([NotNull] BITEParser.RelationalContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.shift"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitShift([NotNull] BITEParser.ShiftContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.additive"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAdditive([NotNull] BITEParser.AdditiveContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.multiplicative"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultiplicative([NotNull] BITEParser.MultiplicativeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.unary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnary([NotNull] BITEParser.UnaryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.call"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCall([NotNull] BITEParser.CallContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrimary([NotNull] BITEParser.PrimaryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.privateModifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrivateModifier([NotNull] BITEParser.PrivateModifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.publicModifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPublicModifier([NotNull] BITEParser.PublicModifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.abstractModifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAbstractModifier([NotNull] BITEParser.AbstractModifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.staticModifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStaticModifier([NotNull] BITEParser.StaticModifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.parameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameters([NotNull] BITEParser.ParametersContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.arguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArguments([NotNull] BITEParser.ArgumentsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.inheritance"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInheritance([NotNull] BITEParser.InheritanceContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.callArguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCallArguments([NotNull] BITEParser.CallArgumentsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.elementAccess"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElementAccess([NotNull] BITEParser.ElementAccessContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.elementIdentifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElementIdentifier([NotNull] BITEParser.ElementIdentifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.argumentExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArgumentExpression([NotNull] BITEParser.ArgumentExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="BITEParser.parametersIdentifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParametersIdentifier([NotNull] BITEParser.ParametersIdentifierContext context);
}
} // namespace AntlrBiteParser