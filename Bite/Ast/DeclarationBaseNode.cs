namespace Bite.Ast
{

public abstract class DeclarationBaseNode : StatementBaseNode
{
    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
