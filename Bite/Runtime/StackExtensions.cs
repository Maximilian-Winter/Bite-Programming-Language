using System;
using Bite.Runtime.Memory;

namespace Bite.Runtime
{

public static class StackExtensions
{
    #region Public

    /// <summary>
    ///     Retrieves the current value on the stack based on the specified type
    /// </summary>
    /// <param name="vmStack"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object PopDataByType( this DynamicBiteVariableStack vmStack, Type type )
    {
        object data;

        DynamicBiteVariable currentStack = vmStack.Peek();

        // Cast the data based on the recieving propertyType
        if ( type == typeof( double ) && currentStack.IsNumeric() )
        {
            data = vmStack.Pop().NumberData;
        }
        else if ( type == typeof( float ) && currentStack.IsNumeric() )
        {
            data = ( float ) vmStack.Pop().NumberData;
        }
        else if ( type == typeof( int ) && currentStack.IsNumeric() )
        {
            data = ( int ) vmStack.Pop().NumberData;
        }
        else if ( type == typeof( string ) && currentStack.DynamicType == DynamicVariableType.String )
        {
            data = vmStack.Pop().StringData;
        }
        else if ( type == typeof( bool ) && currentStack.IsBoolean() )
        {
            data = currentStack.DynamicType == DynamicVariableType.True;
        }
        else
        {
            data = vmStack.Pop().ObjectData;
        }

        return data;
    }

    #endregion
}

}
