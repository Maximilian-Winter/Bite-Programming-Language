using System;
using System.Collections.Generic;
using Bite.Ast;
using Bite.Runtime.SymbolTable;

namespace Bite.SymbolTable
{

public class ModuleSymbol : SymbolWithScope
{
    private readonly string m_ModuleName;
    private readonly List < string > m_SearchedModules = new List < string >();

    public string ScopeName => m_ModuleName + "ModuleSymbol";

    public IEnumerable < ModuleIdentifier > ImportedModules { get; }

    public IEnumerable < ModuleIdentifier > UsedModules { get; }

    #region Public

    public ModuleSymbol(
        string moduleIdentifier,
        IEnumerable < ModuleIdentifier > importedModules,
        IEnumerable < ModuleIdentifier > usedModules ) : base( moduleIdentifier )
    {
        m_ModuleName = moduleIdentifier;
        ImportedModules = importedModules;
        UsedModules = usedModules;
    }

    public ModuleSymbol( string moduleIdentifier ) : base( moduleIdentifier )
    {
        m_ModuleName = moduleIdentifier;
        ImportedModules = new List < ModuleIdentifier >();
        UsedModules = new List < ModuleIdentifier >();
    }

    public override Symbol resolve( string name, out int moduleid, ref int depth, bool throwErrorWhenNotFound = true )
    {
        if ( symbols.ContainsKey( name ) )
        {
            moduleid = InsertionOrderNumber;

            return symbols[name];
        }

        Scope parent = EnclosingScope;
        depth++;

        if ( parent != null )
        {
            Symbol symbol = parent.resolve( name, out moduleid, ref depth, throwErrorWhenNotFound );

            if ( symbol == null )
            {
                if ( UsedModules != null )
                {
                    foreach ( ModuleIdentifier importedModule in UsedModules )
                    {
                        if ( !m_SearchedModules.Contains( importedModule.ToString() ) )
                        {
                            int i;
                            int d = 0;

                            SymbolWithScope module =
                                parent.resolve( importedModule.ToString(), out i, ref d ) as SymbolWithScope;

                            if ( module == null )
                            {
                                throw new Exception(
                                    "Module: " + importedModule + " not found in Scope: " + parent.Name );
                            }

                            m_SearchedModules.Add( importedModule.ToString() );
                            symbol = module.resolve( name, out moduleid, ref d, throwErrorWhenNotFound );

                            if ( symbol != null )
                            {
                                m_SearchedModules.Clear();

                                return symbol;
                            }

                            depth++;
                        }
                    }
                }
            }

            depth++;
            m_SearchedModules.Clear();

            if ( symbol == null && throwErrorWhenNotFound )
            {
                throw new BiteSymbolTableException( $"Compiler Error: Name '{name}' not found in current program!" );
            }
            return symbol;
        }

        m_SearchedModules.Clear();
        moduleid = -2;

        return null;
    }

    #endregion
}

}
