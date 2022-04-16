namespace Bite.Runtime.Memory
{

public static class DynamicVariableExtension
{
    #region Public

    public static DynamicBiteVariable ToDynamicVariable()
    {
        DynamicBiteVariable biteVariable = new DynamicBiteVariable();
        biteVariable.DynamicType = DynamicVariableType.Null;
        biteVariable.NumberData = 0;
        biteVariable.ObjectData = null;

        return biteVariable;
    }

    public static DynamicBiteVariable ToDynamicVariable( int data )
    {
        DynamicBiteVariable biteVariable = new DynamicBiteVariable();
        biteVariable.DynamicType = 0;
        biteVariable.NumberData = data;
        biteVariable.ObjectData = null;

        return biteVariable;
    }

    public static DynamicBiteVariable ToDynamicVariable( double data )
    {
        DynamicBiteVariable biteVariable = new DynamicBiteVariable();
        biteVariable.DynamicType = 0;
        biteVariable.NumberData = data;
        biteVariable.ObjectData = null;

        return biteVariable;
    }

    public static DynamicBiteVariable ToDynamicVariable( bool data )
    {
        DynamicBiteVariable biteVariable = new DynamicBiteVariable();
        biteVariable.NumberData = 0;
        biteVariable.ObjectData = null;

        if ( data )
        {
            biteVariable.DynamicType = DynamicVariableType.True;
        }
        else
        {
            biteVariable.DynamicType = DynamicVariableType.False;
        }

        return biteVariable;
    }

    public static DynamicBiteVariable ToDynamicVariable( string data )
    {
        DynamicBiteVariable biteVariable = new DynamicBiteVariable();
        biteVariable.NumberData = 0;
        biteVariable.ObjectData = null;
        biteVariable.ArrayData = null;
        biteVariable.StringData = data;
        biteVariable.DynamicType = DynamicVariableType.String;

        return biteVariable;
    }

    public static DynamicBiteVariable ToDynamicVariable( object data )
    {
        DynamicBiteVariable biteVariable = new DynamicBiteVariable();

        switch ( data )
        {
            case int i:
                biteVariable.DynamicType = 0;
                biteVariable.StringData = null;
                biteVariable.ObjectData = null;
                biteVariable.ArrayData = null;
                biteVariable.NumberData = i;

                break;

            case double d:
                biteVariable.DynamicType = 0;
                biteVariable.StringData = null;
                biteVariable.ObjectData = null;
                biteVariable.ArrayData = null;
                biteVariable.NumberData = d;

                break;

            case float f:
                biteVariable.DynamicType = 0;
                biteVariable.StringData = null;
                biteVariable.ObjectData = null;
                biteVariable.ArrayData = null;
                biteVariable.NumberData = f;

                break;

            case bool b when b:
                biteVariable.NumberData = 0;
                biteVariable.StringData = null;
                biteVariable.ObjectData = null;
                biteVariable.ArrayData = null;
                biteVariable.DynamicType = DynamicVariableType.True;

                break;

            case bool b:
                biteVariable.NumberData = 0;
                biteVariable.StringData = null;
                biteVariable.ObjectData = null;
                biteVariable.ArrayData = null;
                biteVariable.DynamicType = DynamicVariableType.False;

                break;

            case string s:
                biteVariable.NumberData = 0;
                biteVariable.ObjectData = null;
                biteVariable.ArrayData = null;
                biteVariable.StringData = s;
                biteVariable.DynamicType = DynamicVariableType.String;

                break;

            case object[] oa:
                biteVariable.NumberData = 0;
                biteVariable.StringData = null;
                biteVariable.ObjectData = null;
                biteVariable.ArrayData = oa;
                biteVariable.DynamicType = DynamicVariableType.Array;

                break;

            case FastMemorySpace fastMemorySpace:
                biteVariable.NumberData = 0;
                biteVariable.StringData = null;
                biteVariable.ArrayData = null;
                biteVariable.ObjectData = fastMemorySpace;
                biteVariable.DynamicType = DynamicVariableType.Object;

                break;

            case DynamicBiteVariable dynamicBiteVariable:
                switch ( dynamicBiteVariable.DynamicType )
                {
                    case DynamicVariableType.Null:
                        biteVariable.DynamicType = DynamicVariableType.Null;

                        break;

                    case DynamicVariableType.True:
                        biteVariable.DynamicType = DynamicVariableType.True;

                        break;

                    case DynamicVariableType.False:
                        biteVariable.DynamicType = DynamicVariableType.False;

                        break;

                    case DynamicVariableType.String:
                        biteVariable.StringData = dynamicBiteVariable.StringData;
                        biteVariable.DynamicType = DynamicVariableType.String;

                        break;

                    case DynamicVariableType.Array:
                        biteVariable.ArrayData = dynamicBiteVariable.ArrayData;
                        biteVariable.DynamicType = DynamicVariableType.Array;

                        break;

                    case DynamicVariableType.Object:
                        biteVariable.ObjectData = dynamicBiteVariable.ObjectData;
                        biteVariable.DynamicType = DynamicVariableType.Object;

                        break;

                    default:
                        biteVariable.DynamicType = 0;
                        biteVariable.NumberData = dynamicBiteVariable.NumberData;

                        break;
                }

                break;

            default:
                biteVariable.NumberData = 0;
                biteVariable.StringData = null;
                biteVariable.ArrayData = null;
                biteVariable.ObjectData = data;
                biteVariable.DynamicType = DynamicVariableType.Object;

                break;
        }

        return biteVariable;
    }

    public static DynamicBiteVariable ToDynamicVariable( object[] data )
    {
        DynamicBiteVariable biteVariable = new DynamicBiteVariable();
        biteVariable.NumberData = 0;
        biteVariable.StringData = null;
        biteVariable.ObjectData = null;
        biteVariable.ArrayData = data;
        biteVariable.DynamicType = DynamicVariableType.Array;

        return biteVariable;
    }

    #endregion
}

}
