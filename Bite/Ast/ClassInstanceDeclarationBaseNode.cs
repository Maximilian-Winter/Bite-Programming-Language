using System.Collections.Generic;

namespace Bite.Ast
{

public class ClassInstanceDeclarationBaseNode : DeclarationBaseNode
{
    public ModifiersBaseNode ModifiersBase;
    public Identifier InstanceId;
    public ArgumentsBaseNode ArgumentsBase;
    public Identifier ClassName;
    public List < Identifier > ClassPath;
    public bool IsVariableRedeclaration;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
