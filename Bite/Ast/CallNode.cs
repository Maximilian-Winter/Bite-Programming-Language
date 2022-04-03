using System.Collections.Generic;

namespace Bite.Ast
{

public class CallNode : ExpressionNode
{
    public CallTypes CallType;
    public PrimaryNode Primary;
    public ArgumentsNode Arguments;
    public List < CallElementEntry > ElementAccess;
    public List < CallEntry > CallEntries;
    public bool IsFunctionCall;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
