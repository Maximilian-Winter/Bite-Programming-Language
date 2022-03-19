namespace MemoizeSharp
{

public class BlockStatementNode : StatementNode
{
    public DeclarationsNode Declarations;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
