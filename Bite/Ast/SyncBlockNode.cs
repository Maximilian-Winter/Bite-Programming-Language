namespace Bite.Ast
{

public class SyncBlockNode : StatementBaseNode
{
    public BlockStatementBaseNode Block;

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }
}

}