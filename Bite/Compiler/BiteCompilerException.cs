using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Compiler
{

public class BiteCompilerException : ApplicationException
{
    public string BiteCompilerExceptionMessage { get; }

    public IReadOnlyCollection < BiteCompilerSyntaxError > SyntaxErrors { get; }

    public override string Message
    {
        get
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append( BiteCompilerExceptionMessage );

            stringBuilder.AppendLine();

            foreach ( BiteCompilerSyntaxError syntaxError in SyntaxErrors )
            {
                stringBuilder.AppendLine( syntaxError.Message );
            }

            return stringBuilder.ToString();
        }
    }

    #region Public

    public BiteCompilerException( string message, IReadOnlyCollection < BiteCompilerSyntaxError > syntaxErrors ) :
        base( message )
    {
        BiteCompilerExceptionMessage = message;
        SyntaxErrors = syntaxErrors;
    }

    #endregion
}

}
