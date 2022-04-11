namespace Bite.Ast
{

public class InitializerBaseNode : AstBaseNode
{
    public ExpressionStatementBaseNode Expression;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
