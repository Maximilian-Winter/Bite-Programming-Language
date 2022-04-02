using Bite.Parser;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Xunit;

namespace UnitTests
{

    public class StatementUnitTests
    {

        private BiteResult ExecStatements(string statements)
        {
            var compiler = new Compiler(true);
            var program = compiler.CompileStatements(statements);
            return program.Run();
        }


        [Fact]
        public void VariableDeclaration()
        {
            var result = ExecStatements("var a;");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
        }

        [Fact]
        public void VariableDefintition()
        {
            var result = ExecStatements("var a = 12345; a;");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(12345, result.ReturnValue.NumberData);
        }



        #region Unary Operator Tests

        [Fact]
        public void PrefixIncrement()
        {
            var result = ExecStatements("var a = 7; ++a;");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(8, result.ReturnValue.NumberData);
        }

        [Fact]
        public void PrefixDecrement()
        {
            var result = ExecStatements("var a = 7; --a;");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(6, result.ReturnValue.NumberData);
        }

        [Fact]
        public void PostfixIncrement()
        {
            var result = ExecStatements("var a = 7; a++;");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(8, result.ReturnValue.NumberData);
        }

        [Fact]
        public void PostfixDecrement()
        {
            var result = ExecStatements("var a = 7; a--;");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(6, result.ReturnValue.NumberData);
        }

        #endregion
    }
}
