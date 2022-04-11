namespace Bite.Ast
{

public class ReturnStatementBaseNode : StatementBaseNode
{
    public ExpressionStatementBaseNode ExpressionStatementBase;

    #region Public

    public ReturnStatementBaseNode()
    {
        ExpressionStatementBase = new ExpressionStatementBaseNode();
    }

    public ReturnStatementBaseNode( ExpressionStatementBaseNode expressionStatementBase )
    {
        ExpressionStatementBase = expressionStatementBase;
    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
