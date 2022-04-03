namespace Bite.Ast
{

public interface IAstVisitor
{
    object Visit( HeteroAstNode heteroAstNode );
}

}
