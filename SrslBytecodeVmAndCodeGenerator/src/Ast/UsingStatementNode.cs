namespace MemoizeSharp
{

    public class UsingStatementNode : StatementNode
    {
        #region Public

        public HeteroAstNode UsingNode;
        public BlockStatementNode UsingBlock;

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
