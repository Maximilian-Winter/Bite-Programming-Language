using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Compiler
{

public class BiteCompilerException : ApplicationException
{
    public BiteCompilerException( string message, IReadOnlyCollection < BiteCompilerSyntaxError > syntaxErrors ) :
        base( message )
    {
        BiteCompilerExceptionMessage = message;
        SyntaxErrors = syntaxErrors;
    }

    public string BiteCompilerExceptionMessage { get; }

    public IReadOnlyCollection < BiteCompilerSyntaxError > SyntaxErrors { get; }

    public override string Message
    {
        get
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append( BiteCompilerExceptionMessage );

            stringBuilder.AppendLine();

            foreach ( var syntaxError in SyntaxErrors )
            {
                stringBuilder.AppendLine( syntaxError.Message );
            }

            return stringBuilder.ToString();
        }
    }
}

}
