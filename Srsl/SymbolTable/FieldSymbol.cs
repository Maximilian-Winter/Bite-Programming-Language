namespace Srsl.SymbolTable
{

    public class FieldSymbol : DynamicVariable, MemberSymbol
    {
        public virtual int SlotNumber => slot;

        #region Public

        public FieldSymbol(
            string name,
            AccesModifierType accesModifierType,
            ClassAndMemberModifiers classAndMemberModifiers) : base(name, accesModifierType, classAndMemberModifiers)
        {
        }

        #endregion

        protected internal int slot;
    }

}
