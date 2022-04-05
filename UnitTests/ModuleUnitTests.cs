#define USE_NEW_PARSER

using System.Collections.Generic;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Xunit;

namespace UnitTests
{

public class ModuleUnitTests
{
    private BiteResult ExecModules( string mainModule, IEnumerable < string > modules )
    {
#if USE_NEW_PARSER
        var compiler = new BITECompiler();
#else
        var compiler = new Compiler( true );
#endif
        BiteProgram program = compiler.Compile( mainModule, modules );

        return program.Run();
    }

    [Fact]
    public void LoadModulesInDependencyOrder()
    {
        string mainModule = "module MainModule; import SubModule; using SubModule; var a = SubModule.a; a;";
        string subModule = "module SubModule; import SubSubModule; using SubSubModule; var a = SubSubModule.a;";
        string subSubModule = "module SubSubModule; var a = 12345;";
        BiteResult result = ExecModules( "MainModule", new[] { subSubModule, subModule, mainModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 12345, result.ReturnValue.NumberData );
    }

    [Fact]
    public void LoadModulesInReversedDependencyOrder()
    {
        string mainModule = "module MainModule; import SubModule; using SubModule; var a = SubModule.a; a;";
        string subModule = "module SubModule; import SubSubModule; using SubSubModule; var a = SubSubModule.a;";
        string subSubModule = "module SubSubModule; var a = 12345;";
        BiteResult result = ExecModules( "MainModule", new[] { mainModule, subModule, subSubModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 12345, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ReferenceStringsFromSubmodule()
    {
        string mainModule =
            "module MainModule; import SubModule; using SubModule; var greeting = SubModule.a + \" \" + SubModule.b; greeting;";

        string subModule = "module SubModule; var a = \"Hello\"; var b = \"World!\";";
        BiteResult result = ExecModules( "MainModule", new[] { mainModule, subModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World!", result.ReturnValue.StringData );
    }

    [Fact]
    public void VariableDeclaration()
    {
        string mainModule = "module MainModule; import System; using System; var a = 1;";
        BiteResult result = ExecModules( "MainModule", new[] { mainModule } );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }
}

}
