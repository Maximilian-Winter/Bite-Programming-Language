using System.Collections.Generic;

namespace Bite.Runtime
{

public class Module
{
    public bool MainModule { get; set; }

    public string Name { get; set; }

    public IReadOnlyCollection < string > Imports { get; set; }

    public string Code { get; set; }
}

}
