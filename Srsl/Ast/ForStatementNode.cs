namespace Srsl.Ast
{

    public class ForStatementNode : StatementNode
    {
        public VariableDeclarationNode VariableDeclaration;
        public ExpressionStatementNode ExpressionStatement;
        public ExpressionNode Expression1;
        public ExpressionNode Expression2;

        public BlockStatementNode Block;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
