using Force.DeepCloner;

namespace Srsl_Parser.Runtime
{

public static class DynamicVariableExtension
{
    public static DynamicSrslVariable ToDynamicVariable()
    {
        DynamicSrslVariable srslVariable = new DynamicSrslVariable();
        srslVariable.DynamicType = DynamicVariableType.Null;
        srslVariable.NumberData = 0;
        srslVariable.ObjectData = null;
        return srslVariable;
        
    }
    
    public static DynamicSrslVariable ToDynamicVariable(int data)
    {
        DynamicSrslVariable srslVariable = new DynamicSrslVariable();
        srslVariable.DynamicType = 0;
        srslVariable.NumberData = data;
        srslVariable.ObjectData = null;

        return srslVariable;
    }
    
    public static DynamicSrslVariable ToDynamicVariable(double data)
    {
        DynamicSrslVariable srslVariable = new DynamicSrslVariable();
        srslVariable.DynamicType = 0;
        srslVariable.NumberData = data;
        srslVariable.ObjectData = null;
        return srslVariable;
    }
    
    public static DynamicSrslVariable ToDynamicVariable(bool data)
    {
        DynamicSrslVariable srslVariable = new DynamicSrslVariable();
        srslVariable.NumberData = 0;
        srslVariable.ObjectData = null;

        if ( data )
        {
            srslVariable.DynamicType = DynamicVariableType.True;
        }
        else
        {
            srslVariable.DynamicType = DynamicVariableType.False;
        }
        return srslVariable;
    }
    
    public static DynamicSrslVariable ToDynamicVariable(string data)
    {
        DynamicSrslVariable srslVariable = new DynamicSrslVariable();
        srslVariable.NumberData = 0;
        srslVariable.ObjectData = null;
        srslVariable.ArrayData = null;
        srslVariable.StringData = data;
        srslVariable.DynamicType = DynamicVariableType.String;
        
        return srslVariable;
    }
    
    public static DynamicSrslVariable ToDynamicVariable(object data)
    {
        DynamicSrslVariable srslVariable = new DynamicSrslVariable();
        switch ( data )
        {
            case int i:
                srslVariable.DynamicType = 0;
                srslVariable.StringData = null;
                srslVariable.ObjectData = null;
                srslVariable.ArrayData = null;
                srslVariable.NumberData = i;
               
                break;

            case double d:
                srslVariable.DynamicType = 0;
                srslVariable.StringData = null;
                srslVariable.ObjectData = null;
                srslVariable.ArrayData = null;
                srslVariable.NumberData = d;

                break;

            case bool b when b:
                srslVariable.NumberData = 0;
                srslVariable.StringData = null;
                srslVariable.ObjectData = null;
                srslVariable.ArrayData = null;
                srslVariable.DynamicType = DynamicVariableType.True;

                break;

            case bool b:
                srslVariable.NumberData = 0;
                srslVariable.StringData = null;
                srslVariable.ObjectData = null;
                srslVariable.ArrayData = null;
                srslVariable.DynamicType = DynamicVariableType.False;

                break;

            case string s:
                srslVariable.NumberData = 0;
                srslVariable.ObjectData = null;
                srslVariable.ArrayData = null;
                srslVariable.StringData = s;
                srslVariable.DynamicType = DynamicVariableType.String;
                
                break;

            case object[] oa:
                srslVariable.NumberData = 0;
                srslVariable.StringData = null;
                srslVariable.ObjectData = null;
                srslVariable.ArrayData = oa;
                srslVariable.DynamicType = DynamicVariableType.Array;

                break;
            default:
                srslVariable.NumberData = 0;
                srslVariable.StringData = null;
                srslVariable.ArrayData = null;
                srslVariable.ObjectData = data.ShallowClone();
                srslVariable.DynamicType = DynamicVariableType.Object;

                break;
        }
        return srslVariable;
    }
    
    public static DynamicSrslVariable ToDynamicVariable(object[] data)
    {
        DynamicSrslVariable srslVariable = new DynamicSrslVariable();
        srslVariable.NumberData = 0;
        srslVariable.StringData = null;
        srslVariable.ObjectData = null;
        srslVariable.ArrayData = data;
        srslVariable.DynamicType = DynamicVariableType.Array;
        return srslVariable;
    }
}

}
