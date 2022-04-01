using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Srsl.Ast;
using Srsl.Parser;
using Srsl.Runtime;
using Srsl.Runtime.Bytecode;
using Srsl.Runtime.CodeGen;

namespace TestApp
{

    public class Program
    {
        #region Public

        public static void Main(string[] args)
        {
            BiteParser parser = new BiteParser();

            var files = Directory.EnumerateFiles(".\\TestProgram", "*.srsl", SearchOption.AllDirectories);

            var program = parser.ParseModules("MainModule", files.Select<string, Func<string>>(f =>
            {
                return () => File.ReadAllText(f);
            }));

           //var program = parser.ParseModules("MainModule", files.Select(File.ReadAllText));
            
            CodeGenerator generator = new CodeGenerator();

            var context= generator.CompileProgram(program);

            BiteVm biteVm = new BiteVm();

            int k = 3;
            long elapsedMillisecondsAccu = 0;
            for (int i = 0; i < k; i++)
            {
                Stopwatch stopwatch2 = new Stopwatch();
                stopwatch2.Start();
                biteVm.Interpret(context);
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

        #endregion
    }

}
