using System.Collections.Generic;

namespace Srsl_Parser.SymbolTable
{

    public interface Scope
    {
        IList<Symbol> AllSymbols { get; }

        void define(Symbol sym);

        IList<Scope> EnclosingPathToRoot { get; }

        Scope EnclosingScope { get; set; }

        string Name { get; }

        void nest(Scope scope);

        Symbol resolve(string name, out int moduleId, ref int depth);
    }

}
