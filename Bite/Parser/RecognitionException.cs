using System;

namespace Bite.Parser
{

    public abstract class RecognitionException : Exception
    {
        #region Public
        public int Line { get; }
        public int Column { get; }

        public RecognitionException() 
        {
        }

        public RecognitionException(string msg) : base(msg)
        {
        }

        public RecognitionException(string msg, Token token) : base(msg)
        {
            Line = token.DebugInfoToken.LineNumber;
            Column = token.DebugInfoToken.ColumnNumber;
        }

        public override string Message
        {
            get
            {
                return $"{base.Message}";
            }
        }

        #endregion
    }

}