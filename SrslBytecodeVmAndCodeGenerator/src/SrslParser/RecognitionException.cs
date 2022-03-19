using System;

namespace Srsl_Parser
{

public abstract class RecognitionException : Exception
{
    #region Public

    public RecognitionException( string msg ) : base( msg )
    {
    }

    #endregion
}

}