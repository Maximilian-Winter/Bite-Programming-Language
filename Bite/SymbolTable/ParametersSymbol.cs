using System.Collections.Generic;

namespace Bite.SymbolTable
{

public class ParametersSymbol : BaseSymbol
{
    public List < ParameterSymbol > ParameterSymbols;

    #region Public

    public ParametersSymbol( string name ) : base( name )
    {
    }

    #endregion
}

}
