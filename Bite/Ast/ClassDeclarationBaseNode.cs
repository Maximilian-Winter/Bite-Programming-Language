using System.Collections.Generic;

namespace Bite.Ast
{

public class ClassDeclarationBaseNode : DeclarationBaseNode
{
    public Identifier ClassId;
    public List < Identifier > Inheritance;
    public ModifiersBaseNode ModifiersBase;
    public BlockStatementBaseNode BlockStatementBase;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
