using System;
using System.Diagnostics;
using Bite.Compiler;
using Bite.Runtime;
using Bite.Runtime.CodeGen;

namespace Bite.Cli
{

public class REPL
{
    private static void PrintModule( string module )
    {
        var lines = module.Split( new[] { "\r\n" }, StringSplitOptions.None );

        foreach ( var line in lines )
        {
            Console.WriteLine( $"> {line}" );
        }
    }

    public static void Start()
    {
        Console.WriteLine( "Starting Bite interactive command prompt...\r\n" );

        Console.WriteLine( "type 'declare' to declare functions and classes" );
        Console.WriteLine( "type 'reset' to reset the module" );
        Console.WriteLine( "type 'help' to display help." );
        Console.WriteLine( "type 'exit' or press CTRL+Z and press Enter to quit\r\n" );

        string module = "module MainModule;\r\nimport System;\r\nusing System;\r\n";

        PrintModule( module );

        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();

        BiteProgram program = null;

        BiteCompiler compiler = new BiteCompiler();

        program = compiler.Compile( new[] { module } );

        bool running = true;
        bool declaring = false;
        bool resetting = false;

        while ( running )
        {
            if ( !declaring )
            {
                Console.Write( "> " );
            }

            var buffer = ConsoleEx.Buffer( !declaring, out bool ctrlZPressed );


            if ( ctrlZPressed )
            {
                if ( declaring )
                {
                    Console.WriteLine( "-- DECLARE END --" );
                    declaring = false;
                }
                else
                {
                    running = false;
                }
            }



            if ( !declaring )
            {
                var bufferString = buffer.ToString();

                if ( bufferString.Length > 0 )
                {
                    switch ( bufferString.Trim().ToLower() )
                    {
                        case "exit":
                            running = false;

                            break;

                        case "reset":
                            resetting = true;

                            break;

                        case "declare":
                            declaring = true;

                            break;
                    }

                    if ( declaring )
                    {
                        Console.WriteLine( "-- DECLARE START --" );

                        Console.WriteLine(
                            "You are now declaring. Type ^Z and press Enter to end and compile your declaration." );

                    }
                    else if ( resetting )
                    {
                        program = compiler.Compile( new[] { module } );

                        Console.Clear();
                        PrintModule( module );

                        resetting = false;
                    }
                    else if ( running )
                    {
                        compiler = new BiteCompiler();

                        try
                        {
                            program = compiler.CompileStatementsWithSymbolTable( bufferString, program.SymbolTable );
                            BiteVmInterpretResult result = biteVm.Interpret( program );
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine( e.Message );
                        }
                    }
                }
            }

        }

        Console.WriteLine( "\r\n\r\nGoodbye!\r\n" );
    }
}

}
