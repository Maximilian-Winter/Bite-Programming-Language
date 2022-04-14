using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Bite.Runtime.Memory
{

public static class DynamicBiteVariableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumeric( this DynamicBiteVariable variable )
    {
        return variable.DynamicType < DynamicVariableType.True;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsBoolean( this DynamicBiteVariable variable )
    {
        return variable.DynamicType == DynamicVariableType.True ||
               variable.DynamicType == DynamicVariableType.False;
    }

    }

    [StructLayout( LayoutKind.Explicit )]
public class DynamicBiteVariable
{
    [FieldOffset( 4 )]
    public DynamicVariableType DynamicType;

    [FieldOffset( 0 )]
    public double NumberData;

    [FieldOffset( 8 )]
    public string StringData;

    [FieldOffset( 8 )]
    public object[] ArrayData;

    [FieldOffset( 8 )]
    public object ObjectData;

    #region Public

    public DynamicBiteVariable()
    {
        DynamicType = DynamicVariableType.Null;
        NumberData = 0;
        StringData = null;
        ObjectData = null;
        ArrayData = null;
    }

    public DynamicBiteVariable( DynamicBiteVariable dynamicBiteVariable )
    {
        DynamicType = dynamicBiteVariable.DynamicType;
        NumberData = dynamicBiteVariable.NumberData;
        StringData = dynamicBiteVariable.StringData;
        ObjectData = dynamicBiteVariable.ObjectData;
        ArrayData = dynamicBiteVariable.ArrayData;
    }

    public DynamicBiteVariable( int value )
    {
        DynamicType = 0;
        NumberData = value;

        StringData = null;
        ObjectData = null;
        ArrayData = null;
    }

    public DynamicBiteVariable( double value )
    {
        DynamicType = 0;
        NumberData = value;

        StringData = null;
        ObjectData = null;
        ArrayData = null;
    }

    public DynamicBiteVariable( bool value )
    {
        NumberData = 0;
        StringData = null;
        ObjectData = null;
        ArrayData = null;

        if ( value )
        {
            DynamicType = DynamicVariableType.True;
        }
        else
        {
            DynamicType = DynamicVariableType.False;
        }
    }

    public DynamicBiteVariable( string value )
    {
        NumberData = 0;
        ObjectData = null;
        ArrayData = null;
        StringData = value;
        DynamicType = DynamicVariableType.String;
    }

    public DynamicBiteVariable( object value )
    {
        switch ( value )
        {
            case int i:
                DynamicType = 0;
                StringData = null;
                ObjectData = null;
                ArrayData = null;
                NumberData = i;

                break;

            case double d:
                DynamicType = 0;
                StringData = null;
                ObjectData = null;
                ArrayData = null;
                NumberData = d;

                break;

            case bool b when b:
                NumberData = 0;
                StringData = null;
                ObjectData = null;
                ArrayData = null;
                DynamicType = DynamicVariableType.True;

                break;

            case bool b:
                NumberData = 0;
                StringData = null;
                ObjectData = null;
                ArrayData = null;
                DynamicType = DynamicVariableType.False;

                break;

            case string s:
                NumberData = 0;
                ObjectData = null;
                ArrayData = null;
                StringData = s;
                DynamicType = DynamicVariableType.String;

                break;

            case object[] oa:
                NumberData = 0;
                StringData = null;
                ObjectData = null;
                ArrayData = oa;
                DynamicType = DynamicVariableType.Array;

                break;

            default:
                NumberData = 0;
                StringData = null;
                ArrayData = null;
                ObjectData = value;
                DynamicType = DynamicVariableType.Object;

                break;
        }
    }

    public DynamicBiteVariable( object[] value )
    {
        NumberData = 0;
        StringData = null;
        ObjectData = null;
        ArrayData = value;
        DynamicType = DynamicVariableType.Array;
    }

    public void Change( DynamicBiteVariable dynamicBiteVariable )
    {
        DynamicType = dynamicBiteVariable.DynamicType;
        NumberData = dynamicBiteVariable.NumberData;
        StringData = dynamicBiteVariable.StringData;
        ObjectData = dynamicBiteVariable.ObjectData;
        ArrayData = dynamicBiteVariable.ArrayData;
    }

    public new Type GetType()
    {
        if ( DynamicType == DynamicVariableType.Null )
        {
            return null;
        }

        if ( DynamicType == DynamicVariableType.True )
        {
            return typeof( bool );
        }

        if ( DynamicType == DynamicVariableType.False )
        {
            return typeof( bool );
        }

        if ( DynamicType == DynamicVariableType.String )
        {
            return typeof( string );
        }

        if ( DynamicType == DynamicVariableType.Array )
        {
            return ArrayData.GetType();
        }

        if ( DynamicType == DynamicVariableType.Object )
        {
            return ObjectData.GetType();
        }

        return typeof( double );
    }

    public object ToObject()
    {
        if ( DynamicType == DynamicVariableType.Null )
        {
            return null;
        }

        if ( DynamicType == DynamicVariableType.True )
        {
            return true;
        }

        if ( DynamicType == DynamicVariableType.False )
        {
            return false;
        }

        if ( DynamicType == DynamicVariableType.String )
        {
            return StringData;
        }

        if ( DynamicType == DynamicVariableType.Array )
        {
            return ArrayData;
        }

        if ( DynamicType == DynamicVariableType.Object )
        {
            return ObjectData;
        }

        return NumberData;
    }

    public override string ToString()
    {
        switch ( DynamicType )
        {
            case DynamicVariableType.Null:
                return "Null";

            case DynamicVariableType.True:
                return "True";

            case DynamicVariableType.False:
                return "False";

            case DynamicVariableType.String:
                return StringData;

            case DynamicVariableType.Array:
                return ArrayData.ToString();

            case DynamicVariableType.Object:
                return ObjectData.ToString();

            default:
                return NumberData.ToString();
        }
    }

    #endregion
}

}
