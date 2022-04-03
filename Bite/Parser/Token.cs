using Bite.Ast;

namespace Bite.Parser
{

public class Token
{
    public int type;
    public string text;
    public DebugInfo DebugInfoToken = new DebugInfo();

    #region Public

    public Token()
    {
    }

    public Token( int type, string text )
    {
        this.type = type;
        this.text = text;
        DebugInfoToken.ColumnNumber = Lexer.CurrentColumnNumber;
        DebugInfoToken.LineNumber = Lexer.CurrentLineNumber;
    }

    public Token( int type, string text, int lineNumber, int columnNumber )
    {
        this.type = type;
        this.text = text;
        DebugInfoToken.ColumnNumber = columnNumber;
        DebugInfoToken.LineNumber = lineNumber;
    }

    public override string ToString()
    {
        return "Input: '" +
               text +
               "' Tokentype: " +
               BiteLexer.tokenNames[type > 0 ? type - 1 : type] +
               " Line: " +
               ( DebugInfoToken.LineNumber + 1 ) +
               " Column: " +
               DebugInfoToken.ColumnNumber +
               ">";
    }

    #endregion
}

}
