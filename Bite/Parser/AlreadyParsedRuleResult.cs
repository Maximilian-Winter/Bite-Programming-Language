using System;

namespace Srsl.Parser
{

    public class AlreadyParsedFailedException : RecognitionException
    {
        public AlreadyParsedFailedException() : base()
        {
        }
    }

    public struct AlreadyParsedRuleResult
    {
        public bool Result { get; private set; }
        public bool Failed { get; private set; }

        public static AlreadyParsedRuleResult FromResult(bool result)
        {
            return new AlreadyParsedRuleResult() { Result = result };
        }

        public static AlreadyParsedRuleResult AsFailed()
        {
            return new AlreadyParsedRuleResult() { Failed = true };
        }
    }
}