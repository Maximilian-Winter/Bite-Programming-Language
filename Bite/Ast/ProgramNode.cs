using System.Collections.Generic;
using System.Linq;

namespace Bite.Ast
{
    public class ModuleDependencyNodeFactory
    {
        private int m_DependencyId = 0;

        public ModuleDependencyNode Create(ModuleNode module)
        {
            return new ModuleDependencyNode(module, m_DependencyId++);
        }
    }

    public class ModuleDependencyNode
    {
        private List<ModuleDependencyNode> _children = new List<ModuleDependencyNode>();
        public int DependencyId { get; internal set; }
        public string Id { get; }
        public ModuleNode Module { get; }
        public ModuleDependencyNode Parent { get; private set; }
        public IReadOnlyCollection<ModuleDependencyNode> Children => _children;

        public ModuleDependencyNode(ModuleNode module, int dependencyId)
        {
            DependencyId = dependencyId;
            Module = module;
            Id = module.ModuleIdent.ModuleId.Id;
        }

        public void AddChild(ModuleDependencyNode node)
        {
            node.Parent = this;
            _children.Add(node);
        }
    }

    public class ProgramNode : HeteroAstNode
    {
        public string MainModule { get; }
        private Dictionary<string, ModuleNode> m_ModuleNodes;

        #region Public

        public IEnumerable<ModuleNode> ModuleNodes => m_ModuleNodes.Values;

        public ProgramNode(string mainModule)
        {
            MainModule = mainModule;
            m_ModuleNodes = new Dictionary<string, ModuleNode>();
        }

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public void AddModule(ModuleNode module)
        {
            var moduleKey = module.ModuleIdent.ToString();
            if (!m_ModuleNodes.ContainsKey(moduleKey))
            {
                m_ModuleNodes.Add(moduleKey, module);
            }
            else
            {
                m_ModuleNodes[moduleKey].AddStatements(module.Statements);
            }
        }

        private ModuleDependencyNode MakeDependencyTree(IEnumerable<ModuleNode> modules)
        {
            var factory = new ModuleDependencyNodeFactory();

            var dependencyNodes = modules.Select(m => factory.Create(m)).ToList();

            var moduleIdLookup = dependencyNodes.ToDictionary(m => m.Id);

            foreach (var dependencyNode in dependencyNodes)
            {
                foreach (var importedModule in dependencyNode.Module.ImportedModules)
                {
                    if(importedModule.ModuleId.Id.StartsWith("System"))
                        continue;
                    moduleIdLookup[dependencyNode.Id].AddChild(moduleIdLookup[importedModule.ModuleId.Id]);
                }
            }

            return dependencyNodes.FirstOrDefault(n => n.Parent == null);
        }

        public ModuleNode GetMainModule()
        {
            return m_ModuleNodes.Values.FirstOrDefault(m => m.ModuleIdent.ModuleId.Id == MainModule);
        }

        public IEnumerable<ModuleNode> GetModulesInDepedencyOrder()
        {
            var root = MakeDependencyTree(m_ModuleNodes.Values);

            var hashset = new HashSet<int>();

            // Traverse the dependency tree, returning nodes starting at the leaves
            // The hashset will ensure that we never returnn the same node twice, in case
            // two modules import the same module

            foreach (var dependencyNode in Traverse(root))
            {
                if (!hashset.Contains(dependencyNode.DependencyId))
                {
                    hashset.Add(dependencyNode.DependencyId);
                    yield return dependencyNode.Module;
                }
            }
        }

        private IEnumerable<ModuleDependencyNode> Traverse(ModuleDependencyNode node)
        {
            foreach (var child in node.Children)
            {
                foreach (var dependencyNode in Traverse(child))
                {
                    yield return dependencyNode;
                }
            }
            yield return node;
        }

        #endregion
    }

}
