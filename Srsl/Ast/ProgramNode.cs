using System.Collections.Generic;

namespace Srsl.Ast
{

    public class ProgramNode : HeteroAstNode
    {
        public string MainModule { get; }
        public Dictionary<string, ModuleNode> ModuleNodes { get; }

        #region Public

        public ProgramNode(string mainModule)
        {
            MainModule = mainModule;
            ModuleNodes = new Dictionary<string, ModuleNode>();
        }

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public void AddModule(ModuleNode module)
        {
            var moduleKey = module.ModuleIdent.ToString();
            if (!ModuleNodes.ContainsKey(moduleKey))
            {
                ModuleNodes.Add(moduleKey, module);
            }
            else
            {
                ModuleNodes[moduleKey].AddStatements(module.Statements);
            }
        }

        #endregion
    }

}
