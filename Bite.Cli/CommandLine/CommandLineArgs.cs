using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bite.Cli.CommandLine
{

public class CommandLineException : Exception
{
    public CommandLineException( string message ) : base( message )
    {

    }
}

/// <summary>
///     A quick'n'dirty commandline parser with an API similar to https://github.com/commandlineparser/commandline
/// </summary>
public class CommandLineArgs
{
    private enum ArgState
    {
        ReadOption,
        ReadValue,
        ReadArray
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

            List < string > arrayValues = new List < string >();

            try
            {
                foreach ( string arg in m_Args )
                {
                    if ( state == ArgState.ReadArray )
                    {
                        if ( arg.StartsWith( "-" ) && arg.Length == 2 || arg.StartsWith( "--" ) )
                        {
                            PropertyInfo property = currentPropertyOption.Property;
                            property.SetValue( options, arrayValues.ToArray() );
                            state = ArgState.ReadOption;
                        }
                    }

                    switch ( state )
                    {
                        case ArgState.ReadOption:
                            currentPropertyOption = null;

                            if ( arg.StartsWith( "-" ) && arg.Length == 2 )
                            {
                                if ( !propertyLookupByShortName.TryGetValue( arg[1], out currentPropertyOption ) )
                                {
                                    throw new CommandLineException( $"Unknown option {arg}" );
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
                                    throw new CommandLineException( $"Unknown option {arg}" );
                                }

                                state = ArgState.ReadValue;
                            }
                            else
                            {
                                throw new CommandLineException( $"Invalid option {arg}" );
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
                            else if ( propertyType.IsArray )
                            {
                                arrayValues = new List < string >() { arg };
                                state = ArgState.ReadArray;
                            }

                            if ( state != ArgState.ReadArray )
                            {
                                state = ArgState.ReadOption;
                            }

                            break;
                    }
                }


                if ( state == ArgState.ReadArray )
                {
                    PropertyInfo property = currentPropertyOption.Property;
                    property.SetValue( options, arrayValues.ToArray() );
                }

                success( options );
            }
            catch (CommandLineException)
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

    private IEnumerable < PropertyOption > GetPropertyOptions<T>()
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

        string spaces = new string( ' ', longestNameLength - "help".Length );

        Console.WriteLine( $"  -h  (--help){spaces} : this help screen" );

        foreach ( PropertyOption propertyOption in propertyOptions )
        {
            string longName = propertyOption.LongName;
            spaces = new string( ' ', longestNameLength - propertyOption.LongName.Length );

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
