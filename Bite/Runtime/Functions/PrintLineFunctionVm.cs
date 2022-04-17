using System;
using System.Collections.Generic;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions
{

public class PrintLineFunctionVm : IBiteVmCallable
{
    #region Public

    public object Call( List < DynamicBiteVariable > arguments )
    {
        int argumentsCount = arguments.Count;

        for ( int i = 0; i < argumentsCount; i++ )
        {
            Console.WriteLine( arguments[i].ToString() );
        }

        return null;
    }

    #endregion
}

}
