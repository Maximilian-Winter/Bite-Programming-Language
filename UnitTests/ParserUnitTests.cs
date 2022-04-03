using System;
using Bite.Ast;
using Bite.Parser;
using Xunit;

namespace UnitTests
{

public class ParserUnitTests
{
    private ExpressionNode ParseExpression( string expression )
    {
        BiteParser parser = new BiteParser();

        return parser.ParseExpression( expression );
    }

    private void AssertInt( ExpressionNode expression, int value )
    {
        PrimaryNode primary = ( ( CallNode ) expression ).Primary;
        Assert.Equal( PrimaryNode.PrimaryTypes.IntegerLiteral, primary.PrimaryType );
        Assert.Equal( value, primary.IntegerLiteral );
    }

    private void AssertCallExpression( ExpressionNode expression, Action < ExpressionNode > assertPrimary )
    {
        PrimaryNode primary = ( ( CallNode ) expression ).Primary;
        Assert.Equal( PrimaryNode.PrimaryTypes.Expression, primary.PrimaryType );
        assertPrimary( ( ExpressionNode ) primary.Expression );
    }

    private void AssertBinary(
        ExpressionNode expression,
        Action < ExpressionNode > assertLeft,
        Action < ExpressionNode > assertRight,
        BinaryOperationNode.BinaryOperatorType operatorType )
    {
        BinaryOperationNode binaryExpression = expression as BinaryOperationNode;

        assertLeft( binaryExpression.LeftOperand );
        assertRight( binaryExpression.RightOperand );
        Assert.Equal( operatorType, binaryExpression.Operator );
    }

    [Fact]
    public void ArithmeticAddNumbers()
    {
        ExpressionNode result = ParseExpression( "1 + 1" );

        AssertBinary(
            result.Assignment.Binary,
            expression => AssertInt( expression, 1 ),
            expression => AssertInt( expression, 1 ),
            BinaryOperationNode.BinaryOperatorType.Plus
        );
    }

    [Fact]
    public void ArithmeticDivideNumbers()
    {
        ExpressionNode result = ParseExpression( "1 / 2" );

        AssertBinary(
            result.Assignment.Binary,
            expression => AssertInt( expression, 1 ),
            expression => AssertInt( expression, 2 ),
            BinaryOperationNode.BinaryOperatorType.Div
        );
    }

    [Fact]
    public void ArithmeticMultipleAddition()
    {
        ExpressionNode result = ParseExpression( "1 + 2 + 3" );

        AssertBinary(
            result.Assignment.Binary,
            exp1 => AssertInt( exp1, 1 ),
            expression => AssertBinary(
                expression,
                exp1 => AssertInt( exp1, 2 ),
                exp1 => AssertInt( exp1, 3 ),
                BinaryOperationNode.BinaryOperatorType.Plus
            ),
            BinaryOperationNode.BinaryOperatorType.Plus
        );
    }

    [Fact]
    public void ArithmeticMultiplyNumbers()
    {
        ExpressionNode result = ParseExpression( "4 * 4" );

        AssertBinary(
            result.Assignment.Binary,
            expression => AssertInt( expression, 4 ),
            expression => AssertInt( expression, 4 ),
            BinaryOperationNode.BinaryOperatorType.Mult
        );
    }

    [Fact]
    public void ArithmeticOperatorPrecedence()
    {
        ExpressionNode result = ParseExpression( "2 * 5 - 4 / 2 + 6" );

        AssertBinary(
            result.Assignment.Binary,
            expression => AssertBinary(
                expression,
                exp1 => AssertInt( exp1, 2 ),
                exp1 => AssertInt( exp1, 5 ),
                BinaryOperationNode.BinaryOperatorType.Mult
            ),
            expression => AssertBinary(
                expression,
                exp1 => AssertBinary(
                    exp1,
                    exp2 => AssertInt( exp2, 4 ),
                    exp2 => AssertInt( exp2, 2 ),
                    BinaryOperationNode.BinaryOperatorType.Div ),
                exp1 => AssertInt( exp1, 6 ),
                BinaryOperationNode.BinaryOperatorType.Plus
            ),
            BinaryOperationNode.BinaryOperatorType.Minus
        );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceDivideBeforeSubtract()
    {
        ExpressionNode result = ParseExpression( "1 - 3 / 3" );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceMultiplyBeforeAdd()
    {
        ExpressionNode result = ParseExpression( "1 + 2 * 3" );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceMultiplyBeforeAddOuterFirst()
    {
        ExpressionNode result = ParseExpression( "1 * 2 + 3 * 4" );

        AssertBinary(
            result.Assignment.Binary,
            expression => AssertBinary(
                expression,
                exp1 => AssertInt( exp1, 1 ),
                exp1 => AssertInt( exp1, 2 ),
                BinaryOperationNode.BinaryOperatorType.Mult
            ),
            expression => AssertBinary(
                expression,
                exp1 => AssertInt( exp1, 3 ),
                exp1 => AssertInt( exp1, 4 ),
                BinaryOperationNode.BinaryOperatorType.Mult
            ),
            BinaryOperationNode.BinaryOperatorType.Plus
        );
    }

    [Fact]
    public void ArithmeticParenthesesPrecedenceGroupByAdditionSubtraction()
    {
        ExpressionNode result = ParseExpression( "2 * (5 - 4) / (2 + 6)" );

        AssertBinary(
            result.Assignment.Binary,
            expression => AssertInt( expression, 2 ),
            expression => AssertBinary(
                expression,
                exp1 => AssertCallExpression(
                    exp1,
                    exp2 =>
                        AssertBinary(
                            exp2,
                            exp3 => AssertInt( exp3, 5 ),
                            exp3 => AssertInt( exp3, 4 ),
                            BinaryOperationNode.BinaryOperatorType.Minus )
                ),
                exp1 => AssertBinary(
                    exp1,
                    exp2 => AssertInt( exp2, 2 ),
                    exp2 => AssertInt( exp2, 6 ),
                    BinaryOperationNode.BinaryOperatorType.Plus ),
                BinaryOperationNode.BinaryOperatorType.Div ),
            BinaryOperationNode.BinaryOperatorType.Mult
        );
    }

    [Fact]
    public void ArithmeticParenthesesPrecedenceGroupByMultiplicationDivision()
    {
        ExpressionNode result = ParseExpression( "(2 * 5) - (4 / 2) + 6" );

        AssertBinary(
            result.Assignment.Binary,
            expression => AssertBinary(
                expression,
                exp1 => AssertInt( exp1, 2 ),
                exp1 => AssertInt( exp1, 5 ),
                BinaryOperationNode.BinaryOperatorType.Mult
            ),
            expression => AssertBinary(
                expression,
                exp1 => AssertBinary(
                    exp1,
                    exp2 => AssertInt( exp2, 4 ),
                    exp2 => AssertInt( exp2, 2 ),
                    BinaryOperationNode.BinaryOperatorType.Div ),
                exp1 => AssertInt( exp1, 6 ),
                BinaryOperationNode.BinaryOperatorType.Plus
            ),
            BinaryOperationNode.BinaryOperatorType.Minus
        );
    }

    [Fact]
    public void ArithmeticSubtractNumbers()
    {
        ExpressionNode result = ParseExpression( "6 - 3" );

        AssertBinary(
            result.Assignment.Binary,
            expression => AssertInt( expression, 6 ),
            expression => AssertInt( expression, 3 ),
            BinaryOperationNode.BinaryOperatorType.Minus
        );
    }
}

}
