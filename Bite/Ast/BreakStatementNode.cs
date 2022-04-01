namespace Bite.Ast
{

public class BreakStatementNode : StatementNode
{
    public override object Accept(IAstVisitor visitor)
    {
        return visitor.Visit(this);
    }
}

}
