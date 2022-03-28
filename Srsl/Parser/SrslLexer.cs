using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Srsl.Parser
{

    public class SrslLexer : Lexer
    {
        public static readonly string[] tokenNames =
        {
        "DeclareModule", "DeclareClass", "DeclareStruct", "DeclareClassInstance",
        "DeclareFunction", "DeclareVariable", "DeclareGetter", "DeclareSetter",
        "DeclareForLoop", "DeclareWhileLoop", "DeclareStatic", "DeclareAbstract",
        "DeclarePublic", "DeclarePrivate", "ControlFlowIf", "ControlFlowElse",
        "FunctionReturn", "NullReference", "ThisReference", "UsingDirective",
        "ImportDirective", "AssignOperator", "PlusAssignOperator", "MinusAssignOperator",
        "MultiplyAssignOperator", "DivideAssignOperator", "ModuloAssignOperator",
        "BitwiseAndAssignOperator", "BitwiseOrAssignOperator", "BitwiseXorAssignOperator",
        "BitwiseLeftShiftAssignOperator", "BitwiseRightShiftAssignOperator", "LogicalOrOperator",
        "LogicalAndOperator", "UnequalOperator", "EqualOperator", "GreaterOperator",
        "ShiftRightOperator", "GreaterEqualOperator", "SmallerOperator", "ShiftLeftOperator",
        "SmallerEqualOperator", "MinusOperator", "MinusMinusOperator", "PlusOperator",
        "PlusPlusOperator", "DivideOperator", "MultiplyOperator", "LogicalNegationOperator",
        "DotOperator", "QuestionMarkOperator", "ColonOperator", "ReferenceOperator",
        "ModuloOperator", "ComplimentOperator", "BitwiseAndOperator", "BitwiseXorOperator",
        "BitwiseOrOperator", "OpeningRoundBracket", "ClosingRoundBracket", "OpeningCurlyBracket",
        "ClosingCurlyBracket", "SquarebracketLeft", "SquarebracketRight", "CommaSeperator",
        "SemicolonSeperator", "BooleanLiteral", "False_", "True_", "IntegerLiteral",
        "FloatingLiteral", "StringLiteral", "UnterminatedStringLiteral",
        "DecimalLiteral", "Identifier", "COMMENT", "WHITESPACE", "LINE_COMMENT"
    };

        public const int
            DeclareModule = 1, DeclareClass = 2, DeclareStruct = 3, DeclareClassInstance = 4,
            DeclareFunction = 5, DeclareVariable = 6, DeclareGetter = 7, DeclareSetter = 8,
            DeclareForLoop = 9, DeclareWhileLoop = 10, DeclareStatic = 11, DeclareAbstract = 12,
            DeclarePublic = 13, DeclarePrivate = 14, ControlFlowIf = 15, ControlFlowElse = 16,
            FunctionReturn = 17, NullReference = 18, ThisReference = 19, UsingDirective = 20,
            ImportDirective = 21, AssignOperator = 22, PlusAssignOperator = 23, MinusAssignOperator = 24,
            MultiplyAssignOperator = 25, DivideAssignOperator = 26, ModuloAssignOperator = 27,
            BitwiseAndAssignOperator = 28, BitwiseOrAssignOperator = 29, BitwiseXorAssignOperator = 30,
            BitwiseLeftShiftAssignOperator = 31, BitwiseRightShiftAssignOperator = 32,
            LogicalOrOperator = 33, LogicalAndOperator = 34, UnequalOperator = 35, EqualOperator = 36,
            GreaterOperator = 37, ShiftRightOperator = 38, GreaterEqualOperator = 39, SmallerOperator = 40,
            ShiftLeftOperator = 41, SmallerEqualOperator = 42, MinusOperator = 43, MinusMinusOperator = 44,
            PlusOperator = 45, PlusPlusOperator = 46, DivideOperator = 47, MultiplyOperator = 48,
            LogicalNegationOperator = 49, DotOperator = 50, QuestionMarkOperator = 51, ColonOperator = 52,
            ReferenceOperator = 53, ModuloOperator = 54, ComplimentOperator = 55, BitwiseAndOperator = 56,
            BitwiseXorOperator = 57, BitwiseOrOperator = 58, OpeningRoundBracket = 59, ClosingRoundBracket = 60,
            OpeningCurlyBracket = 61, ClosingCurlyBracket = 62, SquarebracketLeft = 63,
            SquarebracketRight = 64, CommaSeperator = 65, SemicolonSeperator = 66, BooleanLiteral = 67,
            False_ = 68, True_ = 69, IntegerLiteral = 70, FloatingLiteral = 71, StringLiteral = 72,
            UnterminatedStringLiteral = 73, DecimalLiteral = 74, Identifier = 75, COMMENT = 76,
            WHITESPACE = 77, LINE_COMMENT = 78;

        private readonly Dictionary<string, int> keywords = new Dictionary<string, int>
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
        { "null", NullReference },
        { "this", ThisReference },
        { "using", UsingDirective },
        { "import", ImportDirective }
    };

        private Queue<Token> m_TokenQueue = new Queue<Token>();

        internal virtual bool letter => char.IsLetter(c);

        internal virtual bool alphanumeric => char.IsLetterOrDigit(c) || c == '.';

        internal virtual bool number => char.IsNumber(c);

        #region Public

        public SrslLexer(string input) : base(input)
        {
        }

        public static bool IsInteger(string Expression)
        {
            int retNum;

            bool isNum = int.TryParse(
                Expression,
                NumberStyles.Integer,
                NumberFormatInfo.InvariantInfo,
                out retNum);

            return isNum;
        }

        public static bool IsNumeric(string Expression)
        {
            double retNum;

            bool isNum = double.TryParse(
                Expression,
                NumberStyles.Any,
                NumberFormatInfo.InvariantInfo,
                out retNum);

            return isNum;
        }

        public override string getTokenName(int x)
        {
            return tokenNames[x];
        }

        public override Token nextToken()
        {
            if (m_TokenQueue.Count > 0)
            {
                return m_TokenQueue.Dequeue();
            }

            while (c != EOF_MARKER)
            {
                switch (c)
                {
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        WS();

                        continue;

                    case ',':
                        consume();

                        return new Token(CommaSeperator, ",");

                    case '(':
                        consume();

                        return new Token(OpeningRoundBracket, "(");

                    case ')':
                        consume();

                        return new Token(ClosingRoundBracket, ")");

                    case '[':
                        consume();

                        return new Token(SquarebracketLeft, "[");

                    case ']':
                        consume();

                        return new Token(SquarebracketRight, "]");

                    case '.':
                        consume();

                        return new Token(DotOperator, ".");

                    case '{':
                        consume();

                        return new Token(OpeningCurlyBracket, "{");

                    case '}':
                        consume();

                        return new Token(ClosingCurlyBracket, "}");

                    case ';':
                        consume();

                        return new Token(SemicolonSeperator, ";");

                    case '!':
                        char bangTest = peek();

                        if (bangTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(UnequalOperator, "!=");
                        }

                        consume();

                        return new Token(LogicalNegationOperator, "!");

                    case '%':
                        char moduloTest = peek();

                        if (moduloTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(ModuloAssignOperator, "%=");
                        }

                        consume();

                        return new Token(ModuloOperator, "%");

                    case '|':
                        char orTest = peek();

                        if (orTest == '|')
                        {
                            consume();
                            consume();

                            return new Token(LogicalOrOperator, "||");
                        }

                        if (orTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(BitwiseOrOperator, "|=");
                        }

                        consume();

                        return new Token(BitwiseOrOperator, "|");

                    case '&':
                        char andTest = peek();

                        if (andTest == '&')
                        {
                            consume();
                            consume();

                            return new Token(LogicalAndOperator, "&&");
                        }

                        if (andTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(BitwiseAndAssignOperator, "&=");
                        }

                        consume();

                        return new Token(BitwiseAndOperator, "&");

                    case '^':
                        char xorTest = peek();

                        if (xorTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(BitwiseXorAssignOperator, "^=");
                        }

                        consume();

                        return new Token(BitwiseXorOperator, "^");

                    case ':':

                        consume();

                        return new Token(ColonOperator, ":");

                    case '?':

                        consume();

                        return new Token(QuestionMarkOperator, "?");

                    case '~':

                        consume();

                        return new Token(ComplimentOperator, "~");

                    case '<':
                        char greateTest = peek();

                        if (greateTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(SmallerEqualOperator, "<=");
                        }

                        if (greateTest == '<')
                        {
                            char greateTest2 = peek(2);

                            if (greateTest2 == '=')
                            {
                                consume();
                                consume();
                                consume();

                                return new Token(BitwiseLeftShiftAssignOperator, "<<=");
                            }

                            consume();
                            consume();

                            return new Token(ShiftLeftOperator, "<<");
                        }

                        consume();

                        return new Token(SmallerOperator, "<");

                    case '>':
                        char eqTest = peek();

                        if (eqTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(GreaterEqualOperator, ">=");
                        }

                        if (eqTest == '>')
                        {
                            char eqTest2 = peek(2);

                            if (eqTest2 == '=')
                            {
                                consume();
                                consume();
                                consume();

                                return new Token(BitwiseRightShiftAssignOperator, ">>=");
                            }

                            consume();
                            consume();

                            return new Token(ShiftRightOperator, ">>");
                        }

                        consume();

                        return new Token(GreaterOperator, ">");

                    case '*':
                        char mulTest = peek();

                        if (mulTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(MultiplyAssignOperator, "*=");
                        }

                        consume();

                        return new Token(MultiplyOperator, "*");

                    case '/':
                        char divTest = peek();

                        if (divTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(DivideAssignOperator, "-=");
                        }

                        if (divTest == '/')
                        {
                            LINECOMMENT();

                            break;
                        }

                        if (divTest == '*')
                        {
                            COMMENTSECTION();

                            break;
                        }

                        consume();

                        return new Token(DivideOperator, "/");

                    case '+':
                        char plusTest = peek();

                        if (plusTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(PlusAssignOperator, "+=");
                        }

                        if (plusTest == '+')
                        {
                            consume();
                            consume();

                            return new Token(PlusPlusOperator, "++");
                        }

                        consume();

                        return new Token(PlusOperator, "+");

                    case '-':
                        char minusTest = peek();

                        if (minusTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(MinusAssignOperator, "-=");
                        }

                        if (minusTest == '-')
                        {
                            consume();
                            consume();

                            return new Token(MinusMinusOperator, "--");
                        }

                        if (minusTest == '>')
                        {
                            consume();
                            consume();

                            return new Token(ReferenceOperator, "->");
                        }

                        consume();

                        return new Token(MinusOperator, "-");

                    case '=':
                        char equalTest = peek();

                        if (equalTest == '=')
                        {
                            consume();
                            consume();

                            return new Token(EqualOperator, "==");
                        }

                        consume();

                        return new Token(AssignOperator, "=");

                    case '"':
                        consume();
                        StringBuilder stringBuilder = new StringBuilder();

                        while (c != '"')
                        {
                            stringBuilder.Append(c);
                            consume();
                        }

                        consume();

                        return new Token(StringLiteral, stringBuilder.ToString());

                    default:
                        StringBuilder buf = new StringBuilder();

                        while (letter)
                        {
                            buf.Append(c);
                            LETTER();
                        }

                        while (alphanumeric)
                        {
                            buf.Append(c);
                            ALPHANUMERIC();
                        }

                        string testString2 = buf.ToString();

                        if (IsNumeric(testString2))
                        {
                            if (IsInteger(testString2))
                            {
                                return new Token(IntegerLiteral, testString2);
                            }

                            return new Token(FloatingLiteral, testString2);
                        }

                        if (keywords.ContainsKey(testString2))
                        {
                            return new Token(keywords[testString2], testString2);
                        }

                        if (testString2 == "false" || testString2 == "true")
                        {
                            return new Token(BooleanLiteral, testString2);
                        }

                        StringBuilder call = new StringBuilder();
                        int counter = 0;

                        if (testString2.Contains("."))
                        {
                            foreach (char c1 in testString2)
                            {
                                if (c1 == '.')
                                {
                                    string tokenString = call.ToString();

                                    if (IsNumeric(tokenString))
                                    {
                                        if (IsInteger(tokenString))
                                        {
                                            m_TokenQueue.Enqueue(new Token(IntegerLiteral, tokenString));
                                        }
                                        else
                                        {
                                            m_TokenQueue.Enqueue(new Token(FloatingLiteral, tokenString));
                                        }
                                    }
                                    else
                                    {
                                        m_TokenQueue.Enqueue(new Token(Identifier, tokenString));
                                    }

                                    m_TokenQueue.Enqueue(new Token(DotOperator, "."));

                                    if (counter < testString2.Length - 2)
                                    {
                                        call.Clear();
                                    }
                                }
                                else
                                {
                                    call.Append(c1);
                                }
                            }

                            m_TokenQueue.Enqueue(new Token(Identifier, call.ToString()));
                        }
                        else
                        {
                            m_TokenQueue.Enqueue(new Token(Identifier, testString2));
                        }

                        return m_TokenQueue.Dequeue();

                        throw new Exception("invalid character: " + c);
                }
            }

            return new Token(EOF_TYPE, "<EOF>");
        }

        internal virtual void ALPHANUMERIC()
        {
            if (alphanumeric)
            {
                consume();
            }
            else
            {
                throw new Exception("expecting ALPHANUMERIC; found " + c);
            }
        }

        /// <summary>
        ///     LETTER   : 'a'..'z'|'A'..'Z'; // define what a letter is (\w)
        /// </summary>
        internal virtual void LETTER()
        {
            if (letter)
            {
                consume();
            }
            else
            {
                throw new Exception("expecting LETTER; found " + c);
            }
        }

        internal virtual void NUMBER()
        {
            if (number)
            {
                consume();
            }
            else
            {
                throw new Exception("expecting NUMBER; found " + c);
            }
        }

        /// <summary>
        ///     WS : (' '|'\t'|'\n'|'\r')* ; // ignore any whitespace
        /// </summary>
        internal override void WS()
        {
            while (c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                consume();
            }
        }

        #endregion

        #region Private

        private void COMMENTSECTION()
        {
            while (true)
            {
                consume();

                if (c == '*' && peek() == '/')
                {
                    consume();
                    consume();

                    break;
                }
            }
        }

        private void LINECOMMENT()
        {
            while (c != '\n')
            {
                consume();
            }
        }

        #endregion
    }

}