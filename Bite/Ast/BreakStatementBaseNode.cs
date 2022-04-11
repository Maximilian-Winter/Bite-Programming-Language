namespace Bite.Ast
{

public class BreakStatementBaseNode : StatementBaseNode
{
    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
