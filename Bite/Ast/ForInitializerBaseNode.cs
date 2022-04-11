namespace Bite.Ast
{

public class ForInitializerBaseNode : AstBaseNode
{
    public LocalVariableInitializerBaseNode LocalVariableInitializerBase { get; set; }

    public ExpressionBaseNode[] Expressions { get; set; }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }
}

}