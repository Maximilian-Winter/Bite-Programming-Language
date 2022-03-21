using System.Collections.Generic;

namespace MemoizeSharp
{

    public class ArgumentsNode : HeteroAstNode
    {
        public List<ExpressionNode> Expressions;
        public List<bool> IsReference;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
