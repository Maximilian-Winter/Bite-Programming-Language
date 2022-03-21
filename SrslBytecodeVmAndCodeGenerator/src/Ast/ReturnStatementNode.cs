namespace MemoizeSharp
{

    public class ReturnStatementNode : StatementNode
    {
        public ExpressionStatementNode ExpressionStatement;

        #region Public

        public ReturnStatementNode()
        {
            ExpressionStatement = new ExpressionStatementNode();
        }

        public ReturnStatementNode(ExpressionStatementNode expressionStatement)
        {
            ExpressionStatement = expressionStatement;
        }

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
