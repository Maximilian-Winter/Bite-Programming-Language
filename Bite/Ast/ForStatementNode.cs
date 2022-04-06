namespace Bite.Ast
{

public class ForStatementNode : StatementNode
{
    public ForInitializerNode Initializer { get; set; }
    public ExpressionNode Condition { get; set; }
    public ExpressionNode[] Iterators { get; set; }
    public StatementNode Statement { get; set; }

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}