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
                    biteVm.
                    biteVm.InitVm(  );

                    var statements = Console.ReadLine();

                    while ( statements != "exit" )
                    {
                        Compiler compiler = new Compiler( true );
                        BiteProgram program = compiler.CompileStatements( statements );
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
