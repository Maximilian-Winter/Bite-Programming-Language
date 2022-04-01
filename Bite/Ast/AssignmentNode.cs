namespace Bite.Ast
{

    public class AssignmentNode : ExpressionNode
    {


        public CallNode Call;
        public AssignmentOperatorTypes OperatorType;
        public AssignmentTypes Type;
        public BinaryOperationNode Binary;
        public TernaryOperationNode Ternary;
        public UnaryPostfixOperation UnaryPostfix;
        public UnaryPrefixOperation UnaryPrefix;
        public PrimaryNode PrimaryNode;

        #region Public

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
