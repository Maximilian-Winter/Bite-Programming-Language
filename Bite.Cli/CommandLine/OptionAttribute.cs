using System;

namespace Bite.Cli
{

    public class OptionAttribute : Attribute
    {
        public char ShortName { get; }
        public string LongName { get; }
        public bool Required { get; }
        public object Defaultvalue { get; }

        public OptionAttribute(char shortName, string longName, object defaultvalue)
        {
            ShortName = shortName;
            LongName = longName;
            Defaultvalue = defaultvalue;
        }

        public OptionAttribute(char shortName, string longName, bool required)
        {
            ShortName = shortName;
            LongName = longName;
            Required = required;
        }
    }
}
