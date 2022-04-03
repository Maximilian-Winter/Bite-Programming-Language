namespace Bite.SymbolTable
{

public class VariableSymbol : BaseSymbol, TypedSymbol
{
    public ClassAndMemberModifiers ClassAndMemberModifiers => m_ClassAndMemberModifier;

    public AccesModifierType AccesModifier => m_AccessModifier;

    #region Public

    public VariableSymbol(
        string name,
        AccesModifierType accesModifierType,
        ClassAndMemberModifiers classAndMemberModifiers ) : base( name )
    {
        m_AccessModifier = accesModifierType;
        m_ClassAndMemberModifier = classAndMemberModifiers;
    }

    #endregion

    protected internal AccesModifierType m_AccessModifier;
    protected internal ClassAndMemberModifiers m_ClassAndMemberModifier;
}

}
