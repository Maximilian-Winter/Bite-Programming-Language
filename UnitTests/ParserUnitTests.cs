using System;
using System.IO;
using Bite.Ast;
using Bite.Parser;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Bite.Runtime.Memory;
using Xunit;

namespace UnitTests
{

    public class ParserUnitTests
    {
        private ExpressionNode ParseExpression(string expression)
        {
            var parser = new BiteParser();

            return parser.ParseExpression(expression);
        }

        private void AssertInt(ExpressionNode expression, int value)
        {
            var primary = ((CallNode)expression).Primary;
            Assert.Equal(PrimaryNode.PrimaryTypes.IntegerLiteral, primary.PrimaryType);
            Assert.Equal(value, primary.IntegerLiteral);
        }

        private void AssertCallExpression(ExpressionNode expression, Action<ExpressionNode> assertPrimary)
        {
            var primary = ((CallNode)expression).Primary;
            Assert.Equal(PrimaryNode.PrimaryTypes.Expression, primary.PrimaryType);
            assertPrimary((ExpressionNode)primary.Expression);
        }

        private void AssertBinary(ExpressionNode expression, Action<ExpressionNode> assertLeft, Action<ExpressionNode> assertRight, BinaryOperationNode.BinaryOperatorType operatorType)
        {
            var binaryExpression = expression as BinaryOperationNode;

            assertLeft(binaryExpression.LeftOperand);
            assertRight(binaryExpression.RightOperand);
            Assert.Equal(operatorType, binaryExpression.Operator);
        }

        [Fact]
        public void AddNumbers()
        {
            var result = ParseExpression("1 + 1");

            AssertBinary(result.Assignment.Binary,
                expression => AssertInt(expression, 1),
                expression => AssertInt(expression, 1),
                BinaryOperationNode.BinaryOperatorType.Plus
                );
        }

        [Fact]
        public void SubtractNumbers()
        {
            var result = ParseExpression("6 - 3");

            AssertBinary(result.Assignment.Binary,
                expression => AssertInt(expression, 6),
                expression => AssertInt(expression, 3),
                BinaryOperationNode.BinaryOperatorType.Minus
            );
        }

        [Fact]
        public void MultiplyNumbers()
        {
            var result = ParseExpression("4 * 4");

            AssertBinary(result.Assignment.Binary,
                expression => AssertInt(expression, 4),
                expression => AssertInt(expression, 4),
                BinaryOperationNode.BinaryOperatorType.Mult
            );
        }

        [Fact]
        public void DivideNumbers()
        {
            var result = ParseExpression("1 / 2");

            AssertBinary(result.Assignment.Binary,
                expression => AssertInt(expression, 1),
                expression => AssertInt(expression, 2),
                BinaryOperationNode.BinaryOperatorType.Div
            );
        }

        [Fact]
        public void MultipleAddition()
        {
            var result = ParseExpression("1 + 2 + 3");

            AssertBinary(result.Assignment.Binary,
                exp1 => AssertInt(exp1, 1),
                expression => AssertBinary(expression,
                    exp1 => AssertInt(exp1, 2),
                    exp1 => AssertInt(exp1, 3),
                    BinaryOperationNode.BinaryOperatorType.Plus
                    ),
                BinaryOperationNode.BinaryOperatorType.Plus
            );
        }

        [Fact]
        public void OperatorPrecedenceMultiplyBeforeAdd()
        {
            var result = ParseExpression("1 + 2 * 3");
        }

        [Fact]
        public void OperatorPrecedenceMultiplyBeforeAddOuterFirst()
        {
            var result = ParseExpression("1 * 2 + 3 * 4");

            AssertBinary(result.Assignment.Binary,
                expression => AssertBinary(expression,
                    exp1 => AssertInt(exp1, 1),
                    exp1 => AssertInt(exp1, 2),
                    BinaryOperationNode.BinaryOperatorType.Mult
                    ),
                 expression => AssertBinary(expression,
                    exp1 => AssertInt(exp1, 3),
                    exp1 => AssertInt(exp1, 4),
                    BinaryOperationNode.BinaryOperatorType.Mult
                    ),
                BinaryOperationNode.BinaryOperatorType.Plus
            );
        }

        [Fact]
        public void OperatorPrecedenceDivideBeforeSubtract()
        {
            var result = ParseExpression("1 - 3 / 3");
        }

        [Fact]
        public void OperatorPrecedence()
        {
            var result = ParseExpression("2 * 5 - 4 / 2 + 6");

            AssertBinary(result.Assignment.Binary,
                expression => AssertBinary(expression,
                    exp1 => AssertInt(exp1, 2),
                    exp1 => AssertInt(exp1, 5),
                    BinaryOperationNode.BinaryOperatorType.Mult
                ),
                expression => AssertBinary(expression,
                    exp1 => AssertBinary(exp1,
                        exp2 => AssertInt(exp2, 4),
                        exp2 => AssertInt(exp2, 2),
                        BinaryOperationNode.BinaryOperatorType.Div),
                    exp1 => AssertInt(exp1, 6),
                    BinaryOperationNode.BinaryOperatorType.Plus
                ),
                BinaryOperationNode.BinaryOperatorType.Minus
            );
        }

        [Fact]
        public void ParenthesesPrecedenceGroupByAdditionSubtraction()
        {
            var result = ParseExpression("2 * (5 - 4) / (2 + 6)");

            AssertBinary(result.Assignment.Binary,
                expression => AssertInt(expression, 2),
                expression => AssertBinary(expression,
                    exp1 => AssertCallExpression(exp1, (exp2) =>
                        AssertBinary(exp2,
                            exp3 => AssertInt(exp3, 5),
                            exp3 => AssertInt(exp3, 4),
                            BinaryOperationNode.BinaryOperatorType.Minus)
                    ),
                    exp1 => AssertBinary(exp1,
                        exp2 => AssertInt(exp2, 2),
                        exp2 => AssertInt(exp2, 6),
                        BinaryOperationNode.BinaryOperatorType.Plus)
                    ,
                    BinaryOperationNode.BinaryOperatorType.Div),
                BinaryOperationNode.BinaryOperatorType.Mult
            );
        }

        [Fact]
        public void ParenthesesPrecedenceGroupByMultiplicationDivision()
        {
            var result = ParseExpression("(2 * 5) - (4 / 2) + 6");

            AssertBinary(result.Assignment.Binary,
                expression => AssertBinary(expression,
                    exp1 => AssertInt(exp1, 2),
                    exp1 => AssertInt(exp1, 5),
                    BinaryOperationNode.BinaryOperatorType.Mult
                ),
                expression => AssertBinary(expression,
                    exp1 => AssertBinary(exp1,
                        exp2 => AssertInt(exp2, 4),
                        exp2 => AssertInt(exp2, 2),
                        BinaryOperationNode.BinaryOperatorType.Div),
                    exp1 => AssertInt(exp1, 6),
                    BinaryOperationNode.BinaryOperatorType.Plus
                ),
                BinaryOperationNode.BinaryOperatorType.Minus
            );
        }
    }
}