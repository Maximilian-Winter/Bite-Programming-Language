namespace Bite.Ast
{

public class BlockStatementBaseNode : StatementBaseNode
{
    public DeclarationsBaseNode DeclarationsBase;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
