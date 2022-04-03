namespace Bite.Ast
{

public class UsingStatementNode : StatementNode
{
    public HeteroAstNode UsingNode;
    public BlockStatementNode UsingBlock;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
