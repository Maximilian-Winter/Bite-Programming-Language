using System.Collections.Generic;

namespace Bite.Ast
{

public class IfStatementNode : StatementNode
{
    public ExpressionNode Expression;
    public BlockStatementNode ThenBlock;
    public List < IfStatementEntry > IfStatementEntries;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
