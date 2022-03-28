using System.Collections.Generic;
using Srsl.Ast;

namespace Srsl.SymbolTable
{

    public class FunctionSymbol : SymbolWithScope, TypedSymbol
    {
        public virtual FunctionDeclarationNode DefNode
        {
            set => defNode = value;
            get => defNode;
        }

        public virtual Type Type
        {
            get => retType;
            set => retType = value;
        }

        public virtual int NumberOfParameters
        {
            get
            {
                int i = 0;

                foreach (KeyValuePair<string, Symbol> sym in symbols)
                {
                    if (sym.Value is ParameterSymbol)
                    {
                        i++;
                    }
                }
                return i;
            }
        }

        public ClassAndMemberModifiers ClassAndMemberModifiers => m_ClassAndMemberModifier;

        public AccesModifierType AccesModifier => m_AccessModifier;

        #region Public

        public FunctionSymbol(
            string name,
            AccesModifierType accesModifierType,
            ClassAndMemberModifiers classAndMemberModifiers) : base(name)
        {
            m_AccessModifier = accesModifierType;
            m_ClassAndMemberModifier = classAndMemberModifiers;
        }

        public override string ToString()
        {
            return name + ":" + base.ToString();
        }

        #endregion

        protected internal FunctionDeclarationNode defNode;
        protected internal AccesModifierType m_AccessModifier;
        protected internal ClassAndMemberModifiers m_ClassAndMemberModifier;
        protected internal Type retType;
    }

}
