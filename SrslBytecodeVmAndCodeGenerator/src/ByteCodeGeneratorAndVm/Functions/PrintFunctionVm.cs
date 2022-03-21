using System;
using System.Collections.Generic;

namespace Srsl_Parser.Runtime
{

    public class PrintFunctionVm : ISrslVmCallable
    {
        #region Public

        public object Call(List<DynamicSrslVariable> arguments)
        {
            if (arguments[0].DynamicType != DynamicVariableType.Null)
            {
                Console.WriteLine(arguments[0].ToString());
            }
            else
            {
                System.Console.WriteLine("Error: Passed Null Reference to Function!");
            }

            return null;
        }

        #endregion
    }

}
