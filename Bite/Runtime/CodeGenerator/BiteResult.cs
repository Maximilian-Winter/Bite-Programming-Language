using Bite.Runtime.Memory;

namespace Bite.Runtime.CodeGen
{
    public class BiteResult
    {
        public DynamicBiteVariable ReturnValue { get; set; }
        public BiteVmInterpretResult InterpretResult { get; set; }
    }
}