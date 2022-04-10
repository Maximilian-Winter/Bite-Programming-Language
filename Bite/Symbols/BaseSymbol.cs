using Bite.Ast;

namespace Bite.Symbols
{

public abstract class BaseSymbol : Symbol
{
    public virtual string Name => name;

    public virtual Scope SymbolScope
    {
        get => scope;
        set => scope = value;
    }

    public bool IsExternal => isExternal;

    public virtual Type Type
    {
        get => type;
        set => type = value;
    }

    public virtual HeteroAstNode DefNode
    {
        set => defNode = value;
        get => defNode;
    }

    public virtual int InsertionOrderNumber
    {
        get => lexicalOrder;
        set => lexicalOrder = value;
    }

    #region Public

    public BaseSymbol( string name )
    {
        this.name = name;
    }

    public BaseSymbol( string name, bool isExternal )
    {
        this.name = name;
        this.isExternal = isExternal;
    }

    public override bool Equals( object obj )
    {
        if ( !( obj is Symbol ) )
        {
            return false;
        }

        if ( obj == this )
        {
            return true;
        }

        return name.Equals( ( ( Symbol ) obj ).Name );
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }

    public override string ToString()
    {
        string s = "";

        if ( scope != null )
        {
            s = scope.Name + ".";
        }

        if ( type != null )
        {
            string ts = type.ToString();

            if ( type is SymbolWithScope )
            {
                ts = ( ( SymbolWithScope ) type ).getFullyQualifiedName( "." );
            }

            return '<' + s + Name + ":" + ts + '>';
        }

        return s + Name;
    }

    #endregion

    protected internal HeteroAstNode defNode;
    protected internal int lexicalOrder;
    protected internal readonly string name;
    protected internal Scope scope;
    protected internal Type type;
    protected readonly bool isExternal;
}

}
