namespace Srsl_Parser.SymbolTable
{

public class LocalScope : BaseScope
{
    public override string Name => "local";

    #region Public

    public LocalScope( Scope enclosingScope ) : base( enclosingScope )
    {
    }

    #endregion
}

}
