using Bite.Cli.CommandLine;

namespace Bite.Cli
{

public class Options
{
    [Option( 'm', "main", true, "<MainModule>", "The entry point of the program" )]
    public string MainModule { get; set; }

    [Option( 'p', "path", ".", "<path\\to\\modules>", "The path containing the modules to be loaded" )]
    public string Path { get; set; }

    [Option( 'i', "input", false, "<module1.bite> [module2.bite] ...", "A list of modules to be loaded" )]
    public string[] Modules { get; set; }
}

}
