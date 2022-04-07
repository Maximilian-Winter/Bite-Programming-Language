using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bite.Cli.CommandLine
{

/// <summary>
///     A quick'n'dirty commandline parser with an API similar to https://github.com/commandlineparser/commandline
/// </summary>
public class CommandLineArgs
{
    private enum ArgState
    {
        ReadOption,
        ReadValue
    }

    private readonly string[] m_Args;

    #region Public

    public CommandLineArgs( string[] args )
    {
        m_Args = args;
    }

    public void Parse<T>( Action < T > success ) where T : new()
    {
        T options = new T();

        IEnumerable < PropertyOption > propertyOptions = GetPropertyOptions < T >();

        if ( m_Args.Length > 0 )
        {
            ArgState state = ArgState.ReadOption;

            Dictionary < char, PropertyOption > propertyLookupByShortName =
                propertyOptions.ToDictionary( p => p.ShortName );

            Dictionary < string, PropertyOption > propertyLookupByLongName =
                propertyOptions.ToDictionary( p => p.LongName );

            PropertyOption currentPropertyOption = null;

            try
            {
                foreach ( string arg in m_Args )
                {
                    switch ( state )
                    {
                        case ArgState.ReadOption:
                            currentPropertyOption = null;

                            if ( arg.StartsWith( "-" ) && arg.Length == 2 )
                            {
                                if ( !propertyLookupByShortName.TryGetValue( arg[1], out currentPropertyOption ) )
                                {
                                    throw new Exception( $"Unknown option {arg}" );
                                }

                                if ( currentPropertyOption.Property.PropertyType == typeof( bool ) )
                                {
                                    currentPropertyOption.Property.SetValue( options, true );
                                }

                                state = ArgState.ReadValue;
                            }
                            else if ( arg.StartsWith( "--" ) )
                            {
                                if ( !propertyLookupByLongName.TryGetValue(
                                        arg.Substring( 2 ),
                                        out currentPropertyOption ) )
                                {
                                    throw new Exception( $"Unknown option {arg}" );
                                }

                                state = ArgState.ReadValue;
                            }
                            else
                            {
                                throw new Exception( $"Invalid option {arg}" );
                            }

                            break;

                        case ArgState.ReadValue:
                            Type propertyType = currentPropertyOption.Property.PropertyType;
                            PropertyInfo property = currentPropertyOption.Property;

                            if ( propertyType == typeof( string ) )
                            {
                                property.SetValue( options, arg );
                            }
                            else if ( propertyType == typeof( int ) )
                            {
                                property.SetValue( options, int.Parse( arg ) );
                            }
                            else if ( propertyType == typeof( double ) )
                            {
                                property.SetValue( options, double.Parse( arg ) );
                            }

                            state = ArgState.ReadOption;

                            break;
                    }
                }

                success( options );
            }
            catch
            {
                OnFail( propertyOptions );
            }

        }
        else
        {
            success( options );
        }
    }

    #endregion

    #region Private

    private IEnumerable < PropertyOption > GetPropertyOptions < T >()
    {
        Type type = typeof( T );
        PropertyInfo[] properties = type.GetProperties();

        foreach ( PropertyInfo property in properties )
        {
            OptionAttribute optionAttribute = property.GetCustomAttributes < OptionAttribute >().FirstOrDefault();

            if ( optionAttribute != null )
            {
                yield return new PropertyOption
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

    private void OnFail( IEnumerable < PropertyOption > propertyOptions )
    {
        int longestNameLength = propertyOptions.Select( p => p.LongName.Length ).Max();

        string appName = Path.GetFileName( Assembly.GetExecutingAssembly().Location );

        Console.WriteLine( "USAGE:\r\n" );
        Console.WriteLine( $"  {appName} <OPTIONS>" );

        Console.WriteLine();

        Console.WriteLine( "OPTIONS:\r\n" );

        foreach ( PropertyOption propertyOption in propertyOptions )
        {
            string longName = propertyOption.LongName;
            string spaces = new string( ' ', longestNameLength - propertyOption.LongName.Length );

            Console.WriteLine(
                $"  -{propertyOption.ShortName}  (--{longName}){spaces} : {propertyOption.Description}" );
        }
    }

    #endregion
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
