namespace Srsl.Ast
{

    public class WhileStatementNode : StatementNode
    {
        public ExpressionNode Expression;
        public BlockStatementNode WhileBlock;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
