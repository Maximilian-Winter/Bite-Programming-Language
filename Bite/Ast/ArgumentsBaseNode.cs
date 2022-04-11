using System.Collections.Generic;

namespace Bite.Ast
{

public class ArgumentsBaseNode : AstBaseNode
{
    public List < ExpressionBaseNode > Expressions;
    public List < bool > IsReference;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
