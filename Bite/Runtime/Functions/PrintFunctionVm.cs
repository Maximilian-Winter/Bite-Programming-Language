using System;
using System.Collections.Generic;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions
{

public class PrintFunctionVm : IBiteVmCallable
{
    #region Public

    public object Call( List < DynamicBiteVariable > arguments )
    {
        if ( arguments[0].DynamicType != DynamicVariableType.Null )
        {
            Console.WriteLine( arguments[0].ToString() );
        }
        else
        {
            Console.WriteLine( "Error: Passed Null Reference to Function!" );
        }

        return null;
    }

    #endregion
}

}
