namespace Srsl.Ast
{

    public class TernaryOperationNode : ExpressionNode
    {
        public ExpressionNode LeftOperand;
        public ExpressionNode MidOperand;
        public ExpressionNode RightOperand;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
