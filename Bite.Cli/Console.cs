using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Cli
{

public class Console
{
    public static bool KeyAvailable => System.Console.KeyAvailable;

    public static void WriteLine()
    {
        System.Console.WriteLine();
    }

    public static void WriteLine( string value )
    {
        System.Console.WriteLine( value );
    }

    public static void Write( string value )
    {
        System.Console.Write( value );
    }

    public static void Clear()
    {
        System.Console.Clear();
    }

    private static List < string > History = new List < string >();
    private static int HistoryPosition = 0;

    private static void ClearLine()
    {
        System.Console.Write( new string( ' ', 80 ) );
    }

    public static string Buffer( bool exitOnEnter, out bool ctrlZPressed )
    {
        var buffering = true;
        var buffer = new StringBuilder();
        ctrlZPressed = false;
        var left = System.Console.CursorLeft;
        var top = System.Console.CursorTop;

        while ( buffering )
            if ( System.Console.KeyAvailable )
            {
                var keyInfo = System.Console.ReadKey( true );

                switch ( keyInfo.Key )
                {
                    case ConsoleKey.UpArrow:
                        if ( History.Count > 0 )
                        {
                            if (HistoryPosition > 0)
                            {
                                HistoryPosition--;
                            }
                            else
                            {
                                HistoryPosition = 0;
                            }

                            buffer.Clear();
                            buffer.Append( History[HistoryPosition] );
                            System.Console.CursorVisible = false;
                            System.Console.SetCursorPosition( left, top );
                            ClearLine();
                            System.Console.SetCursorPosition( left, top );
                            System.Console.Write( History[HistoryPosition] );
                            System.Console.CursorVisible = true;
                        }

                        break;

                    case ConsoleKey.DownArrow:
                        if ( History.Count > 0 )
                        {
                            if ( HistoryPosition < History.Count - 1 )
                            {
                                HistoryPosition++;
                            }
                            else
                            {
                                HistoryPosition = History.Count - 1;
                            }

                            buffer.Clear();
                            buffer.Append( History[HistoryPosition] );
                            System.Console.CursorVisible = false;
                            System.Console.SetCursorPosition( left, top );
                            ClearLine();
                            System.Console.SetCursorPosition( left, top );
                            System.Console.Write( History[HistoryPosition] );
                            System.Console.CursorVisible = true;
                        }

                        break;

                    case ConsoleKey.Z when ( keyInfo.Modifiers & ConsoleModifiers.Control ) != 0:
                        buffering = false;
                        ctrlZPressed = true;

                        break;

                    case ConsoleKey.Enter:
                    {
                        History.Add( buffer.ToString() );
                        HistoryPosition = History.Count;

                        if ( exitOnEnter )
                        {
                            buffering = false;
                        }
                        else
                        {
                            buffer.Append( "\r\n" );
                        }

                        System.Console.Write( "\r\n" );

                        break;
                    }

                    case ConsoleKey.Backspace:
                    {
                        if ( buffer.Length > 0 )
                        {
                            buffer.Remove( buffer.Length - 1, 1 );
                            System.Console.CursorVisible = false;
                            System.Console.Write( "\x8 \x8" );
                            System.Console.CursorVisible = true;
                        }

                        break;
                    }

                    default:
                        System.Console.Write( keyInfo.KeyChar );
                        buffer.Append( keyInfo.KeyChar );

                        break;
                }
            }

        return buffer.ToString();
    }


}

}