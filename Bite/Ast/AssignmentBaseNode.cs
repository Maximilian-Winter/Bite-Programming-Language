namespace Bite.Ast
{

public class AssignmentBaseNode : ExpressionBaseNode
{
    public CallBaseNode CallBase;
    public AssignmentOperatorTypes OperatorType;
    public AssignmentTypes Type;
    public BinaryOperationBaseNode Binary;
    public TernaryOperationBaseNode Ternary;
    public UnaryPostfixOperation UnaryPostfix;
    public UnaryPrefixOperation UnaryPrefix;
    public PrimaryBaseNode PrimaryBaseNode;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
