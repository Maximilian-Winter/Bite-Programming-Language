namespace Bite.Ast
{

public class StructDeclarationBaseNode : DeclarationBaseNode
{
    public Identifier StructId;
    public ModifiersBaseNode ModifiersBase;
    public BlockStatementBaseNode Block;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
