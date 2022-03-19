using System.Collections.Generic;

namespace MemoizeSharp
{

public class ProgramNode : HeteroAstNode
{
    public string MainModule;
    public Dictionary <string, ModuleNode > ModuleNodes;

    #region Public

    public ProgramNode()
    {
        ModuleNodes = new Dictionary < string, ModuleNode >();
    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
