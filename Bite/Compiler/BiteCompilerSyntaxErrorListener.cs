using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace Bite.Compiler
{

public class BiteCompilerSyntaxErrorListener : BaseErrorListener
{
    public readonly List<BiteCompilerSyntaxError> Errors = new List<BiteCompilerSyntaxError>();

    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e )
    {
        Errors.Add(new BiteCompilerSyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e));
    }
}

}
