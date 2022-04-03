using System;

namespace Bite.Parser
{

public abstract class RecognitionException : Exception
{
    public int Line { get; }

    public int Column { get; }

    public override string Message => $"{base.Message}";

    #region Public

    public RecognitionException()
    {
    }

    public RecognitionException( string msg ) : base( msg )
    {
    }

    public RecognitionException( string msg, Token token ) : base( msg )
    {
        Line = token.DebugInfoToken.LineNumber;
        Column = token.DebugInfoToken.ColumnNumber;
    }

    #endregion
}

}
