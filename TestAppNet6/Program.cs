using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bite.Compiler;
using Bite.Runtime;
using Bite.Runtime.Bytecode;
using Bite.Runtime.CodeGen;

namespace TestAppNet6;

public class Program
{
    #region Public

    public static void Main( string[] args )
    {
        TestCodeModules();

        PerfTests();

        Console.ReadLine();
    }

    public static void PerfTests()
    {
        BiteCompiler compiler = new();

        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "*.bite",
            SearchOption.AllDirectories );

        Stopwatch stopwatch = new();
        stopwatch.Start();
        BiteProgram program = compiler.Compile( files.Select( File.ReadAllText ) );
        stopwatch.Stop();
        Console.WriteLine( $"Parsing completed in {stopwatch.ElapsedMilliseconds}ms" );

        int k = 5;
        long elapsedMillisecondsAccu = 0;

        for ( int i = 0; i < k; i++ )
        {
            Stopwatch stopwatch2 = new();
            stopwatch2.Start();
            BiteResult result = program.Run();
            stopwatch2.Stop();

            Console.WriteLine( result.ReturnValue.ToString() );

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
    }

    public static void TestCodeModules()
    {
        List < Module > modules = new()
        {
            new()
            {
                Name = "CSharpSystem",
                Imports = new[] { "System" },
                Code = @"
var CSharpInterfaceObject = new NetInterfaceObject();
CSharpInterfaceObject.Type = ""System.Console, System.Console"";
var Console = NetLanguageInterface(CSharpInterfaceObject);
"
            },
            new()
            {
                Name = "MainModule",
                MainModule = true,
                Imports = new[] { "CSharpSystem" },
                Code = @"
var a = ""Hello"";
var b = ""World!"";
var greeting = a + "" "" + b;
Console.WriteLine(greeting);"
            }
        };

        BiteCompiler compiler = new();

        BiteProgram program = compiler.Compile( modules );

        program.Run();
    }

    public static void TestProgram()
    {
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "*.bite",
            SearchOption.AllDirectories );

        BiteCompiler compiler = new();
        BiteProgram program = compiler.Compile( files.Select( File.ReadAllText ) );
        BiteResult vm = program.Run();
    }

    #endregion
}
