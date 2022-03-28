using System.Collections.Generic;

namespace Srsl.Ast
{

    public class ClassDeclarationNode : DeclarationNode
    {
        public Identifier ClassId;
        public List<Identifier> Inheritance;
        public ModifiersNode Modifiers;
        public BlockStatementNode BlockStatement;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
