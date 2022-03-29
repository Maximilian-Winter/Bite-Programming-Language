namespace Srsl.Ast
{

    public interface IAstVisitor
    {
        object Visit(HeteroAstNode heteroAstNode);
    }

}
