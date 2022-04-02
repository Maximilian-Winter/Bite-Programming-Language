using System.IO;
using System.Linq;

using Bite.Cli.CommandLine;
using Bite.Runtime;



namespace Bite.Cli
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

                program.Run();
            });
        }
    }
}
