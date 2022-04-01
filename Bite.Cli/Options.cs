namespace Srsl.Cli
{
    public class Options
    {
        [Option('m', "main", true)]
        public string MainModule { get; set; }
        [Option('p', "path", ".")]
        public string Path { get; set; }
    }
}

