namespace Bite.Ast
{

public class TernaryOperationBaseNode : ExpressionBaseNode
{
    public ExpressionBaseNode LeftOperand;
    public ExpressionBaseNode MidOperand;
    public ExpressionBaseNode RightOperand;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
