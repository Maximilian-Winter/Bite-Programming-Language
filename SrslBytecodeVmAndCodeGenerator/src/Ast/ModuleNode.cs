using System.Collections.Generic;

namespace Srsl.Ast
{

    public class ModuleNode : HeteroAstNode
    {
        public ModuleIdentifier ModuleIdent;
        public List<ModuleIdentifier> ImportedModules;
        public List<ModuleIdentifier> UsedModules;
        public List<StatementNode> Statements;


        #region Public

        public ModuleNode()
        {
            ModuleIdent = new ModuleIdentifier();
            Statements = new List<StatementNode>();
        }

        public void AddStatements(List<StatementNode> statementNodes)
        {
            foreach (StatementNode statementNode in statementNodes)
            {
                Statements.Add(statementNode);
            }
        }

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
