namespace Bite.Ast
{

public class ForStatementNode : StatementNode
{
    public ForInitializerNode Initializer { get; set; }
    public ExpressionNode Condition { get; set; }
    public ExpressionNode[] Iterators { get; set; }
    public StatementNode Statement { get; set; }


    // TODO: Remove
    public VariableDeclarationNode VariableDeclaration;
    public ExpressionNode Iterator;
    public ExpressionStatementNode ExpressionStatement;
    public BlockStatementNode Block;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}