using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Bite.Parser
{

public class BiteLexer : Lexer
{
    public static readonly string[] tokenNames =
    {
        "DeclareModule",
        "DeclareClass",
        "DeclareStruct",
        "DeclareClassInstance",
        "DeclareFunction",
        "DeclareVariable",
        "DeclareGetter",
        "DeclareSetter",
        "DeclareForLoop",
        "DeclareWhileLoop",
        "DeclareStatic",
        "DeclareAbstract",
        "DeclarePublic",
        "DeclarePrivate",
        "ControlFlowIf",
        "ControlFlowElse",
        "FunctionReturn",
        "Break",
        "NullReference",
        "ThisReference",
        "UsingDirective",
        "ImportDirective",
        "AssignOperator",
        "PlusAssignOperator",
        "MinusAssignOperator",
        "MultiplyAssignOperator",
        "DivideAssignOperator",
        "ModuloAssignOperator",
        "BitwiseAndAssignOperator",
        "BitwiseOrAssignOperator",
        "BitwiseXorAssignOperator",
        "BitwiseLeftShiftAssignOperator",
        "BitwiseRightShiftAssignOperator",
        "LogicalOrOperator",
        "LogicalAndOperator",
        "UnequalOperator",
        "EqualOperator",
        "GreaterOperator",
        "ShiftRightOperator",
        "GreaterEqualOperator",
        "SmallerOperator",
        "ShiftLeftOperator",
        "SmallerEqualOperator",
        "MinusOperator",
        "MinusMinusOperator",
        "PlusOperator",
        "PlusPlusOperator",
        "DivideOperator",
        "MultiplyOperator",
        "LogicalNegationOperator",
        "DotOperator",
        "QuestionMarkOperator",
        "ColonOperator",
        "ReferenceOperator",
        "ModuloOperator",
        "ComplimentOperator",
        "BitwiseAndOperator",
        "BitwiseXorOperator",
        "BitwiseOrOperator",
        "OpeningRoundBracket",
        "ClosingRoundBracket",
        "OpeningCurlyBracket",
        "ClosingCurlyBracket",
        "SquarebracketLeft",
        "SquarebracketRight",
        "CommaSeperator",
        "SemicolonSeperator",
        "BooleanLiteral",
        "False_",
        "True_",
        "IntegerLiteral",
        "FloatingLiteral",
        "StringLiteral",
        "UnterminatedStringLiteral",
        "DecimalLiteral",
        "Identifier",
        "COMMENT",
        "WHITESPACE",
        "LINE_COMMENT"
    };

    public const int
        DeclareModule = 1,
        DeclareClass = 2,
        DeclareStruct = 3,
        DeclareClassInstance = 4,
        DeclareFunction = 5,
        DeclareVariable = 6,
        DeclareGetter = 7,
        DeclareSetter = 8,
        DeclareForLoop = 9,
        DeclareWhileLoop = 10,
        DeclareStatic = 11,
        DeclareAbstract = 12,
        DeclarePublic = 13,
        DeclarePrivate = 14,
        ControlFlowIf = 15,
        ControlFlowElse = 16,
        FunctionReturn = 17,
        Break = 18,
        NullReference = 19,
        ThisReference = 20,
        UsingDirective = 21,
        ImportDirective = 22,
        AssignOperator = 23,
        PlusAssignOperator = 24,
        MinusAssignOperator = 25,
        MultiplyAssignOperator = 26,
        DivideAssignOperator = 27,
        ModuloAssignOperator = 28,
        BitwiseAndAssignOperator = 29,
        BitwiseOrAssignOperator = 30,
        BitwiseXorAssignOperator = 31,
        BitwiseLeftShiftAssignOperator = 32,
        BitwiseRightShiftAssignOperator = 33,
        LogicalOrOperator = 34,
        LogicalAndOperator = 35,
        UnequalOperator = 36,
        EqualOperator = 37,
        GreaterOperator = 38,
        ShiftRightOperator = 39,
        GreaterEqualOperator = 40,
        SmallerOperator = 41,
        ShiftLeftOperator = 42,
        SmallerEqualOperator = 43,
        MinusOperator = 44,
        MinusMinusOperator = 45,
        PlusOperator = 46,
        PlusPlusOperator = 47,
        DivideOperator = 48,
        MultiplyOperator = 49,
        LogicalNegationOperator = 50,
        DotOperator = 51,
        QuestionMarkOperator = 52,
        ColonOperator = 53,
        ReferenceOperator = 54,
        ModuloOperator = 55,
        ComplimentOperator = 56,
        BitwiseAndOperator = 57,
        BitwiseXorOperator = 58,
        BitwiseOrOperator = 59,
        OpeningRoundBracket = 60,
        ClosingRoundBracket = 61,
        OpeningCurlyBracket = 62,
        ClosingCurlyBracket = 63,
        SquarebracketLeft = 64,
        SquarebracketRight = 65,
        CommaSeperator = 66,
        SemicolonSeperator = 67,
        BooleanLiteral = 68,
        False_ = 69,
        True_ = 70,
        IntegerLiteral = 71,
        FloatingLiteral = 72,
        StringLiteral = 73,
        UnterminatedStringLiteral = 74,
        DecimalLiteral = 75,
        Identifier = 76,
        COMMENT = 77,
        WHITESPACE = 78,
        LINE_COMMENT = 79;

    private readonly Dictionary < string, int > keywords = new Dictionary < string, int >
    {
        { "module", DeclareModule },
        { "class", DeclareClass },
        { "struct", DeclareStruct },
        { "new", DeclareClassInstance },
        { "function", DeclareFunction },
        { "var", DeclareVariable },
        { "get", DeclareGetter },
        { "set", DeclareSetter },
        { "for", DeclareForLoop },
        { "while", DeclareWhileLoop },
        { "static", DeclareStatic },
        { "abstract", DeclareAbstract },
        { "public", DeclarePublic },
        { "private", DeclarePrivate },
        { "if", ControlFlowIf },
        { "else", ControlFlowElse },
        { "return", FunctionReturn },
        { "break", Break },
        { "null", NullReference },
        { "this", ThisReference },
        { "using", UsingDirective },
        { "import", ImportDirective }
    };

    private readonly Queue < Token > m_TokenQueue = new Queue < Token >();

    internal virtual bool letter => char.IsLetter( c );

    internal virtual bool alphanumeric => char.IsLetterOrDigit( c ) || c == '.';

    internal virtual bool number => char.IsNumber( c );

    #region Public

    public BiteLexer( string input ) : base( input )
    {
    }

    public static bool IsInteger( string Expression )
    {
        int retNum;

        bool isNum = int.TryParse(
            Expression,
            NumberStyles.Integer,
            NumberFormatInfo.InvariantInfo,
            out retNum );

        return isNum;
    }

    public static bool IsNumeric( string Expression )
    {
        double retNum;

        bool isNum = double.TryParse(
            Expression,
            NumberStyles.Any,
            NumberFormatInfo.InvariantInfo,
            out retNum );

        return isNum;
    }

    public override string getTokenName( int x )
    {
        return tokenNames[x];
    }

    public override Token nextToken()
    {
        if ( m_TokenQueue.Count > 0 )
        {
            return m_TokenQueue.Dequeue();
        }

        while ( c != EOF_MARKER )
        {
            switch ( c )
            {
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    WS();

                    continue;

                case ',':
                    consume();

                    return new Token( CommaSeperator, "," );

                case '(':
                    consume();

                    return new Token( OpeningRoundBracket, "(" );

                case ')':
                    consume();

                    return new Token( ClosingRoundBracket, ")" );

                case '[':
                    consume();

                    return new Token( SquarebracketLeft, "[" );

                case ']':
                    consume();

                    return new Token( SquarebracketRight, "]" );

                case '.':
                    consume();

                    return new Token( DotOperator, "." );

                case '{':
                    consume();

                    return new Token( OpeningCurlyBracket, "{" );

                case '}':
                    consume();

                    return new Token( ClosingCurlyBracket, "}" );

                case ';':
                    consume();

                    return new Token( SemicolonSeperator, ";" );

                case '!':
                    char bangTest = peek();

                    if ( bangTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( UnequalOperator, "!=" );
                    }

                    consume();

                    return new Token( LogicalNegationOperator, "!" );

                case '%':
                    char moduloTest = peek();

                    if ( moduloTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( ModuloAssignOperator, "%=" );
                    }

                    consume();

                    return new Token( ModuloOperator, "%" );

                case '|':
                    char orTest = peek();

                    if ( orTest == '|' )
                    {
                        consume();
                        consume();

                        return new Token( LogicalOrOperator, "||" );
                    }

                    if ( orTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( BitwiseOrOperator, "|=" );
                    }

                    consume();

                    return new Token( BitwiseOrOperator, "|" );

                case '&':
                    char andTest = peek();

                    if ( andTest == '&' )
                    {
                        consume();
                        consume();

                        return new Token( LogicalAndOperator, "&&" );
                    }

                    if ( andTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( BitwiseAndAssignOperator, "&=" );
                    }

                    consume();

                    return new Token( BitwiseAndOperator, "&" );

                case '^':
                    char xorTest = peek();

                    if ( xorTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( BitwiseXorAssignOperator, "^=" );
                    }

                    consume();

                    return new Token( BitwiseXorOperator, "^" );

                case ':':

                    consume();

                    return new Token( ColonOperator, ":" );

                case '?':

                    consume();

                    return new Token( QuestionMarkOperator, "?" );

                case '~':

                    consume();

                    return new Token( ComplimentOperator, "~" );

                case '<':
                    char greateTest = peek();

                    if ( greateTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( SmallerEqualOperator, "<=" );
                    }

                    if ( greateTest == '<' )
                    {
                        char greateTest2 = peek( 2 );

                        if ( greateTest2 == '=' )
                        {
                            consume();
                            consume();
                            consume();

                            return new Token( BitwiseLeftShiftAssignOperator, "<<=" );
                        }

                        consume();
                        consume();

                        return new Token( ShiftLeftOperator, "<<" );
                    }

                    consume();

                    return new Token( SmallerOperator, "<" );

                case '>':
                    char eqTest = peek();

                    if ( eqTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( GreaterEqualOperator, ">=" );
                    }

                    if ( eqTest == '>' )
                    {
                        char eqTest2 = peek( 2 );

                        if ( eqTest2 == '=' )
                        {
                            consume();
                            consume();
                            consume();

                            return new Token( BitwiseRightShiftAssignOperator, ">>=" );
                        }

                        consume();
                        consume();

                        return new Token( ShiftRightOperator, ">>" );
                    }

                    consume();

                    return new Token( GreaterOperator, ">" );

                case '*':
                    char mulTest = peek();

                    if ( mulTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( MultiplyAssignOperator, "*=" );
                    }

                    consume();

                    return new Token( MultiplyOperator, "*" );

                case '/':
                    char divTest = peek();

                    if ( divTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( DivideAssignOperator, "-=" );
                    }

                    if ( divTest == '/' )
                    {
                        LINECOMMENT();

                        break;
                    }

                    if ( divTest == '*' )
                    {
                        COMMENTSECTION();

                        break;
                    }

                    consume();

                    return new Token( DivideOperator, "/" );

                case '+':
                    char plusTest = peek();

                    if ( plusTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( PlusAssignOperator, "+=" );
                    }

                    if ( plusTest == '+' )
                    {
                        consume();
                        consume();

                        return new Token( PlusPlusOperator, "++" );
                    }

                    consume();

                    return new Token( PlusOperator, "+" );

                case '-':
                    char minusTest = peek();

                    if ( minusTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( MinusAssignOperator, "-=" );
                    }

                    if ( minusTest == '-' )
                    {
                        consume();
                        consume();

                        return new Token( MinusMinusOperator, "--" );
                    }

                    if ( minusTest == '>' )
                    {
                        consume();
                        consume();

                        return new Token( ReferenceOperator, "->" );
                    }

                    consume();

                    return new Token( MinusOperator, "-" );

                case '=':
                    char equalTest = peek();

                    if ( equalTest == '=' )
                    {
                        consume();
                        consume();

                        return new Token( EqualOperator, "==" );
                    }

                    consume();

                    return new Token( AssignOperator, "=" );

                case '"':
                    consume();
                    StringBuilder stringBuilder = new StringBuilder();

                    while ( c != '"' )
                    {
                        stringBuilder.Append( c );
                        consume();
                    }

                    consume();

                    return new Token( StringLiteral, stringBuilder.ToString() );

                default:
                    StringBuilder buf = new StringBuilder();

                    while ( letter )
                    {
                        buf.Append( c );
                        LETTER();
                    }

                    while ( alphanumeric )
                    {
                        buf.Append( c );
                        ALPHANUMERIC();
                    }

                    string testString2 = buf.ToString();

                    if ( IsNumeric( testString2 ) )
                    {
                        if ( IsInteger( testString2 ) )
                        {
                            return new Token( IntegerLiteral, testString2 );
                        }

                        return new Token( FloatingLiteral, testString2 );
                    }

                    if ( keywords.ContainsKey( testString2 ) )
                    {
                        return new Token( keywords[testString2], testString2 );
                    }

                    if ( testString2 == "false" || testString2 == "true" )
                    {
                        return new Token( BooleanLiteral, testString2 );
                    }

                    StringBuilder call = new StringBuilder();
                    int counter = 0;

                    if ( testString2.Contains( "." ) )
                    {
                        foreach ( char c1 in testString2 )
                        {
                            if ( c1 == '.' )
                            {
                                string tokenString = call.ToString();

                                if ( IsNumeric( tokenString ) )
                                {
                                    if ( IsInteger( tokenString ) )
                                    {
                                        m_TokenQueue.Enqueue( new Token( IntegerLiteral, tokenString ) );
                                    }
                                    else
                                    {
                                        m_TokenQueue.Enqueue( new Token( FloatingLiteral, tokenString ) );
                                    }
                                }
                                else
                                {
                                    m_TokenQueue.Enqueue( new Token( Identifier, tokenString ) );
                                }

                                m_TokenQueue.Enqueue( new Token( DotOperator, "." ) );

                                if ( counter < testString2.Length - 2 )
                                {
                                    call.Clear();
                                }
                            }
                            else
                            {
                                call.Append( c1 );
                            }
                        }

                        m_TokenQueue.Enqueue( new Token( Identifier, call.ToString() ) );
                    }
                    else
                    {
                        m_TokenQueue.Enqueue( new Token( Identifier, testString2 ) );
                    }

                    return m_TokenQueue.Dequeue();

                    throw new Exception( "invalid character: " + c );
            }
        }

        return new Token( EOF_TYPE, "<EOF>" );
    }

    internal virtual void ALPHANUMERIC()
    {
        if ( alphanumeric )
        {
            consume();
        }
        else
        {
            throw new Exception( "expecting ALPHANUMERIC; found " + c );
        }
    }

    /// <summary>
    ///     LETTER   : 'a'..'z'|'A'..'Z'; // define what a letter is (\w)
    /// </summary>
    internal virtual void LETTER()
    {
        if ( letter )
        {
            consume();
        }
        else
        {
            throw new Exception( "expecting LETTER; found " + c );
        }
    }

    internal virtual void NUMBER()
    {
        if ( number )
        {
            consume();
        }
        else
        {
            throw new Exception( "expecting NUMBER; found " + c );
        }
    }

    /// <summary>
    ///     WS : (' '|'\t'|'\n'|'\r')* ; // ignore any whitespace
    /// </summary>
    internal override void WS()
    {
        while ( c == ' ' || c == '\t' || c == '\n' || c == '\r' )
        {
            consume();
        }
    }

    #endregion

    #region Private

    private void COMMENTSECTION()
    {
        while ( true )
        {
            consume();

            if ( c == '*' && peek() == '/' )
            {
                consume();
                consume();

                break;
            }
        }
    }

    private void LINECOMMENT()
    {
        while ( c != '\n' )
        {
            consume();
        }
    }

    #endregion
}

}
