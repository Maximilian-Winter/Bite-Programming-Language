using System.Collections.Generic;
using Bite.Parser;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Xunit;

namespace UnitTests
{

    public class ModuleUnitTests
    {

        private BiteResult ExecModules(string mainModule, IEnumerable<string> modules)
        {
            var compiler = new Compiler(true);
            var program = compiler.Compile(mainModule, modules);
            return program.Run();
        }


        [Fact]
        public void VariableDeclaration()
        {
            var mainModule = "module MainModule; import System; using System; var a = 1;";
            var result = ExecModules("MainModule", new[] { mainModule });
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
        }
        
        [Fact]
        public void LoadModulesInDependencyOrder()
        {
            var mainModule = "module MainModule; import SubModule; using SubModule; var a = SubModule.a; a;";
            var subModule = "module SubModule; import SubSubModule; using SubSubModule; var a = SubSubModule.a;";
            var subSubModule = "module SubSubModule; var a = 12345;";
            var result = ExecModules("MainModule", new[] { subSubModule, subModule, mainModule });
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(12345, result.ReturnValue.NumberData);
        }

        [Fact]
        public void LoadModulesInReversedDependencyOrder()
        {
            var mainModule = "module MainModule; import SubModule; using SubModule; var a = SubModule.a; a;";
            var subModule = "module SubModule; import SubSubModule; using SubSubModule; var a = SubSubModule.a;";
            var subSubModule = "module SubSubModule; var a = 12345;";
            var result = ExecModules("MainModule", new[] { mainModule, subModule, subSubModule });
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(12345, result.ReturnValue.NumberData);
        }
        
        [Fact]
        public void ReferenceStringsFromSubmodule()
        {
            var mainModule = "module MainModule; import SubModule; using SubModule; var greeting = SubModule.a + \" \" + SubModule.b; greeting;";
            var subModule = "module SubModule; var a = \"Hello\"; var b = \"World!\";";
            var result = ExecModules("MainModule", new[] { mainModule, subModule });
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal("Hello World!", result.ReturnValue.StringData);
        }
    }
}
