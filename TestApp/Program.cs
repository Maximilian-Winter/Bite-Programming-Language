using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bite.Runtime;
using Bite.Runtime.Bytecode;
using Bite.Runtime.CodeGen;

namespace TestApp
{

public class Program
{
    #region Public

    public static void Main( string[] args )
    {
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm( );

        var statements = Console.ReadLine();
        BiteProgram program = null;
        while ( statements != "exit" )
        {
            Compiler compiler = new Compiler( true );
            program = compiler.CompileStatements( statements, program );
            BiteVmInterpretResult result = biteVm.Interpret( program, false );
            statements = Console.ReadLine();
        }
        
        /*
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "*.bite",
            SearchOption.AllDirectories );

        BITECompiler compiler = new BITECompiler();

        BiteProgram program = compiler.Compile( "MainModule", files.Select(File.ReadAllText));

        int k = 1;
        long elapsedMillisecondsAccu = 0;
        
        for ( int i = 0; i < k; i++ )
        {
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Start();
            program.Run();
            stopwatch2.Stop();
            Console.WriteLine( "--Elapsed Time for Interpreting Run {0} is {1} ms", i, stopwatch2.ElapsedMilliseconds );
            elapsedMillisecondsAccu += stopwatch2.ElapsedMilliseconds;
        }

        Console.WriteLine( "--Average Elapsed Time for Interpreting per Run is {0} ms", elapsedMillisecondsAccu / k );
        Console.WriteLine( "--Total Elapsed Time for Interpreting {0} Runs is {1} ms", k, elapsedMillisecondsAccu );

        IOrderedEnumerable < KeyValuePair < string, long > > sortedDict =
            from entry in ChunkDebugHelper.InstructionCounter orderby entry.Value descending select entry;

        long totalInstructions = 0;

        foreach ( KeyValuePair < string, long > keyValuePair in sortedDict )
        {
            totalInstructions += keyValuePair.Value;
        }

        foreach ( KeyValuePair < string, long > keyValuePair in sortedDict )
        {
            Console.WriteLine(
                "--Instruction Count for Instruction {0}: {2}     {1}%",
                keyValuePair.Key,
                ( 100.0 / totalInstructions * keyValuePair.Value ).ToString( "00.0" ),
                keyValuePair.Value );
        }

        ChunkDebugHelper.InstructionCounter.Clear();

        Console.ReadLine();*/
    }

    #endregion
}

}
