namespace Bite.Ast
{

public class UnaryPrefixOperation : ExpressionBaseNode
{
    public enum UnaryPrefixOperatorType
    {
        Plus,
        Compliment,
        PlusPlus,
        MinusMinus,
        LogicalNot,
        Negate
    }

    public ExpressionBaseNode Primary;
    public UnaryPrefixOperatorType Operator;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
