namespace Bite.Ast
{

public interface IAstVisitor
{
    object Visit( AstBaseNode astBaseNode );
}

}
