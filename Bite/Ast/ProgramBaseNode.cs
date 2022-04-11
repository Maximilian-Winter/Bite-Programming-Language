using System.Collections.Generic;
using System.Linq;

namespace Bite.Ast
{

public class ModuleDependencyNodeFactory
{
    private int m_DependencyId = 0;

    #region Public

    public ModuleDependencyNode Create( ModuleBaseNode moduleBase )
    {
        return new ModuleDependencyNode( moduleBase, m_DependencyId++ );
    }

    #endregion
}

public class ModuleDependencyNode
{
    private readonly List < ModuleDependencyNode > _children = new List < ModuleDependencyNode >();

    public int DependencyId { get; internal set; }

    public string Id { get; }

    public ModuleBaseNode ModuleBase { get; }

    public ModuleDependencyNode Parent { get; private set; }

    public IReadOnlyCollection < ModuleDependencyNode > Children => _children;

    #region Public

    public ModuleDependencyNode( ModuleBaseNode moduleBase, int dependencyId )
    {
        DependencyId = dependencyId;
        ModuleBase = moduleBase;
        Id = moduleBase.ModuleIdent.ModuleId.Id;
    }

    public void AddChild( ModuleDependencyNode node )
    {
        node.Parent = this;
        _children.Add( node );
    }

    #endregion
}

public class ProgramBaseNode : AstBaseNode
{
    private readonly Dictionary < string, ModuleBaseNode > m_ModuleNodes;

    public IEnumerable < ModuleBaseNode > ModuleNodes => m_ModuleNodes.Values;

    #region Public

    public ProgramBaseNode()
    {
        m_ModuleNodes = new Dictionary < string, ModuleBaseNode >();
    }

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    public void AddModule( ModuleBaseNode moduleBase )
    {
        string moduleKey = moduleBase.ModuleIdent.ToString();

        if ( !m_ModuleNodes.ContainsKey( moduleKey ) )
        {
            m_ModuleNodes.Add( moduleKey, moduleBase );
        }
        else
        {
            m_ModuleNodes[moduleKey].AddStatements( moduleBase.Statements );
        }
    }

    public IEnumerable < ModuleBaseNode > GetModulesInDepedencyOrder()
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

                yield return dependencyNode.ModuleBase;
            }
        }
    }

    #endregion

    #region Private

    private ModuleDependencyNode MakeDependencyTree( IEnumerable < ModuleBaseNode > modules )
    {
        ModuleDependencyNodeFactory factory = new ModuleDependencyNodeFactory();

        List < ModuleDependencyNode > dependencyNodes = modules.Select( m => factory.Create( m ) ).ToList();

        Dictionary < string, ModuleDependencyNode > moduleIdLookup = dependencyNodes.ToDictionary( m => m.Id );

        foreach ( ModuleDependencyNode dependencyNode in dependencyNodes )
        {
            foreach ( ModuleIdentifier importedModule in dependencyNode.ModuleBase.ImportedModules )
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
