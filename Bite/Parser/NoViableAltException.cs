namespace Bite.Parser
{

    public class NoViableAltException : RecognitionException
    {
        #region Public

        //public NoViableAltException(string msg) : base(msg)
        //{
        //}

        public Token Token { get; }

        public NoViableAltException(string msg, Token token) : base(msg, token)
        {
            Token = token;
        }

        public override string Message
        {
            get
            {
                return $"{base.Message} {Token}";
            }
        }

        #endregion
    }

}