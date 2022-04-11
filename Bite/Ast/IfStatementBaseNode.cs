using System.Collections.Generic;

namespace Bite.Ast
{

public class IfStatementBaseNode : StatementBaseNode
{
    public ExpressionBaseNode ExpressionBase;

    public StatementBaseNode ThenStatementBase;

    public StatementBaseNode ElseStatementBase;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}