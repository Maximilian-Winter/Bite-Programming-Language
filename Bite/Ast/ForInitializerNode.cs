namespace Bite.Ast
{

public class ForInitializerNode : HeteroAstNode
{
    public LocalVariableInitializerNode LocalVariableInitializer { get; set; }

    public ExpressionNode[] Expressions { get; set; }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }
}

}