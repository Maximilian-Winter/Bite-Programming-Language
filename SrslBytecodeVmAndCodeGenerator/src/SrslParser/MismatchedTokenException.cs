namespace Srsl_Parser
{

public class MismatchedTokenException : RecognitionException
{
    #region Public

    public MismatchedTokenException( string msg ) : base( msg )
    {
    }

    #endregion
}

}