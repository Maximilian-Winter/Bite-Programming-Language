using System.Collections.Generic;

namespace Bite.Ast
{

public class LocalVariableInitializerBaseNode : StatementBaseNode
{
    public IEnumerable < LocalVariableDeclarationBaseNode > VariableDeclarations { get; set; }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }
}

}