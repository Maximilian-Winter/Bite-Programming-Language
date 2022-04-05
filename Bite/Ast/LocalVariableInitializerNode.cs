using System.Collections.Generic;

namespace Bite.Ast
{

public class LocalVariableInitializerNode : StatementNode
{
    public IEnumerable < LocalVariableDeclarationNode > VariableDeclarations { get; set; }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }
}

}