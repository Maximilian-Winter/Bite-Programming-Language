using System.Diagnostics;
using Bite.Runtime;
using Bite.Runtime.CodeGen;

namespace Bite.Cli
{

public class REPL
{

    public static void Start()
    {
        Console.WriteLine( "Bite REPL(Read Evaluate Print Loop)\r\n" );
        Console.WriteLine( "type 'declare' to declare functions, structs and classes" );
        Console.WriteLine( "type 'reset' to reset the module" );
        Console.WriteLine( "type 'help' for help." );
        Console.WriteLine( "type 'exit' or ^Z to quit. type 'help' for help." );
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();

        BiteProgram program = null;

        BITECompiler compiler = new BITECompiler();

        program = compiler.Compile( new[] { "module MainModule; import System; using System;" } );

        bool running = true;
        bool declaring = false;
        bool resetting = false;

        while ( running )
        {
            if ( !declaring )
            {
                Console.Write( "> " );
            }

            var buffer = Console.Buffer( !declaring, out bool ctrlZPressed );


            if ( declaring && ctrlZPressed )
            {
                System.Console.WriteLine( "-- DECLARE END --" );
                declaring = false;
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
                            "You are now declaring. Press ^Z to stop and compile your declaration." );

                    }
                    else if ( resetting )
                    {
                        program = compiler.Compile( 
                            new[] { "module MainModule; import System; using System;" } );

                        Console.Clear();
                        Console.WriteLine( "-- MODULE RESET --" );
                        resetting = false;
                    }
                    else if ( running )
                    {
                        compiler = new BITECompiler();
                        program = compiler.CompileStatements( bufferString, program );
                        BiteVmInterpretResult result = biteVm.Interpret( program, false );
                    }
                }
            }

        }

        Console.WriteLine( "\r\n\r\nGoodbye!\r\n" );
    }
}

}
