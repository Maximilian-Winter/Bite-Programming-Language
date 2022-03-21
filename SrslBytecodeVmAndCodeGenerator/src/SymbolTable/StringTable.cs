using System.Collections.Generic;

namespace Srsl_Parser.SymbolTable
{

    public class StringTable
    {
        public virtual int NumberOfStrings => index + 1;

        #region Public

        public virtual int add(string s)
        {
            if (table.ContainsKey(s))
            {
                return table[s];
            }

            index++;
            table.Add(s, index);
            strings.Add(s);

            return index;
        }

        public virtual string get(int i)
        {
            if (i < size() && i >= 0)
            {
                return strings[i];
            }

            return null;
        }

        public virtual int size()
        {
            return table.Count;
        }

        public virtual string[] toArray()
        {
            return ((List<string>)strings).ToArray();
        }

        public virtual IList<string> toList()
        {
            return strings;
        }

        public override string ToString()
        {
            return table.ToString();
        }

        #endregion

        protected internal int index = -1;
        protected internal IList<string> strings = new List<string>();
        protected internal IDictionary<string, int> table = new Dictionary<string, int>();
    }

}
