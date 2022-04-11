namespace Bite.Ast
{

public class VariableDeclarationBaseNode : DeclarationBaseNode
{
    public ModifiersBaseNode ModifiersBase;
    public Identifier VarId;
    public InitializerBaseNode InitializerBase;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
