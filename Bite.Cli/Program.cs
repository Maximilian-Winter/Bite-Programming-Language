using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bite.Cli.CommandLine;
using Bite.Compiler;
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
                    BiteCompiler compiler = new BiteCompiler();

                    BiteProgram program = compiler.Compile( o.Modules.Select( File.ReadAllText ) );

                    program.Run();
                }
                else if ( o.Path != null )
                {
                    IEnumerable < string > files = Directory.EnumerateFiles(
                        o.Path,
                        "*.bite",
                        SearchOption.AllDirectories );

                    BiteCompiler compiler = new BiteCompiler();

                    BiteProgram program = compiler.Compile( files.Select( File.ReadAllText ) );

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
