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
        if ( arguments[0].DynamicType != DynamicVariableType.Null )
        {
            int arraySize = arguments.Count;

            for ( int i = 0; i < arraySize; i++ )
            {
                Console.WriteLine( arguments[i].ToString() );
            }
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
