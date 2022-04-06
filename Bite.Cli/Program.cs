using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bite.Cli.CommandLine;
using Bite.Runtime;
using Bite.Runtime.CodeGen;

namespace Bite.Cli
{

internal class Program
{
    #region Private

    private static void Main( string[] args )
    {
        CommandLineArgs commandLine = new CommandLineArgs( args );

        commandLine.Parse < Options >(
            o =>
            {
                if ( o.Interactive )
                {

                    BiteVm biteVm = new BiteVm();
                    biteVm.InitVm(  );


                    var statements = Console.ReadLine();

                    BiteProgram program = null;

                    Compiler compiler = new Compiler( true );
                    program = compiler.Compile( "MainModule", new []{ "module MainModule; import System; using System;" } );

                    while ( statements != "exit" )
                    {
                        compiler = new Compiler( true );
                        program = compiler.CompileStatements( statements, program );
                        BiteVmInterpretResult result = biteVm.Interpret( program, false );
                        statements = Console.ReadLine();
                    }
                }
                else
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(
                        o.Path,
                        "*.bite",
                        SearchOption.AllDirectories );

                    Compiler compiler = new Compiler( true );

                    BiteProgram program = compiler.Compile( o.MainModule, files.Select( File.ReadAllText ) );

                    program.Run();
                }
            } );
    }

    #endregion
}

}
