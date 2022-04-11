using System.Collections.Generic;

namespace Bite.Ast
{

public class CallBaseNode : ExpressionBaseNode
{
    public CallTypes CallType;
    public PrimaryBaseNode PrimaryBase;
    public ArgumentsBaseNode ArgumentsBase;
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
