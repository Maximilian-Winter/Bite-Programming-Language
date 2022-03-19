using System;
using System.Collections.Generic;

namespace Srsl_Parser
{

public abstract class Parser
{
    public const int FAILED = -1;
    internal Lexer input;
    internal IList < int > markers;
    internal IList < Token > lookahead;
    internal int p = 0;

    public virtual bool Speculating => markers.Count > 0;

    #region Public

    public Parser( Lexer input )
    {
        this.input = input;
        markers = new List < int >();
        lookahead = new List < Token >();
        sync( 1 );
    }

    public abstract void clearMemo();

    public virtual bool alreadyParsedRule(
        IDictionary < int, IDictionary < string, int > > memoization,
        string ruleName )
    {
        int indexV = index();

        if ( !memoization.ContainsKey( indexV ) )
        {
            return false;
        }

        IDictionary < string, int > memoI = memoization[indexV];

        if ( !memoI.ContainsKey( ruleName ) )
        {
            return false;
        }

        int memo = memoI[ruleName];

        if ( memo == FAILED )
        {
            //Console.WriteLine( "Previously Failed: " + ruleName );

            throw new PreviousParseFailedException();
        }

        /*Console.WriteLine(
            "Parsed " +
            ruleName +
            " before at Token Index: " +
            index() + " Line: " + lookahead[index()].DebugInfo.LineNumber + " Column: " + lookahead[index()].DebugInfo.ColumnNumber +
            "; skip ahead to token index " +
            memo +
            ": " +
            lookahead[memo].text+ " Line: " + lookahead[memo].DebugInfo.LineNumber + " Column: " + lookahead[memo].DebugInfo.ColumnNumber );*/

        seek( memo );

        return true;
    }

    public virtual void consume()
    {
        p++;

        if ( p == lookahead.Count && !Speculating )
        {
            p = 0;
            lookahead.Clear();
            clearMemo();
        }

        sync( 1 );
    }

    public virtual void fill( int n )
    {
        for ( int i = 1; i <= n; i++ )
        {
            lookahead.Add( input.nextToken() );
        }
    }

    public virtual int index()
    {
        return p;
    }

    public virtual int LA( int i )
    {
        return LT( i ).type;
    }

    public virtual Token LT( int i )
    {
        sync( i );

        return lookahead[p + i - 1];
    }

    public virtual int mark()
    {
        markers.Add( p );

        return p;
    }

    public virtual void match( int x )
    {
        if ( LA( 1 ) == x )
        {
            consume();
        }
        else
        {
            throw new MismatchedTokenException( "expecting " + SrslLexer.tokenNames[x > 0? x -1 : x] + " found " + LT( 1 ) );
        }
    }

    public virtual void memoize(
        IDictionary < int, IDictionary < string, int > > memoization,
        string ruleName,
        int startTokenIndex,
        bool failed )
    {
        int stopTokenIndex = failed ? FAILED : index();

        if ( memoization.ContainsKey( startTokenIndex ) )
        {
            memoization[startTokenIndex][ruleName] = stopTokenIndex;
        }
        else
        {
            memoization[startTokenIndex] = new Dictionary < string, int >();
            memoization[startTokenIndex][ruleName] = stopTokenIndex;
        }
    }

    public virtual void release()
    {
        int marker = markers[markers.Count - 1];
        markers.RemoveAt( markers.Count - 1 );
        seek( marker );
    }

    public virtual void seek( int index )
    {
        p = index;
    }

    public virtual void sync( int i )
    {
        if ( p + i - 1 > lookahead.Count - 1 )
        {
            int n = p + i - 1 - ( lookahead.Count - 1 );
            fill( n );
        }
    }

    #endregion
}

}