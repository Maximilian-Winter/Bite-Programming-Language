namespace Bite.Ast
{

public class SyncBlockNode : StatementBaseNode
{
    public BlockStatementBaseNode Block;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
