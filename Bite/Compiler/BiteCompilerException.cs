using System;
using System.Collections.Generic;

namespace Bite.Runtime
{

public class BiteCompilerException : ApplicationException
{
    public BiteCompilerException(string message, List<BiteCompilerSyntaxError> syntaxErrors): base(message)
    {
        BiteCompilerExceptionMessage = message;
        SyntaxErrors = syntaxErrors;
    }

    public string BiteCompilerExceptionMessage { get; }

    public List<BiteCompilerSyntaxError> SyntaxErrors{ get; }
}

}
