using System;
using System.Text;

namespace Bite.Cli
{

public class ConsoleEx
{
    #region Public

    public static string Buffer( bool exitOnEnter, out bool ctrlZPressed )
    {
        bool buffering = true;
        StringBuilder buffer = new StringBuilder();
        ctrlZPressed = false;

        while ( buffering )
        {
            string input = Console.ReadLine();

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

    #endregion
}

}
