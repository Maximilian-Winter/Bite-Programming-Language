namespace MemoizeSharp
{

    public interface IAstVisitor
    {
        object Visit(HeteroAstNode heteroAstNode);
    }

}
