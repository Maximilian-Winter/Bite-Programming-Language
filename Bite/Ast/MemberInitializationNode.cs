namespace Bite.Ast
{

public class MemberInitializationNode : AstBaseNode
{
    public Identifier Identifier;
    public ExpressionBaseNode Expression;

    public override object Accept( IAstVisitor visitor )
    {
        throw new System.NotImplementedException();
    }
}

}