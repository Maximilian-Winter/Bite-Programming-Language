using System.Collections.Generic;

namespace Bite.Ast
{

public abstract class AstVisitor<T>
{
    public AstVisitor()
    {
    }

    #region Public

    public abstract T Visit( ProgramBaseNode node );

    public abstract T Visit( ModuleBaseNode node );

    public abstract T Visit( ModifiersBaseNode node );

    public abstract T Visit( DeclarationBaseNode node );

    public abstract T Visit( UsingStatementBaseNode node );

    public abstract T Visit( DeclarationsBaseNode node );

    public abstract T Visit( ClassDeclarationBaseNode node );

    public abstract T Visit( FunctionDeclarationBaseNode node );

    public abstract T Visit( LocalVariableInitializerBaseNode node );

    public abstract T Visit( LocalVariableDeclarationBaseNode node );

    public abstract T Visit( VariableDeclarationBaseNode node );

    public abstract T Visit( ClassInstanceDeclarationBaseNode node );

    public abstract T Visit( CallBaseNode node );

    public abstract T Visit( ArgumentsBaseNode node );

    public abstract T Visit( ParametersBaseNode node );

    public abstract T Visit( AssignmentBaseNode node );

    public abstract T Visit( ExpressionBaseNode node );

    public abstract T Visit( BlockStatementBaseNode node );

    public abstract T Visit( StatementBaseNode node );

    public abstract T Visit( ExpressionStatementBaseNode node );

    public abstract T Visit( IfStatementBaseNode node );

    public abstract T Visit( ForStatementBaseNode node );

    public abstract T Visit( WhileStatementBaseNode node );

    public abstract T Visit( ReturnStatementBaseNode node );

    public abstract T Visit( BreakStatementBaseNode node );

    public abstract T Visit( InitializerBaseNode node );
    
    public abstract T Visit( ExecuteOnMainThreadNode node );

    public abstract T Visit( BinaryOperationBaseNode node );

    public abstract T Visit( TernaryOperationBaseNode node );

    public abstract T Visit( PrimaryBaseNode node );

    public abstract T Visit( StructDeclarationBaseNode node );

    public abstract T Visit( UnaryPostfixOperation node );

    public abstract T Visit( UnaryPrefixOperation node );

    public abstract T Visit( AstBaseNode node );

    #endregion
}

}