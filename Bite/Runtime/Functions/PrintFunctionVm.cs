using System;
using System.Collections.Generic;
using Srsl.Runtime.Memory;

namespace Srsl.Runtime.Functions
{

    public class PrintFunctionVm : IBiteVmCallable
    {
        #region Public

        public object Call(List<DynamicBiteVariable> arguments)
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
