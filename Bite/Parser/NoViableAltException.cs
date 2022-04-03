namespace Bite.Parser
{

public class NoViableAltException : RecognitionException
{
    //public NoViableAltException(string msg) : base(msg)
    //{
    //}

    public Token Token { get; }

    public override string Message => $"{base.Message} {Token}";

    #region Public

    public NoViableAltException( string msg, Token token ) : base( msg, token )
    {
        Token = token;
    }

    #endregion
}

}
