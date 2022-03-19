namespace Srsl_Parser.SymbolTable
{

public class StructSymbol : DataAggregateSymbol
{
    #region Public

    public StructSymbol( string name, AccesModifierType accesModifierType, ClassAndMemberModifiers structModifiers ) :
        base( name, accesModifierType, structModifiers )
    {
    }

    #endregion
}

}
