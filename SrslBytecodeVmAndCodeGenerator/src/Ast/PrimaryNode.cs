namespace MemoizeSharp
{

public class PrimaryNode : ExpressionNode
{
    public enum PrimaryTypes
    {
        Default,
        Identifier,
        BooleanLiteral,
        IntegerLiteral,
        FloatLiteral,
        StringLiteral,
        Expression,
        NullReference,
        ThisReference
    }

    public PrimaryTypes PrimaryType;
    public Identifier PrimaryId;
    public bool? BooleanLiteral = null;
    public int? IntegerLiteral = null;
    public double? FloatLiteral = null;
    public string StringLiteral;
    public HeteroAstNode Expression;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
