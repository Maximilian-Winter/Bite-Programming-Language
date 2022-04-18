using System.Collections.Generic;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions
{

public interface IBiteVmCallable
{
    object Call( DynamicBiteVariable[] arguments );
}

}
