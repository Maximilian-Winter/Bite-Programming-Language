using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bite.Symbols
{

[DebuggerDisplay( "{Name}" )]
public abstract class BaseScope : Scope
{
    public abstract string Name { get; }

    public virtual IDictionary < string, Symbol > Members => symbols;

    public virtual Scope EnclosingScope
    {
        set => enclosingScope = value;
        get => enclosingScope;
    }

    public virtual IList < Scope > EnclosingPathToRoot
    {
        get
        {
            IList < Scope > scopes = new List < Scope >();
            Scope s = this;

            while ( s != null )
            {
                scopes.Add( s );
                s = s.EnclosingScope;
            }

            return scopes;
        }
    }

    public virtual IList < Symbol > Symbols
    {
        get
        {
            ICollection < Symbol > values = symbols.Values;

            if ( values is IList )
            {
                return ( IList < Symbol > ) values;
            }

            return new List < Symbol >( values );
        }
    }

    public virtual IList < Symbol > AllSymbols
    {
        get
        {
            IList < Symbol > syms = new List < Symbol >();
            ( ( List < Symbol > ) syms ).AddRange( Symbols );

            foreach ( Symbol s in symbols.Values )
            {
                if ( s is Scope scope )
                {
                    ( ( List < Symbol > ) syms ).AddRange( scope.AllSymbols );
                }
            }

            return syms;
        }
    }

    public virtual int NumberOfSymbols => symbols.Count;

    public virtual int NestedSymbolCount
    {
        get
        {
            int t = 0;

            for ( int i = 0; i < nestedScopesNotSymbols.Count; i++ )
            {
                t += ( nestedScopesNotSymbols[i] as BaseScope ).NumberOfSymbols;
            }

            return t;
        }
    }

    #region Public

    public virtual void define( Symbol sym )
    {
        //GlobalSymbolTable.Define( sym );
        if ( symbols.ContainsKey( sym.Name ) )
        {
            throw new ArgumentException( "duplicate symbol " + sym.Name );
        }

        sym.SymbolScope = this;
        sym.InsertionOrderNumber = symbols.Count;
        symbols[sym.Name] = sym;
    }

    public virtual void nest( Scope scope )
    {
        if ( scope is SymbolWithScope )
        {
            throw new ArgumentException( "Add SymbolWithScope instance " + scope.Name + " via nest()" );
        }

        nestedScopesNotSymbols.Add( scope );
    }

    public virtual Symbol resolve( string name, out int moduleId, ref int depth, bool throwErrorWhenNotFound = true )
    {
        if ( symbols.ContainsKey( name ) )
        {
            moduleId = -1;

            return symbols[name];
        }

        Scope parent = EnclosingScope;

        if ( parent != null )
        {
            depth++;

            return parent.resolve( name, out moduleId, ref depth, throwErrorWhenNotFound );
        }

        moduleId = -2;

        return null;
    }

    public override string ToString()
    {
        return symbols.Keys.ToString();
    }

    #endregion

    #region Protected

    protected BaseScope()
    {
    }

    protected BaseScope( Scope enclosingScope )
    {
        EnclosingScope = enclosingScope;
    }

    #endregion

    protected internal Scope enclosingScope;

    protected internal IList < Scope > nestedScopesNotSymbols = new List < Scope >();
    protected internal IDictionary < string, Symbol > symbols = new Dictionary < string, Symbol >();
}

}
