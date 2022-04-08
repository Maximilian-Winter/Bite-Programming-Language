namespace Bite.Runtime.Memory
{

public enum DynamicVariableType : uint
{
    Null = 0xfff80001, // Use NaN boxing
    True = 0xfff80002,
    False = 0xfff80003,
    String = 0xfff80004,
    Array = 0xfff80005,
    Object = 0xfff80006
}

}
