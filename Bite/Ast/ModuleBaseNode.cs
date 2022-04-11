using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Bite.Ast
{

[DebuggerDisplay( "{ModuleIdent.ModuleId.Id}" )]
public class ModuleBaseNode : AstBaseNode
{
    public ModuleIdentifier ModuleIdent;
    public IEnumerable < ModuleIdentifier > ImportedModules;
    public IEnumerable < ModuleIdentifier > UsedModules;
    public IEnumerable < StatementBaseNode > Statements;

    #region Public

    public ModuleBaseNode()
    {
        ModuleIdent = new ModuleIdentifier();
    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    public void AddStatements( IEnumerable < StatementBaseNode > statementNodes )
    {
        Statements = statementNodes.ToList();
    }

    #endregion
}

}
