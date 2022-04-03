namespace Bite.Ast
{

public class BreakStatementNode : StatementNode
{
    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
