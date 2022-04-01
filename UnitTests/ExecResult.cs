using System.IO;
using Srsl.Ast;
using Srsl.Runtime;
using Srsl.Runtime.Memory;

namespace UnitTests
{
    public class ExecResult
    {
        public BiteVmInterpretResult InterpretResult { get; set; }
        public DynamicBiteVariable LastValue { get; set; }
    }
}