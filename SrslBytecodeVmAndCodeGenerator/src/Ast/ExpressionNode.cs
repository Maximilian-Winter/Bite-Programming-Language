namespace Srsl.Ast
{

    public class ExpressionNode : HeteroAstNode
    {
        public AssignmentNode Assignment;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
