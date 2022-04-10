﻿using System.Collections.Generic;

namespace Bite.Symbols
{

public class SymbolTable
{
    public SymbolTable( )
    {
        RootScope = new GlobalScope();
        CurrentScope = RootScope;
    }

    /// <summary>
    /// Defines the specified module on the Global scope
    /// </summary>
    /// <returns></returns>
    public SymbolTable WithModule( ModuleSymbol module )
    {
        RootScope.DefineModule( module );

        return this;
    }

    /// <summary>
    /// Creates the System module and defines it on the Global scope
    /// </summary>
    /// <returns></returns>
    public SymbolTable WithSystem()
    {
        //var systemModuleBuilder = new SystemModuleBuilder();

        //var systemModule = systemModuleBuilder.BuildSystemModule();

        //RootScope.DefineModule( systemModule );

        return this;
    }

    /// <summary>
    /// Defines external objects as variables to the Global scope
    /// </summary>
    /// <param name="externalObjects"></param>
    /// <returns></returns>
    public SymbolTable WithExternalObjects( Dictionary<string, object> externalObjects = null )
    {
        if (externalObjects != null)
        {
            foreach (var module in externalObjects)
            {
                VariableSymbol variableSymbol =
                    new VariableSymbol( module.Key, AccesModifierType.Public, ClassAndMemberModifiers.Static, true );
                RootScope.DefineVariable( variableSymbol );
            }
        }
        return this;
    }

    /// <summary>
    /// Returns a new <see cref="SymbolTable"/> with the System module defined
    /// </summary>
    public static SymbolTable Default => new SymbolTable().WithSystem();

    internal void PopScope()
    {
        CurrentScope = CurrentScope.EnclosingScope;
    }

    internal void PushScope( Scope s )
    {
        CurrentScope = s;
    }

    internal Scope CurrentScope { get; private set; }
    
    public GlobalScope RootScope { get; private set; }
}

}