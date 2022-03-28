namespace Srsl.Ast
{

    public class StructDeclarationNode : DeclarationNode
    {
        public Identifier StructId;
        public ModifiersNode Modifiers;
        public BlockStatementNode Block;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
