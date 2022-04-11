namespace Bite.Ast
{

public class BinaryOperationBaseNode : ExpressionBaseNode
{
    public enum BinaryOperatorType
    {
        Plus,
        Minus,
        Modulo,
        Mult,
        Div,
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
        And,
        Or,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        ShiftLeft,
        ShiftRight
    }

    public ExpressionBaseNode LeftOperand;
    public BinaryOperatorType? Operator;
    public ExpressionBaseNode RightOperand;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
