using System.Collections.Generic;

namespace Bite.Ast
{

public class ClassInstanceDeclarationNode : DeclarationNode
{
    public ModifiersNode Modifiers;
    public Identifier InstanceId;
    public ArgumentsNode Arguments;
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
