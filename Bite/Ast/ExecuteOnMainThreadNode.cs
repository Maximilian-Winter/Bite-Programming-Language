namespace Bite.Ast
{

public class ExecuteOnMainThreadNode : AstBaseNode
{
    public BlockStatementBaseNode BlockToExecuteOnMainThread;

    public override object Accept( IAstVisitor visitor )
    {
        return null;
    }
}

}
