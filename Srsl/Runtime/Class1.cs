using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Srsl.Parser;
using Srsl.Runtime.CodeGen;

namespace Srsl.Runtime
{
    internal class Class1
    {
        public void Compile()
        {

            var files = Directory.EnumerateFiles(".\\TestProgram", "*.srsl", SearchOption.AllDirectories);

            SrslParser parser = new SrslParser();

            var program = parser.ParseModules("MainModule", files.Select(File.ReadAllText));
            
            CodeGenerator generator = new CodeGenerator();

            var context = generator.CompileProgram(program);

            SrslVm srslVm = new SrslVm();

            int k = 5;
            long elapsedMillisecondsAccu = 0;
            for (int i = 0; i < k; i++)
            {
                Stopwatch stopwatch2 = new Stopwatch();
                stopwatch2.Start();
                srslVm.Interpret(context);
                stopwatch2.Stop();
                Console.WriteLine("--Elapsed Time for Interpreting Run {0} is {1} ms", i, stopwatch2.ElapsedMilliseconds);
                elapsedMillisecondsAccu += stopwatch2.ElapsedMilliseconds;


            }
        }
    }
}
