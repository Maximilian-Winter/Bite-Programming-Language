using System;
using System.Collections.Generic;
using Bite.Ast;

namespace Bite.SymbolTable
{

    public abstract class DataAggregateSymbol : SymbolWithScope, MemberSymbol, Type
    {
        public virtual DeclarationNode DefNode
        {
            set => m_DefinitionNode = value;
            get => m_DefinitionNode;
        }

        public override IList<Symbol> Symbols => base.Symbols;

        public override IDictionary<string, Symbol> Members => base.Members;

        public virtual int NumberOfDefinedFields
        {
            get
            {
                int n = 0;

                foreach (Symbol s in Symbols)
                {
                    if (s is MemberSymbol f)
                    {
                        if (f is FieldSymbol)
                        {
                            n++;
                        }
                    }
                }

                return n;
            }
        }

        public virtual int NumberOfFields => NumberOfDefinedFields;

        public virtual IList<FieldSymbol> DefinedFields
        {
            get
            {
                IList<FieldSymbol> fields = new List<FieldSymbol>();

                foreach (Symbol s in Symbols)
                {
                    if (s is MemberSymbol f)
                    {
                        if (f is FieldSymbol t)
                        {
                            fields.Add(t);
                        }
                    }
                }

                return fields;
            }
        }

        public virtual IList<FieldSymbol> Fields => DefinedFields;

        public ClassAndMemberModifiers ClassAndMemberModifiers { get; }

        public AccesModifierType AccesModifier => m_AccessModifier;

        public virtual int TypeIndex
        {
            get => typeIndex;
            set => typeIndex = value;
        }

        int MemberSymbol.SlotNumber => -1;

        #region Public

        public DataAggregateSymbol(
            string name,
            AccesModifierType accessModifier,
            ClassAndMemberModifiers classAndMemberModifiers) : base(name)
        {
            m_AccessModifier = accessModifier;
            m_ClassAndMemberModifier = classAndMemberModifiers;
        }

        public override void define(Symbol sym)
        {
            if (!(sym is MemberSymbol))
            {
                throw new ArgumentException("sym is " + sym.GetType().Name + " not MemberSymbol");
            }

            base.define(sym);
            SetSlotNumber(sym);
        }

        public virtual Symbol resolveField(string name)
        {
            Symbol s = resolveMember(name);

            if (s is FieldSymbol)
            {
                return s;
            }

            return null;
        }

        public virtual Symbol resolveMember(string name)
        {
            Symbol s = symbols[name];

            if (s is MemberSymbol)
            {
                return s;
            }

            return null;
        }

        public virtual void SetSlotNumber(Symbol sym)
        {
            if (sym is FieldSymbol)
            {
                FieldSymbol fsym = (FieldSymbol)sym;
                fsym.slot = nextFreeFieldSlot++;
            }
        }

        #endregion

        protected internal AccesModifierType m_AccessModifier;
        protected internal ClassAndMemberModifiers m_ClassAndMemberModifier;
        protected internal DeclarationNode m_DefinitionNode;

        protected internal int nextFreeFieldSlot = 0;
        protected internal int typeIndex;
    }

}
