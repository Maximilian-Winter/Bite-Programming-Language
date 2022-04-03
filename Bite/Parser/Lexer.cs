using System;

namespace Bite.Parser
{

public abstract class Lexer
{
    public static readonly char EOF_MARKER = char.MaxValue;
    public static int CurrentLineNumber = 0;
    public static int CurrentColumnNumber = 0;
    public const int EOF_TYPE = 0;
    internal string input;
    internal int i = 0;
    internal char c;

    #region Public

    public Lexer( string input )
    {
        this.input = input;
        c = input[i];
    }

    public abstract string getTokenName( int x );

    public abstract Token nextToken();

    internal abstract void WS();

    public virtual void consume()
    {
        advance();
    }

    public virtual void match( char x )
    {
        if ( c == x )
        {
            consume();
        }
        else
        {
            throw new Exception( "expecting " + x + "; found " + c );
        }
    }

    public virtual char peek()
    {
        return input[i + 1];
    }

    public virtual char peek( int p )
    {
        return input[i + p];
    }

    #endregion

    #region Private

    private void advance()
    {
        i++;

        if ( i >= input.Length )
        {
            c = EOF_MARKER;
        }
        else
        {
            if ( c == '\n' )
            {
                CurrentLineNumber++;
                CurrentColumnNumber = 0;
            }
            else
            {
                if ( c == '\t' )
                {
                    CurrentColumnNumber += 5;
                }
                else
                {
                    CurrentColumnNumber++;
                }
            }

            c = input[i];
        }
    }

    #endregion
}

}
