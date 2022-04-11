using System.Collections.Generic;

namespace Bite.Ast
{

public class ParametersBaseNode : AstBaseNode
{
    public List < Identifier > Identifiers;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
