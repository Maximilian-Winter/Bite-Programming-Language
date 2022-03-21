namespace MemoizeSharp
{

    public class BinaryOperationNode : ExpressionNode
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

        public ExpressionNode LeftOperand;
        public BinaryOperatorType? Operator;
        public ExpressionNode RightOperand;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
