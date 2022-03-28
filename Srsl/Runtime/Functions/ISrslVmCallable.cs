using System.Collections.Generic;
using Srsl.Runtime.Memory;

namespace Srsl.Runtime.Functions
{

    public interface ISrslVmCallable
    {
        object Call(List<DynamicSrslVariable> arguments);
    }

}
