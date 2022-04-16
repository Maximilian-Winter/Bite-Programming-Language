using Bite.Runtime.Memory;

namespace Bite.Runtime
{

public class BiteFunctionCall
{
    public string FunctionName;
    public BiteChunkWrapper BiteChunkWrapper = null;
    public DynamicBiteVariable[] FunctionArguments;

    #region Public

    public BiteFunctionCall(
        string functionName,
        DynamicBiteVariable[] functionArguments )
    {
        FunctionName = functionName;
        FunctionArguments = functionArguments;
    }

    public BiteFunctionCall(
        BiteChunkWrapper fWrapper,
        DynamicBiteVariable[] functionArguments )
    {
        BiteChunkWrapper = fWrapper;
        FunctionArguments = functionArguments;
    }

    #endregion
}

}
