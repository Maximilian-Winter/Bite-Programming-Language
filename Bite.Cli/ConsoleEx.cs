using System;
using System.Text;

namespace Bite.Cli
{

public class ConsoleEx
{
    public static string Buffer( bool exitOnEnter, out bool ctrlZPressed )
    {
        var buffering = true;
        var buffer = new StringBuilder();
        ctrlZPressed = false;

        while ( buffering )
        {
            var input = Console.ReadLine();

            if ( input == null )
            {
                ctrlZPressed = true;
                return buffer.ToString();
            }

            buffer.Append( input );

            if ( exitOnEnter )
            {
                buffering = false;
            }
            else
            {
                buffer.AppendLine();
            }
        }

        return buffer.ToString();
    }
}

}