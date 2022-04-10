namespace Bite.Symbols
{

public interface MemberSymbol : Symbol
{
    AccesModifierType AccesModifier { get; }

    ClassAndMemberModifiers ClassAndMemberModifiers { get; }

    int SlotNumber { get; }
}

}
