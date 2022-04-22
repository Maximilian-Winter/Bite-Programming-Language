using System.Collections.Generic;
using Bite.Compiler;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Bite.Symbols;
using Xunit;

namespace UnitTests
{

public class ModuleUnitTests
{
    private BiteResult ExecModules( IEnumerable < string > modules )
    {
        BiteCompiler compiler = new BiteCompiler();

        BiteProgram program = compiler.Compile( modules );

        return program.Run();
    }

    [Fact]
    public void AmbiguousReferences()
    {
        string mainModule =
            "module MainModule; import SubModuleA; import SubModuleB; using SubModuleA; using SubModuleB; var greeting = Hello + \" \" + World; greeting;";

        string subModuleA = "module SubModuleA; var Hello = \"Hi\"; var World = \"Fellow Kids!\";";
        string subModuleB = "module SubModuleB; var Hello = \"Hello\"; var World = \"World!\";";

        try
        {
            BiteResult result = ExecModules( new[] { mainModule, subModuleA, subModuleB } );
        }
        catch ( BiteSymbolTableException e )
        {
            Assert.Equal( "Symbol Table Error: Ambiguous references: Hello", e.BiteSymbolTableExceptionMessage );
        }
    }

    [Fact]
    public void LoadModuleDependencyBehavior()
    {
        string moduleA = "module ModuleA; import ModuleC; import ModuleB; var a = 1; ModuleB.foo();";
        string moduleB = "module ModuleB; import ModuleD; ModuleD.d = 11; function foo() { return ModuleD.d; }";
        string moduleC = "module ModuleC; import ModuleD; ModuleD.d = 13; function foo() { return ModuleD.d; }";
        string moduleD = "module ModuleD; var d = 7;";
        BiteResult result = ExecModules( new[] { moduleA, moduleC, moduleB, moduleD } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 11, result.ReturnValue.NumberData );
    }

    [Fact]
    public void LoadModuleDependencyBehaviorInvariant()
    {
        string moduleA = "module ModuleA; import ModuleC; import ModuleB; var a = 1; var b = ModuleB.foo(); b;";
        string moduleB = "module ModuleB; import ModuleD; ModuleD.d = 11; function foo() { return ModuleD.d; }";
        string moduleC = "module ModuleC; import ModuleD; ModuleD.d = 13; function foo() { return ModuleD.d; }";
        string moduleD = "module ModuleD; var d = 7;";
        BiteResult result = ExecModules( new[] { moduleC, moduleA, moduleD, moduleB } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 11, result.ReturnValue.NumberData );
    }

    [Fact]
    public void LoadModulesInDependencyOrder()
    {
        string mainModule = "module MainModule; import SubModule; using SubModule; var a = SubModule.a; a;";
        string subModule = "module SubModule; import SubSubModule; using SubSubModule; var a = SubSubModule.a;";
        string subSubModule = "module SubSubModule; var a = 12345;";
        BiteResult result = ExecModules( new[] { subSubModule, subModule, mainModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 12345, result.ReturnValue.NumberData );
    }

    [Fact]
    public void LoadModulesInReversedDependencyOrder()
    {
        string mainModule = "module MainModule; import SubModule; using SubModule; var a = SubModule.a; a;";
        string subModule = "module SubModule; import SubSubModule; using SubSubModule; var a = SubSubModule.a;";
        string subSubModule = "module SubSubModule; var a = 12345;";
        BiteResult result = ExecModules( new[] { mainModule, subModule, subSubModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 12345, result.ReturnValue.NumberData );
    }

    [Fact]
    public void LoadSystemModule()
    {
        string mainModule = "module MainModule; import System;";
        BiteResult result = ExecModules( new[] { mainModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void ModuleFunctionCallWithoutUsing()
    {
        string mainModule = "module MainModule; import System; System.PrintLine( \"Hello World!\" );";
        BiteResult result = ExecModules( new[] { mainModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void ReferenceStringsFromSubmodule()
    {
        string mainModule =
            "module MainModule; import SubModule; using SubModule; var greeting = SubModule.a + \" \" + SubModule.b; greeting;";

        string subModule = "module SubModule; var a = \"Hello\"; var b = \"World!\";";
        BiteResult result = ExecModules( new[] { mainModule, subModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World!", result.ReturnValue.StringData );
    }

    [Fact]
    public void SystemModuleDeclaration()
    {
        string mainModule = "module MainModule; import System; using System; PrintLine( \"Hello World!\" );";
        BiteResult result = ExecModules( new[] { mainModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void VariableDeclaration()
    {
        string mainModule = "module MainModule; import System; using System; var a = 1;";
        BiteResult result = ExecModules( new[] { mainModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }
}

}
