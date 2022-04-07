using System;
using System.Diagnostics;
using System.Text;
using Bite.Runtime;
using Bite.Runtime.CodeGen;

namespace Bite.Cli
{

public class REPL
{
    public static void Start()
    {
        Console.WriteLine( "type 'declare' to declare functions, structs and classes" );
        Console.WriteLine( "type 'help' for help." );
        Console.WriteLine( "type 'exit' or ^Z to quit. type 'help' for help." );
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();

        BiteProgram program = null;

        BITECompiler compiler = new BITECompiler();

        program = compiler.Compile( "MainModule", new[] { "module MainModule; import System; using System;" } );

        bool running = true;
        bool declaring = false;

        while ( running )
        {
            var buffering = true;
            var buffer = new StringBuilder();

            if ( !declaring )
            {
                Console.Write( "> " );
            }

            while ( buffering )
                if ( Console.KeyAvailable )
                {
                    var keyInfo = Console.ReadKey( true );

                    if ( keyInfo.Key == ConsoleKey.Z && ( keyInfo.Modifiers & ConsoleModifiers.Control ) != 0 )
                    {
                        if ( declaring )
                        {
                            Console.WriteLine( "-- DECLARE END --" );
                            buffering = false;
                            declaring = false;
                            compiler = new BITECompiler();
                            program = compiler.CompileStatements( buffer.ToString(), program );
                            BiteVmInterpretResult result = biteVm.Interpret( program, false );
                        }
                        else
                        {
                            running = false;
                            Console.Write( "^Z" );
                        }
                    }
                    else if ( keyInfo.Key == ConsoleKey.Enter )
                    {
                        if ( declaring )
                        {
                            buffer.Append( "\r\n" );
                        }
                        else
                        {
                            buffering = false;
                        }

                        Console.Write( "\r\n" );
                    }
                    else if ( keyInfo.Key == ConsoleKey.Backspace )
                    {
                        if ( buffer.Length > 0 )
                        {
                            buffer.Remove( buffer.Length - 1, 1 );
                            Console.Write( "\x8 \x8" );
                        }
                    }
                    else
                    {
                        Console.Write( keyInfo.KeyChar );
                        buffer.Append( keyInfo.KeyChar );
                    }
                }

            if ( !declaring )
            {
                switch ( buffer.ToString().ToLower().Trim() )
                {
                    case "exit":
                        running = false;

                        break;

                    case "declare":
                        declaring = true;
                        buffer.Clear();
                        Console.WriteLine( "-- DECLARE START --" );
                        Console.WriteLine( "You are now declaring. Press ^Z to stop and compile your declaration." );

                        break;
                }

                if ( running && !declaring )
                {
                    compiler = new BITECompiler();
                    program = compiler.CompileStatements( buffer.ToString(), program );
                    BiteVmInterpretResult result = biteVm.Interpret( program, false );
                }

            }

        }

        Console.WriteLine( "\r\n\r\nGoodbye!\r\n" );
    }
}

}
