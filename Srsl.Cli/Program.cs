using System;
using System.Diagnostics;
using Srsl.Parser;
using Srsl.Runtime;
using Srsl.Runtime.CodeGenerator;
using System.IO;
using System.Linq;

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

            SrslParser parser = new SrslParser();
            var program = parser.ParseModules(options.MainModule, files.Select(File.ReadAllText));
            CodeGenerator generator = new CodeGenerator();
            var context = generator.CompileProgram(program);
            SrslVm srslVm = new SrslVm();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            srslVm.Interpret(context);
            stopwatch.Stop();
        }
    }
}
