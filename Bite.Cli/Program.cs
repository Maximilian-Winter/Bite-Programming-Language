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
        Console.WriteLine( "Bite Programming Langauge v0.1 (c) 2022\r\n" );
        CommandLineArgs commandLine = new CommandLineArgs( args );

        commandLine.Parse < Options >(
            o =>
            {
                if ( o.Modules != null )
                {
                }
                else if ( o.Path != null )
                {
                    IEnumerable < string > files = Directory.EnumerateFiles(
                        o.Path,
                        "*.bite",
                        SearchOption.AllDirectories );

                    BITECompiler compiler = new BITECompiler();

                    BiteProgram program = compiler.Compile( "MainModule", files.Select( File.ReadAllText ) );

                    program.Run();
                }
                else
                {
                    REPL.Start();
                }
            } );
    }

    #endregion
}

}
