using Srsl.Parser;
using Srsl.Runtime;
using Srsl.Runtime.CodeGen;
using Xunit;

namespace UnitTests
{

    public class InterpreterUnitTests
    {

        private ExecResult ExecExpression(string expression)
        {
            var parser = new BiteParser();

            var expressionNode = parser.ParseExpression(expression);

            var generator = new CodeGenerator();

            var context = generator.CompileExpression(expressionNode);

            var srslVm = new BiteVm();

            var result = srslVm.Interpret(context);

            return new ExecResult()
            {
                InterpretResult = result,
                LastValue = srslVm.RetVal
            };
        }

        [Fact]
        public void AddNumbers()
        {
            var result = ExecExpression("1 + 1");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(2, result.LastValue.NumberData);
        }

        [Fact]
        public void SubtractNumbers()
        {
            var result = ExecExpression("6 - 3");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(3, result.LastValue.NumberData);
        }

        [Fact]
        public void MultiplyNumbers()
        {
            var result = ExecExpression("4 * 4");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(16, result.LastValue.NumberData);
        }

        [Fact]
        public void DivideNumbers()
        {
            var result = ExecExpression("1 / 2");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(0.5, result.LastValue.NumberData);
        }

        [Fact]
        public void MultipleAddition()
        {
            var result = ExecExpression("1 + 2 + 3");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(6, result.LastValue.NumberData);
        }

        [Fact]
        public void OperatorPrecedenceMultiplyBeforeAdd()
        {
            var result = ExecExpression("1 + 2 * 3");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(7, result.LastValue.NumberData);
        }

        [Fact]
        public void OperatorPrecedenceMultiplyBeforeAddOuterFirst()
        {
            var result = ExecExpression("1 * 2 + 3 * 4");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(14, result.LastValue.NumberData);
        }

        [Fact]
        public void OperatorPrecedenceDivideBeforeSubtract()
        {
            var result = ExecExpression("1 - 3 / 3");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(0, result.LastValue.NumberData);
        }

        [Fact]
        public void OperatorPrecedence()
        {
            var result = ExecExpression("2 * 5 - 4 / 2 + 6");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(14, result.LastValue.NumberData);
        }

        [Fact]
        public void ParenthesesPrecedenceGroupByAdditionSubtraction()
        {
            var result = ExecExpression("2 * (5 - 4) / (2 + 6)");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(0.25, result.LastValue.NumberData);
        }

        [Fact]
        public void ParenthesesPrecedenceGroupByMultiplicationDivision()
        {
            var result = ExecExpression("(2 * 5) - (4 / 2) + 6");
            Assert.Equal(SrslVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(14, result.LastValue.NumberData);
        }
    }
}
