using System.Collections.Generic;

namespace Srsl_Parser.SymbolTable
{

    public class ParametersSymbol : BaseSymbol
    {
        public List<ParameterSymbol> ParameterSymbols;

        #region Public

        public ParametersSymbol(string name) : base(name)
        {
        }

        #endregion
    }

}
