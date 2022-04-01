using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Srsl.Ast;
using Srsl.Runtime.Bytecode;

namespace Srsl.Runtime
{
    public class Module
    {
        public bool MainModule { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<string> Imports { get; set; }
        public string Code { get; set; }
    }
}
