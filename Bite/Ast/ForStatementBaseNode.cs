namespace Bite.Ast
{

public class ForStatementBaseNode : StatementBaseNode
{
    public ForInitializerBaseNode InitializerBase { get; set; }

    public ExpressionBaseNode Condition { get; set; }

    public ExpressionBaseNode[] Iterators { get; set; }

    public StatementBaseNode StatementBase { get; set; }

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
