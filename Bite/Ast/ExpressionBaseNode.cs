namespace Bite.Ast
{

public class ExpressionBaseNode : AstBaseNode
{
    public AssignmentBaseNode AssignmentBase;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
