namespace Srsl_Parser
{

public class NoViableAltException : RecognitionException
{
    #region Public

    public NoViableAltException( string msg ) : base( msg )
    {
    }

    #endregion
}

}