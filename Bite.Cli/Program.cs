using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bite.Parser;
using Bite.Runtime;
using Bite.Runtime.CodeGen;

namespace Bite.Cli
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
