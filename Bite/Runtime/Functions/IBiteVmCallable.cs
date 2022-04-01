using System.Collections.Generic;
using Srsl.Runtime.Memory;

namespace Srsl.Runtime.Functions
{

    public interface IBiteVmCallable
    {
        object Call(List<DynamicBiteVariable> arguments);
    }

}
