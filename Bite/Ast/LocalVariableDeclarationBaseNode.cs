namespace Bite.Ast
{

public class LocalVariableDeclarationBaseNode : StatementBaseNode
{
    public ModifiersBaseNode ModifiersBase { get; set; }

    public Identifier VarId { get; set; }

    public ExpressionBaseNode ExpressionBase { get; set; }

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
