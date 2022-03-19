namespace Srsl_Parser.SymbolTable
{

public interface MemberSymbol : Symbol
{
    AccesModifierType AccesModifier { get; }

    ClassAndMemberModifiers ClassAndMemberModifiers { get; }

    int SlotNumber { get; }
}

}
