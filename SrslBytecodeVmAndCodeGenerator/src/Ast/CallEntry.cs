using System.Collections.Generic;

namespace MemoizeSharp
{

public class CallEntry
{
    public PrimaryNode Primary;
    public ArgumentsNode Arguments;
    public List < CallElementEntry > ElementAccess;
    public bool IsFunctionCall;
}

}
