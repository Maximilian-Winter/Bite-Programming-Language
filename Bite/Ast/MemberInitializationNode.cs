using System;

namespace Bite.Ast
{

public class MemberInitializationNode : AstBaseNode
{
    public Identifier Identifier;
    public ExpressionBaseNode Expression;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        throw new NotImplementedException();
    }

    #endregion
}

}
