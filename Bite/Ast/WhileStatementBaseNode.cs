namespace Bite.Ast
{

public class WhileStatementBaseNode : StatementBaseNode
{
    public ExpressionBaseNode ExpressionBase;
    public BlockStatementBaseNode WhileBlock;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
