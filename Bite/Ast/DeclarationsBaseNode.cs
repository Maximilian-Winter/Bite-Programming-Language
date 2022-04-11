using System.Collections.Generic;

namespace Bite.Ast
{

public class DeclarationsBaseNode : DeclarationBaseNode
{
    public List < ClassDeclarationBaseNode > Classes;
    public List < StructDeclarationBaseNode > Structs;
    public List < ClassInstanceDeclarationBaseNode > ClassInstances;
    public List < VariableDeclarationBaseNode > Variables;
    public List < FunctionDeclarationBaseNode > Functions;
    public List < StatementBaseNode > Statements;

    #region Public

    public DeclarationsBaseNode()
    {
        Classes = new List < ClassDeclarationBaseNode >();
        Structs = new List < StructDeclarationBaseNode >();
        ClassInstances = new List < ClassInstanceDeclarationBaseNode >();
        Variables = new List < VariableDeclarationBaseNode >();
        Functions = new List < FunctionDeclarationBaseNode >();
        Statements = new List < StatementBaseNode >();
    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
