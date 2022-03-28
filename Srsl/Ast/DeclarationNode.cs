namespace Srsl.Ast
{

    public abstract class DeclarationNode : StatementNode
    {
        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
