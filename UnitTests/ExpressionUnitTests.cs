using Bite.Parser;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Bite.Runtime.Memory;
using Xunit;

namespace UnitTests
{

    public class ExpressionUnitTests
    {

        private BiteResult ExecExpression(string expression)
        {
            var compiler = new Compiler(true);
            var program = compiler.CompileExpression(expression);
            return program.Run();
        }

        #region Arithmetic Tests

        [Fact]
        public void ArithmeticAddNumbers()
        {
            var result = ExecExpression("1 + 1");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(2, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticSubtractNumbers()
        {
            var result = ExecExpression("6 - 3");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(3, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticMultiplyNumbers()
        {
            var result = ExecExpression("4 * 4");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(16, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticDivideNumbers()
        {
            var result = ExecExpression("1 / 2");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(0.5, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticMultipleAddition()
        {
            var result = ExecExpression("1 + 2 + 3");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(6, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticOperatorPrecedenceMultiplyBeforeAdd()
        {
            var result = ExecExpression("1 + 2 * 3");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(7, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticOperatorPrecedenceMultiplyBeforeAddOuterFirst()
        {
            var result = ExecExpression("1 * 2 + 3 * 4");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(14, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticOperatorPrecedenceDivideBeforeSubtract()
        {
            var result = ExecExpression("1 - 3 / 3");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(0, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticOperatorPrecedence()
        {
            var result = ExecExpression("2 * 5 - 4 / 2 + 6");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(14, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticParenthesesPrecedenceGroupByAdditionSubtraction()
        {
            var result = ExecExpression("2 * (5 - 4) / (2 + 6)");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(0.25, result.ReturnValue.NumberData);
        }

        [Fact]
        public void ArithmeticParenthesesPrecedenceGroupByMultiplicationDivision()
        {
            var result = ExecExpression("(2 * 5) - (4 / 2) + 6");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(14, result.ReturnValue.NumberData);
        }

        #endregion

        #region Logical Tests

        [Fact]
        public void LogicalAnd()
        {
            {
                var result = ExecExpression("true && false");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("false && true");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("false && false");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("true && true");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
        }

        [Fact]
        public void LogicalOr()
        {
            {
                var result = ExecExpression("true || false");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("false || true");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("false || false");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("true || true");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
        }


        [Fact]
        public void LogicalNot()
        {
            {
                var result = ExecExpression("!true");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("!false");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
        }

        #endregion

        #region Bitwise Tests


        [Fact]
        public void BitwiseAnd()
        {
            var result = ExecExpression("5 & 3");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(1, result.ReturnValue.NumberData);
        }

        [Fact]
        public void BitwiseOr()
        {
            var result = ExecExpression("5 | 3");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(7, result.ReturnValue.NumberData);
        }

        [Fact]
        public void BitwiseXor()
        {
            var result = ExecExpression("5 ^ 3");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(6, result.ReturnValue.NumberData);
        }

        [Fact]
        public void BitwiseLeftShift()
        {
            var result = ExecExpression("2 << 4");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(32, result.ReturnValue.NumberData);
        }

        [Fact]
        public void BitwiseRightShift()
        {
            var result = ExecExpression("64 >> 3");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(8, result.ReturnValue.NumberData);
        }

        [Fact]
        public void BitwiseCompliment()
        {
            var result = ExecExpression("~127");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(-128, result.ReturnValue.NumberData);
        }

        #endregion

        #region Unary Operator Tests

        [Fact]
        public void Negate()
        {
            var result = ExecExpression("-127");
            Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
            Assert.Equal(-127, result.ReturnValue.NumberData);
        }

        #endregion

        #region Comparison Tests

        [Fact]
        public void Equal()
        {
            {
                var result = ExecExpression("3 == 3");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("3 == 4");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
        }

        [Fact]
        public void NotEqual()
        {
            {
                var result = ExecExpression("3 != 3");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("3 != 4");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
        }

        [Fact]
        public void LessThan()
        {
            {
                var result = ExecExpression("1 < 2");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("2 < 1");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
        }


        [Fact]
        public void LessThanOrEqual()
        {
            {
                var result = ExecExpression("1 <= 1");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("0 <= 1");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("1 <= 2");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("2 <= 1");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
        }


        [Fact]
        public void GreaterThan()
        {
            {
                var result = ExecExpression("2 > 1");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("1 > 2");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
        }


        [Fact]
        public void GreaterThanOrEqual()
        {
            {
                var result = ExecExpression("1 >= 1");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("1 >= 0");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("2 >= 1");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.True, result.ReturnValue.DynamicType);
            }
            {
                var result = ExecExpression("1 >= 2");
                Assert.Equal(BiteVmInterpretResult.InterpretOk, result.InterpretResult);
                Assert.Equal(DynamicVariableType.False, result.ReturnValue.DynamicType);
            }
        }

        #endregion

    }
}
