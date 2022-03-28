using System.Collections.Generic;

namespace Srsl.Ast
{

    public class CallEntry
    {
        public PrimaryNode Primary;
        public ArgumentsNode Arguments;
        public List<CallElementEntry> ElementAccess;
        public bool IsFunctionCall;
    }

}
