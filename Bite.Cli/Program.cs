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

                    BITECompiler compiler = new BITECompiler();
                    program = compiler.Compile( "MainModule", new []{ "module MainModule; import System; using System;" } );

                    while ( statements != "exit" )
                    {
                        compiler = new BITECompiler();
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

                    BITECompiler compiler = new BITECompiler();

                    BiteProgram program = compiler.Compile( o.MainModule, files.Select( File.ReadAllText ) );

                    program.Run();
                }
            } );
    }

    #endregion
}

}
