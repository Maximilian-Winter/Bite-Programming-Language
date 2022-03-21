namespace MemoizeSharp
{

    public class UnaryPostfixOperation : ExpressionNode
    {
        public enum UnaryPostfixOperatorType
        {
            PlusPlus,
            MinusMinus
        }

        public ExpressionNode Primary;
        public UnaryPostfixOperatorType Operator;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
