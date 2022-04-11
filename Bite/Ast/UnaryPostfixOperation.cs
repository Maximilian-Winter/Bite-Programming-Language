namespace Bite.Ast
{

public class UnaryPostfixOperation : ExpressionBaseNode
{
    public enum UnaryPostfixOperatorType
    {
        PlusPlus,
        MinusMinus
    }

    public ExpressionBaseNode Primary;
    public UnaryPostfixOperatorType Operator;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
