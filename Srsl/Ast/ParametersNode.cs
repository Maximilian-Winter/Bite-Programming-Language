using System.Collections.Generic;

namespace Srsl.Ast
{

    public class ParametersNode : HeteroAstNode
    {
        public List<Identifier> Identifiers;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
