namespace Srsl_Parser.SymbolTable
{

    public interface Symbol
    {
        bool Equals(object o);

        int GetHashCode();

        int InsertionOrderNumber { get; set; }

        string Name { get; }

        Scope SymbolScope { get; set; }
    }

}
