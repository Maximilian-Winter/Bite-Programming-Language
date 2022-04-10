namespace Bite.Symbols
{

public interface Symbol
{
    bool Equals( object o );

    int GetHashCode();

    int InsertionOrderNumber { get; set; }

    string Name { get; }

    Scope SymbolScope { get; set; }

    bool IsExternal { get; }
}

}
