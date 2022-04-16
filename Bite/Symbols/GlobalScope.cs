namespace Bite.Symbols
{

public class GlobalScope : BaseScope
{
    public override string Name => "global";

    #region Public

    public GlobalScope() : base( null )
    {
    }

    public void DefineModule( ModuleSymbol moduleSymbol )
    {
        moduleSymbol.EnclosingScope = this;
        define( moduleSymbol );
    }

    public void DefineVariable( VariableSymbol variableSymbol )
    {
        define( variableSymbol );
    }

    #endregion
}

}
