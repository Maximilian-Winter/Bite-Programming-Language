using System.Collections.Generic;

namespace Bite.Ast
{

public class IfStatementNode : StatementNode
{
    public ExpressionNode Expression;

    public StatementNode ThenStatement;

    public StatementNode ElseStatement;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}