using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Bite.Ast;
using Bite.Runtime.Bytecode;

namespace Bite.Runtime
{
    public class Module
    {
        public bool MainModule { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<string> Imports { get; set; }
        public string Code { get; set; }
    }
}
