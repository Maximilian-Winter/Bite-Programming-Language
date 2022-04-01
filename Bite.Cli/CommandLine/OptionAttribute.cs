using System;


namespace Bite.Cli.CommandLine
{

    public class OptionAttribute : Attribute
    {
        public char ShortName { get; }
        public string LongName { get; }
        public bool Required { get; }
        public object Defaultvalue { get; }
        public string ShortDescription { get; }
        public string Description { get; }

        public OptionAttribute(char shortName, string longName, object defaultvalue, string shortDescription, string description)
        {
            ShortName = shortName;
            LongName = longName;
            Defaultvalue = defaultvalue;
            ShortDescription = shortDescription;
            Description = description;
        }

        public OptionAttribute(char shortName, string longName, bool required, string shortDescription, string description)
        {
            ShortName = shortName;
            LongName = longName;
            Required = required;
            ShortDescription = shortDescription;
            Description = description;
        }
    }
}
