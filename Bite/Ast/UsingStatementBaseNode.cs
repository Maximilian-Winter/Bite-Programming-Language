namespace Bite.Ast
{

public class UsingStatementBaseNode : StatementBaseNode
{
    public AstBaseNode UsingBaseNode;
    public BlockStatementBaseNode UsingBlock;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
