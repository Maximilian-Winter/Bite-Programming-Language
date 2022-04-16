namespace Bite.Symbols
{

public class SymbolTable
{
    public GlobalScope RootScope { get; }

    internal Scope CurrentScope { get; private set; }

    #region Public

    public SymbolTable()
    {
        RootScope = new GlobalScope();
        CurrentScope = RootScope;
    }

    /// <summary>
    ///     Defines the specified module on the Global scope
    /// </summary>
    /// <returns></returns>
    public SymbolTable WithModule( ModuleSymbol module )
    {
        RootScope.DefineModule( module );

        return this;
    }

    internal void PopScope()
    {
        CurrentScope = CurrentScope.EnclosingScope;
    }

    internal void PushScope( Scope s )
    {
        CurrentScope = s;
    }

    #endregion
}

}
