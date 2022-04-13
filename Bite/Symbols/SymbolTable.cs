using System.Collections.Generic;

namespace Bite.Symbols
{

public class SymbolTable
{
    public SymbolTable( )
    {
        RootScope = new GlobalScope();
        CurrentScope = RootScope;
    }

    /// <summary>
    /// Defines the specified module on the Global scope
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

    internal Scope CurrentScope { get; private set; }
    
    public GlobalScope RootScope { get; private set; }
}

}