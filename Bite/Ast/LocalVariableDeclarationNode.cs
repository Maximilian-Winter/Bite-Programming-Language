namespace Bite.Ast
{

public class LocalVariableDeclarationNode : StatementNode
{
    public ModifiersNode Modifiers { get; set; }

    public Identifier VarId { get; set; }

    public ExpressionNode Expression { get; set; }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }
}

}