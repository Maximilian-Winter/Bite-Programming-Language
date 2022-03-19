using System;
using System.Runtime.InteropServices;

namespace Srsl_Parser.Runtime
{

[StructLayout(LayoutKind.Explicit)]
public struct ConstantValue
{
    [FieldOffset(0)] public ConstantValueType ConstantType;
    [FieldOffset(1)] public bool BoolConstantValue;
    [FieldOffset(1)] public int IntegerConstantValue;
    [FieldOffset(1)] public double DoubleConstantValue;
    [FieldOffset(16)] public string StringConstantValue;

    public ConstantValue(int value):this()
    {
        IntegerConstantValue = value;
        ConstantType = ConstantValueType.Integer;
    }
    
    public ConstantValue(double value):this()
    {
        DoubleConstantValue = value;
        ConstantType = ConstantValueType.Double;
    }
    
    public ConstantValue(string value):this()
    {
        StringConstantValue = value;
        ConstantType = ConstantValueType.String;
    }
    
    public ConstantValue(bool value):this()
    {
        BoolConstantValue = value;
        ConstantType = ConstantValueType.Bool;
    }
    
    public override string ToString()
    {
        switch ( ConstantType )
        {
            case ConstantValueType.Integer:
                return $"{ConstantType.ToString()}: {IntegerConstantValue.ToString()}";

            case ConstantValueType.Double:
                return $"{ConstantType.ToString()}: {DoubleConstantValue.ToString()}";

            case ConstantValueType.String:
                return $"{ConstantType.ToString()}: {StringConstantValue}";
            
            case ConstantValueType.Bool:
                return $"{ConstantType.ToString()}: {BoolConstantValue.ToString()}";

            default:
                throw new ArgumentOutOfRangeException();
        }
       
    }
}

}
