namespace Bite.Ast
{

public class ExpressionStatementNode : StatementNode
{
    public ExpressionNode Expression;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
