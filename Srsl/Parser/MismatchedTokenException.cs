namespace Srsl.Parser
{

    public class MismatchedTokenException : RecognitionException
    {
        #region Public

        public MismatchedTokenException(string msg, Token token) : base(msg, token)
        {
        }

        #endregion
    }

}