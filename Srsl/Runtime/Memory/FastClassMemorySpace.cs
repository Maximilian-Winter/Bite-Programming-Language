using Srsl.Runtime.Bytecode;

namespace Srsl.Runtime.Memory
{

    public class FastClassMemorySpace : FastMemorySpace
    {
        public FastClassMemorySpace(string name, FastMemorySpace enclosingSpace, int stackCount, BinaryChunk callerChunk, int callerInstructionPointer, int memberCount) : base(name, enclosingSpace, stackCount, callerChunk, callerInstructionPointer, memberCount)
        {
        }
    }

}
