using MemoizeSharp;

namespace Srsl_Parser.SymbolTable
{

    public class DynamicVariable : SymbolWithScope, TypedSymbol
    {
        public HeteroAstNode DefinitionNode = null;

        public ClassAndMemberModifiers ClassAndMemberModifiers => m_ClassAndMemberModifier;

        public AccesModifierType AccesModifier => m_AccessModifier;

        public int TypeIndex => 0;

        public Type Type { get; set; }

        #region Public

        public DynamicVariable(
            string name,
            AccesModifierType accesModifierType,
            ClassAndMemberModifiers classAndMemberModifiers) : base(name)
        {
            m_AccessModifier = accesModifierType;
            m_ClassAndMemberModifier = classAndMemberModifiers;
        }

        #endregion

        protected internal AccesModifierType m_AccessModifier;
        protected internal ClassAndMemberModifiers m_ClassAndMemberModifier;
    }

}
