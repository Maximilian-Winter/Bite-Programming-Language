namespace Bite.Symbols
{

public class MethodSymbol : FunctionSymbol, MemberSymbol
{
    public bool IsConstructor;

    public virtual int SlotNumber => slot;

    #region Public

    public MethodSymbol(
        string name,
        AccesModifierType accessModifier,
        ClassAndMemberModifiers classAndMemberModifiers ) : base( name, accessModifier, classAndMemberModifiers )
    {
    }

    #endregion

    protected internal int slot = -1;
}

}
