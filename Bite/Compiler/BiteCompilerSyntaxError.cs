using Antlr4.Runtime;

namespace Bite.Compiler
{

public readonly struct BiteCompilerSyntaxError
{
    public readonly IRecognizer Recognizer;
    public readonly IToken OffendingSymbol;
    public readonly int Line;
    public readonly int CharPositionInLine;
    public readonly string Message;
    public readonly RecognitionException Exception;

    public BiteCompilerSyntaxError(
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string message,
        RecognitionException exception )
    {
        Recognizer = recognizer;
        OffendingSymbol = offendingSymbol;
        Line = line;
        CharPositionInLine = charPositionInLine;
        Message = message;
        Exception = exception;
    }
}

}
