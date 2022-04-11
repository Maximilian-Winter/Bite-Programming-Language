﻿using System.Collections.Generic;
using Bite.Ast;

namespace Bite.Symbols
{

public class FunctionSymbol : SymbolWithScope, TypedSymbol
{
    public virtual FunctionDeclarationBaseNode DefBaseNode
    {
        set => m_DefBaseNode = value;
        get => m_DefBaseNode;
    }

    public virtual Type Type
    {
        get => retType;
        set => retType = value;
    }

    public virtual int NumberOfParameters
    {
        get
        {
            int i = 0;

            foreach ( KeyValuePair < string, Symbol > sym in symbols )
            {
                if ( sym.Value is ParameterSymbol )
                {
                    i++;
                }
            }

            return i;
        }
    }

    public ClassAndMemberModifiers ClassAndMemberModifiers => m_ClassAndMemberModifier;

    public AccesModifierType AccesModifier => m_AccessModifier;

    public bool IsExtern => m_IsExtern;

    public bool IsCallable => m_IsCallable;

    public string LinkName;

    #region Public

        public FunctionSymbol(
        string name,
        AccesModifierType accesModifierType,
        ClassAndMemberModifiers classAndMemberModifiers,
        bool isExtern,
        bool isCallable
        ) : base( name )
    {
        m_AccessModifier = accesModifierType;
        m_ClassAndMemberModifier = classAndMemberModifiers;
        m_IsExtern = isExtern;
        m_IsCallable = isCallable;
    }

    public override string ToString()
    {
        return name + ":" + base.ToString();
    }

    #endregion

    protected internal FunctionDeclarationBaseNode m_DefBaseNode;
    protected internal AccesModifierType m_AccessModifier;
    protected internal ClassAndMemberModifiers m_ClassAndMemberModifier;
    protected internal Type retType;
    protected internal bool m_IsExtern;
    protected internal bool m_IsCallable;
}

}
