using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Srsl.Cli
{
    /// <summary>
    /// A quick'n'dirty commandline parser with an API similar to https://github.com/commandlineparser/commandline
    /// </summary>
    public class CommandLineArgs
    {
        private readonly string[] m_Args;

        private enum ArgState
        {
            ReadOption,
            ReadValue
        }

        public CommandLineArgs(string[] args)
        {
            m_Args = args;
        }

        public void Parse<T>(Action<T> success) where T : new()
        {
            var options = new T();

            var propertyOptions = GetPropertyOptions<T>();

            if (m_Args.Length > 0)
            {

                var state = ArgState.ReadOption;


                var propertyLookupByShortName = propertyOptions.ToDictionary(p => p.ShortName);
                var propertyLookupByLongName = propertyOptions.ToDictionary(p => p.LongName);

                PropertyOption currentPropertyOption = null;

                foreach (var arg in m_Args)
                {
                    switch (state)
                    {
                        case ArgState.ReadOption:
                            currentPropertyOption = null;
                            if (arg.StartsWith("-") && arg.Length == 2)
                            {
                                if (!propertyLookupByShortName.TryGetValue(arg[1], out currentPropertyOption))
                                {
                                    throw new Exception($"Unknown option {arg}");
                                }
                                state = ArgState.ReadValue;
                            }
                            else if (arg.StartsWith("--"))
                            {
                                if (!propertyLookupByLongName.TryGetValue(arg.Substring(2), out currentPropertyOption))
                                {
                                    throw new Exception($"Unknown option {arg}");
                                }
                                state = ArgState.ReadValue;
                            }
                            else
                            {
                                throw new Exception($"Invalid option {arg}");
                            }
                            break;
                        case ArgState.ReadValue:
                            var propertyType = currentPropertyOption.Property.PropertyType;
                            var property = currentPropertyOption.Property;

                            if (propertyType == typeof(string))
                            {
                                property.SetValue(options, arg);
                            }
                            else if (propertyType == typeof(int))
                            {
                                property.SetValue(options, int.Parse(arg));
                            }
                            else if (propertyType == typeof(double))
                            {
                                property.SetValue(options, double.Parse(arg));
                            }

                            state = ArgState.ReadOption;
                            break;

                    }
                }

                success(options);
            }
            else
            {
                OnFail(propertyOptions);
            }

        }

        private void OnFail(IEnumerable<PropertyOption> propertyOptions)
        {
            var longestNameLength = propertyOptions.Select(p => p.LongName.Length).Max();

            var appName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            Console.WriteLine("USAGE:\r\n");
            Console.WriteLine($"  {appName} <OPTIONS>");


            Console.WriteLine();

            Console.WriteLine("OPTIONS:\r\n");

            foreach (var propertyOption in propertyOptions)
            {
                var longName = propertyOption.LongName;
                var spaces = new String(' ', longestNameLength - propertyOption.LongName.Length);
                Console.WriteLine($"  -{propertyOption.ShortName}  (--{longName}){spaces} : {propertyOption.Description}");
            }
        }

        private IEnumerable<PropertyOption> GetPropertyOptions<T>()
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var optionAttribute = property.GetCustomAttributes<OptionAttribute>().FirstOrDefault();
                if (optionAttribute != null)
                {
                    yield return new PropertyOption()
                    {
                        Property = property,
                        ShortName = optionAttribute.ShortName,
                        LongName = optionAttribute.LongName,
                        Required = optionAttribute.Required,
                        DefaultValue = optionAttribute.Defaultvalue,
                        Description = optionAttribute.Description
                    };
                }

            }
        }

    }


    public class PropertyOption
    {
        public PropertyInfo Property { get; set; }
        public char ShortName { get; set; }
        public string LongName { get; set; }
        public bool Required { get; set; }
        public object DefaultValue { get; set; }
        public string Description { get; set; }
    }
}

