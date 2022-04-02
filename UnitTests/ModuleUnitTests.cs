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
            var mainModule = "module MainModule; import System; using system; var a = 1;";
            var result = ExecModules("MainModule", new[] { mainModule });
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
        }
    }
}
