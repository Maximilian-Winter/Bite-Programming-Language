using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Bite.Ast
{

[DebuggerDisplay( "{ModuleIdent.ModuleId.Id}" )]
public class ModuleNode : HeteroAstNode
{
    public ModuleIdentifier ModuleIdent;
    public IEnumerable < ModuleIdentifier > ImportedModules;
    public IEnumerable < ModuleIdentifier > UsedModules;
    public IEnumerable < StatementNode > Statements;

    #region Public

    public ModuleNode()
    {
        ModuleIdent = new ModuleIdentifier();
    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    public void AddStatements( IEnumerable < StatementNode > statementNodes )
    {
        Statements = statementNodes.ToList();
    }

    #endregion
}

}
