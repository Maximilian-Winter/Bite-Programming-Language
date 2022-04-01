using System;
using System.Diagnostics;
using Srsl.Parser;
using Srsl.Runtime;
using System.IO;
using System.Linq;
using Srsl.Runtime.CodeGen;

namespace Srsl.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var commandLine = new CommandLineArgs(args);

            var options = commandLine.Parse<Options>();

            //var options = new Options()
            //{
            //    MainModule = "MainModule",
            //    Path = ".\\TestProgram",
            //};

            var files = Directory.EnumerateFiles(options.Path, "*.srsl", SearchOption.AllDirectories);

            BiteParser parser = new BiteParser();
            var program = parser.ParseModules(options.MainModule, files.Select(File.ReadAllText));
            CodeGenerator generator = new CodeGenerator();
            var context = generator.CompileProgram(program);
            BiteVm biteVm = new BiteVm();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            biteVm.Interpret(context);
            stopwatch.Stop();
        }
    }
}
