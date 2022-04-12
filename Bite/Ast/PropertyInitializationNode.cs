namespace Bite.Ast
{

public class PropertyInitializationNode : AstBaseNode
{
    public Identifier Identifier;
    public ExpressionBaseNode Expression;

    public override object Accept( IAstVisitor visitor )
    {
        throw new System.NotImplementedException();
    }
}

}