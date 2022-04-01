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

            commandLine.Parse<Options>(o =>
            {
                var files = Directory.EnumerateFiles(o.Path, "*.srsl", SearchOption.AllDirectories);

                var compiler = new Compiler(true);

                var program = compiler.Compile(o.MainModule, files.Select(File.ReadAllText));

            BiteParser parser = new BiteParser();
            BiteVm biteVm = new BiteVm();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            biteVm.Interpret(context);
            stopwatch.Stop();
        }
    }
}
