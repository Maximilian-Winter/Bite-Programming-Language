using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Srsl.Parser;
using Srsl.Runtime;
using Srsl.Runtime.Bytecode;
using Srsl.Runtime.CodeGen;

namespace TestAppNet6
{
    public class Program
    {
        public static void TestExpression()
        {
            SrslParser parser = new SrslParser();

            var expression = parser.ParseExpression("1 + 1");

            CodeGenerator generator = new CodeGenerator();

            var context = generator.CompileExpression(expression);

            SrslVm srslVm = new SrslVm();

            var result = srslVm.Interpret(context);

            Console.WriteLine(srslVm.RetVal.NumberData);
        }

        public static void TestStatements()
        {
            SrslParser parser = new SrslParser();

            var statements = parser.ParseStatements("1 + 1;");

            CodeGenerator generator = new CodeGenerator();

            var context = generator.CompileStatements(statements);

            SrslVm srslVm = new SrslVm();

            var result = srslVm.Interpret(context);

            Console.WriteLine(srslVm.RetVal.NumberData);
        }

        public static void Main(string[] args)
        {

            TestExpression();

            TestStatements();

            SrslParser parser = new SrslParser();

            var files = Directory.EnumerateFiles(".\\TestProgram", "*.srsl", SearchOption.AllDirectories);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var program = parser.ParseModules("MainModule", files.Select(File.ReadAllText));
            stopwatch.Stop();
            Console.WriteLine($"Parsing completed in {stopwatch.ElapsedMilliseconds}ms");

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

                Console.WriteLine(srslVm.RetVal.ToString());

                stopwatch2.Stop();
                Console.WriteLine("--Elapsed Time for Interpreting Run {0} is {1} ms", i, stopwatch2.ElapsedMilliseconds);
                elapsedMillisecondsAccu += stopwatch2.ElapsedMilliseconds;


            }
            Console.WriteLine("--Average Elapsed Time for Interpreting per Run is {0} ms", elapsedMillisecondsAccu / k);
            Console.WriteLine("--Total Elapsed Time for Interpreting {0} Runs is {1} ms", k, elapsedMillisecondsAccu);
            var sortedDict = from entry in ChunkDebugHelper.InstructionCounter orderby entry.Value descending select entry;
            long totalInstructions = 0;
            foreach (KeyValuePair<string, long> keyValuePair in sortedDict)
            {
                totalInstructions += keyValuePair.Value;
            }

            foreach (KeyValuePair<string, long> keyValuePair in sortedDict)
            {
                Console.WriteLine("--Instruction Count for Instruction {0}: {2}     {1}%", keyValuePair.Key, (100.0 / totalInstructions * keyValuePair.Value).ToString("00.0"), keyValuePair.Value);
            }

            ChunkDebugHelper.InstructionCounter.Clear();

            Console.ReadLine();

        }
    }
}
