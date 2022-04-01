namespace Srsl.Ast
{

    public class UnaryPrefixOperation : ExpressionNode
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

        public ExpressionNode Primary;
        public UnaryPrefixOperatorType Operator;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
