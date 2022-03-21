using System.Collections.Generic;
using System.Text;

namespace Srsl_Parser.SymbolTable
{

    public abstract class SymbolWithScope : BaseScope, Symbol, Scope
    {
        public override string Name => name;

        public virtual Scope SymbolScope
        {
            get => enclosingScope;
            set => EnclosingScope = value;
        }

        public override Scope EnclosingScope => enclosingScope;

        public virtual string QualifiedName => enclosingScope.Name + "." + name;

        public virtual int InsertionOrderNumber
        {
            get => index;
            set => index = value;
        }

        public override int NumberOfSymbols => symbols.Count;

        #region Public

        public SymbolWithScope(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Symbol))
            {
                return false;
            }

            if (obj == this)
            {
                return true;
            }

            return name.Equals(((Symbol)obj).Name);
        }

        public virtual string getFullyQualifiedName(string scopePathSeparator)
        {
            List<Scope> path = new List<Scope>(EnclosingPathToRoot);
            path.Reverse();

            if (path.Count == 0)
            {
                return "";
            }

            StringBuilder buf = new StringBuilder();
            buf.Append(path[0].Name);

            for (int i = 1; i < path.Count; i++)
            {
                Scope s = path[i];
                buf.Append(scopePathSeparator);
                buf.Append(s.Name);
            }

            return buf.ToString();
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public virtual string getQualifiedName(string scopePathSeparator)
        {
            return enclosingScope.Name + scopePathSeparator + name;
        }

        #endregion

        protected internal int index;
        protected internal readonly string name;
    }

}
