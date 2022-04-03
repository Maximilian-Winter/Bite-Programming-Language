namespace Bite.SymbolTable
{

public class ParameterSymbol : DynamicVariable
{
    #region Public

    public ParameterSymbol( string name ) : base( name, AccesModifierType.None, ClassAndMemberModifiers.None )
    {
    }

    #endregion
}

}
