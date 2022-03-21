using System.Collections.Generic;

namespace Srsl_Parser.Runtime
{

    public interface ISrslVmCallable
    {
        object Call(List<DynamicSrslVariable> arguments);
    }

}
