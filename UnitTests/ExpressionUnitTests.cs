using Bite.Compiler;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Bite.Runtime.Memory;
using Xunit;

namespace UnitTests
{

public class ExpressionUnitTests
{
    private BiteResult ExecExpression( string expression )
    {
        BiteCompiler compiler = new BiteCompiler();

        BiteProgram program = compiler.CompileExpression( expression );

        return program.Run();
    }

    [Fact]
    public void ArithmeticAddNumbers()
    {
        BiteResult result = ExecExpression( "1 + 1" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 2, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticDivideNumbers()
    {
        BiteResult result = ExecExpression( "1 / 2" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 0.5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticMultipleAddition()
    {
        BiteResult result = ExecExpression( "1 + 2 + 3" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticMultiplyNumbers()
    {
        BiteResult result = ExecExpression( "4 * 4" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 16, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticOperatorPrecedence()
    {
        BiteResult result = ExecExpression( "2 * 5 - 4 / 2 + 6" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 14, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceDivideBeforeSubtract()
    {
        BiteResult result = ExecExpression( "1 - 3 / 3" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 0, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceMultiplyBeforeAdd()
    {
        BiteResult result = ExecExpression( "1 + 2 * 3" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 7, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceMultiplyBeforeAddOuterFirst()
    {
        BiteResult result = ExecExpression( "1 * 2 + 3 * 4" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 14, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticParenthesesPrecedenceGroupByAdditionSubtraction()
    {
        BiteResult result = ExecExpression( "2 * (5 - 4) / (2 + 6)" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 0.25, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticParenthesesPrecedenceGroupByMultiplicationDivision()
    {
        BiteResult result = ExecExpression( "(2 * 5) - (4 / 2) + 6" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 14, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticSubtractNumbers()
    {
        BiteResult result = ExecExpression( "6 - 3" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseAnd()
    {
        BiteResult result = ExecExpression( "5 & 3" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseCompliment()
    {
        BiteResult result = ExecExpression( "~127" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( -128, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseLeftShift()
    {
        BiteResult result = ExecExpression( "2 << 4" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 32, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseOr()
    {
        BiteResult result = ExecExpression( "5 | 3" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 7, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseRightShift()
    {
        BiteResult result = ExecExpression( "64 >> 3" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 8, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseXor()
    {
        BiteResult result = ExecExpression( "5 ^ 3" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void Equal()
    {
        {
            BiteResult result = ExecExpression( "3 == 3" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "3 == 4" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void GreaterThan()
    {
        {
            BiteResult result = ExecExpression( "2 > 1" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "1 > 2" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void GreaterThanOrEqual()
    {
        {
            BiteResult result = ExecExpression( "1 >= 1" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "1 >= 0" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "2 >= 1" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "1 >= 2" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LessThan()
    {
        {
            BiteResult result = ExecExpression( "1 < 2" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "2 < 1" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LessThanOrEqual()
    {
        {
            BiteResult result = ExecExpression( "1 <= 1" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "0 <= 1" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "1 <= 2" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "2 <= 1" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LogicalAnd()
    {
        {
            BiteResult result = ExecExpression( "true && false" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "false && true" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "false && false" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "true && true" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LogicalNot()
    {
        {
            BiteResult result = ExecExpression( "!true" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "!false" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LogicalOr()
    {
        {
            BiteResult result = ExecExpression( "true || false" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "false || true" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "false || false" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "true || true" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void Negate()
    {
        BiteResult result = ExecExpression( "-127" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( -127, result.ReturnValue.NumberData );
    }

    [Fact]
    public void NotEqual()
    {
        {
            BiteResult result = ExecExpression( "3 != 3" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            BiteResult result = ExecExpression( "3 != 4" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void String()
    {
        {
            BiteResult result = ExecExpression( "\"Hello World\"" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( "Hello World", result.ReturnValue.StringData );
        }
    }

    [Fact]
    public void StringConcatenation()
    {
        {
            BiteResult result = ExecExpression( "\"Hello\" + \" \" + \"World\"" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( "Hello World", result.ReturnValue.StringData );
        }
    }
}

}
