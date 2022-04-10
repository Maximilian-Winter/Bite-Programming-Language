using System.Collections.Generic;
using System.Linq;

namespace Bite.Ast
{

public class ModuleDependencyNodeFactory
{
    private int m_DependencyId = 0;

    #region Public

    public ModuleDependencyNode Create( ModuleNode module )
    {
        return new ModuleDependencyNode( module, m_DependencyId++ );
    }

    #endregion
}

public class ModuleDependencyNode
{
    private readonly List < ModuleDependencyNode > _children = new List < ModuleDependencyNode >();

    public int DependencyId { get; internal set; }

    public string Id { get; }

    public ModuleNode Module { get; }

    public ModuleDependencyNode Parent { get; private set; }

    public IReadOnlyCollection < ModuleDependencyNode > Children => _children;

    #region Public

    public ModuleDependencyNode( ModuleNode module, int dependencyId )
    {
        DependencyId = dependencyId;
        Module = module;
        Id = module.ModuleIdent.ModuleId.Id;
    }

    public void AddChild( ModuleDependencyNode node )
    {
        node.Parent = this;
        _children.Add( node );
    }

    #endregion
}

public class ProgramNode : HeteroAstNode
{
    private readonly Dictionary < string, ModuleNode > m_ModuleNodes;

    public IEnumerable < ModuleNode > ModuleNodes => m_ModuleNodes.Values;

    #region Public

    public ProgramNode()
    {
        m_ModuleNodes = new Dictionary < string, ModuleNode >();
    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    public void AddModule( ModuleNode module )
    {
        string moduleKey = module.ModuleIdent.ToString();

        if ( !m_ModuleNodes.ContainsKey( moduleKey ) )
        {
            m_ModuleNodes.Add( moduleKey, module );
        }
        else
        {
            m_ModuleNodes[moduleKey].AddStatements( module.Statements );
        }
    }

    public IEnumerable < ModuleNode > GetModulesInDepedencyOrder()
    {
        ModuleDependencyNode root = MakeDependencyTree( m_ModuleNodes.Values );

        HashSet < int > hashset = new HashSet < int >();

        // Traverse the dependency tree, returning nodes starting at the leaves
        // The hashset will ensure that we never returnn the same node twice, in case
        // two modules import the same module

        foreach ( ModuleDependencyNode dependencyNode in Traverse( root ) )
        {
            if ( !hashset.Contains( dependencyNode.DependencyId ) )
            {
                hashset.Add( dependencyNode.DependencyId );

                yield return dependencyNode.Module;
            }
        }
    }

    #endregion

    #region Private

    private ModuleDependencyNode MakeDependencyTree( IEnumerable < ModuleNode > modules )
    {
        ModuleDependencyNodeFactory factory = new ModuleDependencyNodeFactory();

        List < ModuleDependencyNode > dependencyNodes = modules.Select( m => factory.Create( m ) ).ToList();

        Dictionary < string, ModuleDependencyNode > moduleIdLookup = dependencyNodes.ToDictionary( m => m.Id );

        foreach ( ModuleDependencyNode dependencyNode in dependencyNodes )
        {
            foreach ( ModuleIdentifier importedModule in dependencyNode.Module.ImportedModules )
            {
                // Only set relationships for imports in our modules. Ignore System, for example
                if ( moduleIdLookup.TryGetValue( importedModule.ModuleId.Id, out ModuleDependencyNode childNode ) )
                {
                    moduleIdLookup[dependencyNode.Id].AddChild( childNode );
                }
            }
        }

        // && n.Id != "System" 
        // Hack to ensure that if System is not imported, it will not be returned root node
        return dependencyNodes.FirstOrDefault( n => n.Parent == null && n.Id != "System" );
    }

    private IEnumerable < ModuleDependencyNode > Traverse( ModuleDependencyNode node )
    {
        foreach ( ModuleDependencyNode child in node.Children )
        {
            foreach ( ModuleDependencyNode dependencyNode in Traverse( child ) )
            {
                yield return dependencyNode;
            }
        }

        yield return node;
    }

    #endregion
}

}
