using System.Collections.Generic;
using System.Collections.Immutable;

namespace Srsl_Parser.SymbolTable
{

    public class ClassSymbol : DataAggregateSymbol
    {
        public virtual List<ClassSymbol> BaseClassesScopes
        {
            get
            {
                if (!ReferenceEquals(BaseClassNames, null))
                {
                    if (EnclosingScope != null)
                    {
                        List<ClassSymbol> classSymbols = new List<ClassSymbol>();

                        foreach (string baseClassName in BaseClassNames)
                        {
                            int i;
                            int d = 0;
                            Symbol baseClass = EnclosingScope.resolve(baseClassName, out i, ref d);

                            if (baseClass is ClassSymbol)
                            {
                                classSymbols.Add((ClassSymbol)baseClass);
                            }
                        }

                        return classSymbols;
                    }
                }

                return null;
            }
        }

        public virtual IList<ClassSymbol> BaseClassScopesImmutable
        {
            get
            {
                List<ClassSymbol> superClassScope = BaseClassesScopes;

                if (superClassScope != null)
                {
                    return ImmutableList.Create<ClassSymbol>().AddRange(superClassScope);
                }

                return null;
            }
        }

        public virtual ISet<MethodSymbol> DefinedMethods
        {
            get
            {
                ISet<MethodSymbol> methods = new HashSet<MethodSymbol>();

                foreach (MemberSymbol s in Symbols)
                {
                    if (s is MethodSymbol)
                    {
                        methods.Add((MethodSymbol)s);
                    }
                }

                return methods;
            }
        }

        public virtual ISet<MethodSymbol> Methods
        {
            get
            {
                ISet<MethodSymbol> methods = new HashSet<MethodSymbol>();
                List<ClassSymbol> superClassScope = BaseClassesScopes;

                if (superClassScope != null)
                {
                    foreach (ClassSymbol classSymbol in superClassScope)
                    {
                        foreach (MethodSymbol superClassMethod in classSymbol.Methods)
                        {
                            methods.Add(superClassMethod);
                        }
                    }
                }

                foreach (MethodSymbol item in DefinedMethods)
                {
                    if (methods.Contains(item))
                    {
                        methods.Remove(item);
                    }
                }

                foreach (MethodSymbol overridenClassMethod in DefinedMethods)
                {
                    methods.Add(overridenClassMethod);
                }

                return methods;
            }
        }

        public override IList<FieldSymbol> Fields
        {
            get
            {
                IList<FieldSymbol> fields = new List<FieldSymbol>();
                List<ClassSymbol> superClassScope = BaseClassesScopes;

                if (superClassScope != null)
                {
                    foreach (ClassSymbol classSymbol in superClassScope)
                    {
                        foreach (FieldSymbol superClassField in classSymbol.Fields)
                        {
                            fields.Add(superClassField);
                        }
                    }
                }

                foreach (FieldSymbol item in DefinedFields)
                {
                    if (fields.Contains(item))
                    {
                        fields.Remove(item);
                    }
                }

                foreach (FieldSymbol overridenClassField in DefinedFields)
                {
                    fields.Add(overridenClassField);
                }

                return fields;
            }
        }

        public virtual int NumberOfDefinedMethods
        {
            get
            {
                int n = 0;

                foreach (MemberSymbol s in Symbols)
                {
                    if (s is MethodSymbol)
                    {
                        n++;
                    }
                }

                return n;
            }
        }

        public virtual int NumberOfMethods
        {
            get
            {
                int n = 0;

                if (BaseClassesScopes != null)
                {
                    foreach (ClassSymbol baseClassesScope in BaseClassesScopes)
                    {
                        if (baseClassesScope != null)
                        {
                            n += baseClassesScope.NumberOfMethods;
                        }
                    }
                }

                n += NumberOfDefinedMethods;

                return n;
            }
        }

        public override int NumberOfFields
        {
            get
            {
                int n = 0;

                if (BaseClassesScopes != null)
                {
                    foreach (ClassSymbol baseClassesScope in BaseClassesScopes)
                    {
                        if (baseClassesScope != null)
                        {
                            n += baseClassesScope.NumberOfFields;
                        }
                    }
                }


                n += NumberOfDefinedFields;

                return n;
            }
        }

        public List<string> BaseClassNames
        {
            get => m_BaseClassNames;
            set => m_BaseClassNames = value;
        }

        #region Public

        public ClassSymbol(string name, AccesModifierType accesModifierType, ClassAndMemberModifiers structModifiers) :
            base(name, accesModifierType, structModifiers)
        {
        }

        public override Symbol resolve(string name, out int moduleId, ref int depth)
        {
            Symbol s = resolveMember(name);

            if (s != null)
            {
                moduleId = -1;
                return s;
            }

            Scope parent = EnclosingScope;

            if (parent != null)
            {
                depth++;
                return parent.resolve(name, out moduleId, ref depth);
            }
            moduleId = -2;
            return null;
        }

        public override Symbol resolveField(string name)
        {
            Symbol s = resolveMember(name);

            if (s is FieldSymbol)
            {
                return s;
            }

            return null;
        }

        public override Symbol resolveMember(string name)
        {
            if (symbols.ContainsKey(name))
            {
                return symbols[name];
            }

            IList<ClassSymbol> superClassScopes = BaseClassScopesImmutable;

            if (superClassScopes != null)
            {
                foreach (ClassSymbol sup in superClassScopes)
                {
                    Symbol s = sup.resolveMember(name);

                    if (s is MemberSymbol)
                    {
                        return s;
                    }
                }
            }

            return null;
        }

        public virtual MethodSymbol resolveMethod(string name)
        {
            Symbol s = resolveMember(name);

            if (s is MethodSymbol)
            {
                return (MethodSymbol)s;
            }

            return null;
        }

        public override void SetSlotNumber(Symbol symbol)
        {
            if (symbol is MethodSymbol)
            {
                MethodSymbol msym = (MethodSymbol)symbol;
                List<ClassSymbol> superClass = BaseClassesScopes;

                if (superClass != null)
                {
                    foreach (ClassSymbol classSymbol in superClass)
                    {
                        MethodSymbol superMethodSym = classSymbol.resolveMethod(classSymbol.Name);

                        if (superMethodSym != null)
                        {
                            msym.slot = superMethodSym.slot;

                            break;
                        }
                    }
                }

                if (msym.slot == -1)
                {
                    msym.slot = nextFreeMethodSlot++;
                }
            }
            else
            {
                base.SetSlotNumber(symbol);
            }
        }

        public override string ToString()
        {
            List<ClassSymbol> superClassScope = BaseClassesScopes;
            string baseClasses = "";

            foreach (ClassSymbol baseClassesScope in BaseClassesScopes)
            {
                if (baseClassesScope != null)
                {
                    baseClasses += baseClassesScope.ToString();
                }
            }

            return name + ":" + baseClasses;
        }

        #endregion

        protected internal List<string> m_BaseClassNames;
        protected internal int nextFreeMethodSlot = 0;
    }

}
