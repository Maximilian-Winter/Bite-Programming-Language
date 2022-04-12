using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using Bite.Compiler;
using Bite.Runtime.CodeGen;

namespace Benchmarks
{

public class Benchmarks
{
    private readonly Dictionary < string, BiteProgram > programs = new Dictionary < string, BiteProgram >();

    #region Public

    public Benchmarks()
    {
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\Benchmarks",
            "*.bite",
            SearchOption.AllDirectories );

        foreach ( string file in files )
        {
            string name = Path.GetFileNameWithoutExtension( file );
            BiteCompiler compiler = new BiteCompiler();
            programs.Add( name, compiler.Compile( new[] { File.ReadAllText( file ) } ) );
        }
    }

    [Benchmark]
    public void RunFibonacci()
    {
        programs["Fibonacci"].Run();
    }

    [Benchmark]
    public void RunPrime()
    {
        programs["Prime"].Run();
    }

    #endregion
}

}