namespace Bite.Cli
{
    public class CommandLineArgs
    {
        public CommandLineArgs(string[] args)
        {

        }

        public T Parse<T>() where T : new()
        {
            var options = new T();

            return options;
        }
    }
}

