namespace Srsl.Ast
{

    public class VariableDeclarationNode : DeclarationNode
    {
        public ModifiersNode Modifiers;
        public Identifier VarId;
        public InitializerNode Initializer;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
