namespace Srsl_Parser.Runtime
{

    public class FastModuleMemorySpace : FastMemorySpace
    {
        public FastModuleMemorySpace(string name, FastMemorySpace enclosingSpace, int stackCount, BinaryChunk callerChunk, int callerInstructionPointer, int memberCount) : base(name, enclosingSpace, stackCount, callerChunk, callerInstructionPointer, memberCount)
        {
        }
    }

}
