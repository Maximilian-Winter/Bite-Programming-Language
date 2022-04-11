namespace Bite.Ast
{

public class ExpressionStatementBaseNode : StatementBaseNode
{
    public ExpressionBaseNode ExpressionBase;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
