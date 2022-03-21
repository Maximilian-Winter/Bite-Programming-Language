using MemoizeSharp;
using System;
using System.Collections.Generic;

namespace Srsl_Parser.SymbolTable
{

    public class ModuleSymbol : SymbolWithScope
    {
        private string m_ModuleName;
        private List<ModuleIdentifier> m_ImportedModules;
        private List<ModuleIdentifier> m_UsedModules;
        public string ScopeName => m_ModuleName + "ModuleSymbol";
        private List<string> m_SearchedModules = new List<string>();
        public List<ModuleIdentifier> ImportedModules
        {
            get => m_ImportedModules;
        }

        public List<ModuleIdentifier> UsedModules
        {
            get => m_UsedModules;
        }

        public override Symbol resolve(string name, out int moduleid, ref int depth)
        {
            if (symbols.ContainsKey(name))
            {
                moduleid = InsertionOrderNumber;
                return symbols[name];
            }

            Scope parent = EnclosingScope;
            depth++;
            if (parent != null)
            {
                Symbol symbol = parent.resolve(name, out moduleid, ref depth);
                if (symbol == null)
                {
                    foreach (ModuleIdentifier importedModule in UsedModules)
                    {
                        if (!m_SearchedModules.Contains(importedModule.ToString()))
                        {
                            int i;
                            int d = 0;
                            SymbolWithScope module = parent.resolve(importedModule.ToString(), out i, ref d) as SymbolWithScope;

                            if (module == null)
                            {
                                throw new Exception("Module: " + importedModule + ". Not found in Scope: " + parent.Name);
                            }
                            m_SearchedModules.Add(importedModule.ToString());
                            symbol = module.resolve(name, out moduleid, ref d);

                            if (symbol != null)
                            {
                                m_SearchedModules.Clear();
                                return symbol;
                            }
                            depth++;
                        }
                    }
                }
                depth++;
                m_SearchedModules.Clear();
                return symbol;
            }
            m_SearchedModules.Clear();
            moduleid = -2;
            return null;
        }

        #region Public

        public ModuleSymbol(string moduleIdentifier, List<ModuleIdentifier> importedModules, List<ModuleIdentifier> usedModules) : base(moduleIdentifier)
        {
            m_ModuleName = moduleIdentifier;
            m_ImportedModules = importedModules;
            m_UsedModules = usedModules;
        }

        #endregion
    }

}
