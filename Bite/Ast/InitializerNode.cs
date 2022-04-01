namespace Bite.Ast
{

    public class InitializerNode : HeteroAstNode
    {
        public ExpressionStatementNode Expression;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
