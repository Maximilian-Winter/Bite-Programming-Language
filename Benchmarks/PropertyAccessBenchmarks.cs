using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Bite.Compiler;
using Bite.Runtime.CodeGen;

namespace Benchmarks
{

public class PropertyAccessBenchmarks
{
    string statements = @"module Main;

            class foo {
                var x = 5;
                var y = 2;
            }

            var a = 1;
            var b = new foo();

            a += 2;
            b.x += 2;
            b[""y""] += 3;

            bar.i += 1;
            bar.f += 2;
            bar.d += 3;

            bar.I += 4;
            bar.F += 5;
            bar.D += 6;
        ";

    private readonly BiteProgram program;

    public PropertyAccessBenchmarks()
    {
        var bar = new Bar();

        BiteCompiler compiler = new BiteCompiler();
        program = compiler.Compile( new[] { statements } );

    }

    [Benchmark]
    public void ArithmeticAssignment()
    {
        program.Run( new Dictionary < string, object >() { { "bar", new Bar() } } );


    }

}

}