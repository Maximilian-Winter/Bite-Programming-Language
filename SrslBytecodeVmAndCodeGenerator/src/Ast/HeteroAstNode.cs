using Srsl_Parser.SymbolTable;

namespace MemoizeSharp
{

public abstract class HeteroAstNode
{
    public Scope AstScopeNode;
    public DebugInfo DebugInfoAstNode = new DebugInfo();

    #region Public

    public abstract object Accept( IAstVisitor visitor );

    #endregion
}

}
