using System.IO;
using Bite.Ast;
using Bite.Runtime;
using Bite.Runtime.Memory;

namespace UnitTests
{
    public class ExecResult
    {
        public BiteVmInterpretResult InterpretResult { get; set; }
        public DynamicBiteVariable LastValue { get; set; }
    }
}