using System.IO;
using Srsl.Ast;
using Srsl.Runtime;
using Srsl.Runtime.Memory;

namespace UnitTests
{
    public class ExecResult
    {
        public SrslVmInterpretResult InterpretResult { get; set; }
        public DynamicSrslVariable LastValue { get; set; }
    }
}