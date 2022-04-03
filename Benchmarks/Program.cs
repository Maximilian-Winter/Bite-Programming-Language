using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Bite.Runtime;
using Bite.Runtime.CodeGen;

namespace Benchmarks
{
    public class Benchmarks
    {
        private readonly Dictionary<string, BiteProgram> programs = new Dictionary<string, BiteProgram>();

        public Benchmarks()
        {
            var files = Directory.EnumerateFiles(".\\Benchmarks", "*.bite", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var compiler = new Compiler(true);
                programs.Add(name, compiler.Compile("MainModule", new []{ File.ReadAllText(file) }));
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
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            //var b = new Benchmarks();

            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
