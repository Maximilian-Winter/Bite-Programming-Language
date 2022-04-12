namespace Bite.Ast
{

public class ExecuteOnMainThreadNode : StatementBaseNode
{
    public BlockStatementBaseNode BlockToExecuteOnMainThread;

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }
}

}
