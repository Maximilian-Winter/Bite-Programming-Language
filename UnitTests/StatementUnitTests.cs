#define USE_NEW_PARSER

using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Xunit;

namespace UnitTests
{

public class StatementUnitTests
{
    private BiteResult ExecStatements( string statements )
    {
#if USE_NEW_PARSER
        var compiler = new BITECompiler();
#else
        var compiler = new Compiler( true );
#endif
        BiteProgram program = compiler.CompileStatements( statements );

        return program.Run();
    }

    [Fact]
    public void AfterMultiArgumentPostFix()
    {
        string statements = @"
            function foo(a, b){
              return a + b;
            }
            var a = 1;
            var b = 2;
            foo(a++, b++);
            a + b;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void AfterMultiPostFix()
    {
        string statements = @"
            var a = 1;
            var b = 2;
            a++; 
            b++;
            a + b;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticAssignment()
    {
        string statements = @"var a = 1;
            a += 2;
            a;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticAssignmentAssignment()
    {
        string statements = @"var a = 1;
            var b = a += 2;
            b;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassConstructorArguments()
    {
        string statements = @"class TestClass
            {
                var x = 5;

                function TestClass(n)
                {
                    x = n;
                }
            }

            var a = new TestClass(150);

            a.x;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 150, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassFields()
    {
        string statements = @"class TestClass
            {
                var x = 5;
            }

            var a = new TestClass();

            a.x;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassInstanceAsArgument()
    {
        string statements = @"class TestClass
            {
                var x = 5;
                function TestClass(n)
                {
                    x = n;
                }
            }

            function TestFunction(n)
            {
                n.x = 10;
            }

            var a = new TestClass(150);

            TestFunction(a);

            a.x;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassFunctionCall()
    {
        string statements = @"class TestClass
            {
                var x = 5;

                function TestClass()
                {
                }

                function foo() {
                    x++;
                }

                function bar() {
                    x--;
                }
            }

            var a = new TestClass();

            a.foo();
            a.foo();
            a.bar();
            
            a.x;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void DynamicMemberFunctionCall()
    {
        string statements = @"class TestClass
            {
                var x = 5;

                function TestClass()
                {
                }

                function foo() {
                    x++;
                }

                function bar() {
                    x--;
                }
            }

            var a = new TestClass();

            a[""TestClass.foo""]();
            
            a.x;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void DynamicMemberGet()
    {
        string statements = @"class TestClass
            {
                var x = 5;
                function TestClass(n)
                {
                    x = n;
                }
            }

            var a = new TestClass(150);
            
            a[""x""];";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 150, result.ReturnValue.NumberData );
    }

    [Fact]
    public void DynamicMemberSet()
    {
        string statements = @"class TestClass
            {
                var x = 5;
                function TestClass(n)
                {
                    x = n;
                }
            }

            var a = new TestClass(150);
            
            a[""x""] = 10;

            a.x;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void DynamicMemberSetWithVariable()
    {
        string statements = @"class TestClass
            {
                var x = 5;
                function TestClass(n)
                {
                    x = n;
                }
            }

            var a = new TestClass(150);
            var propName = ""x"";

            a[propName] = 10;

            a.x;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForEverLoopBreak()
    {
        BiteResult result = ExecStatements( "var i = 0; for (;;) { i++; if (i == 10) { break; } } i;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForInitLoopBreak()
    {
        var result = ExecStatements( "var a = 0; for (var i = 0;;) { i++; a++; if (i == 10) { break; } } a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForLoop()
    {
        BiteResult result = ExecStatements( "var j = 0; for (var i = 0; i < 10; i++) { j++; } j;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForLoopBreak()
    {
        BiteResult result =
            ExecStatements( "var j = 0; for (var i = 0; i < 10; i++) { j++; if (j == 6) { break; } } j;" );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForLoopDecrement()
    {
        BiteResult result = ExecStatements( "var j = 0; for (var i = 10; i > 0; i--) { j++; } j;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForLoopScope()
    {
        var result = ExecStatements( "var j = 0; for (var i = 0; i < 10; i++) { j++; i++; } j;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForMultipleVarAndIterator()
    {
        var result = ExecStatements( "var a = 0; for (var i = 0, j = 0; i < 10; i++, j++ ) { a += j + i; } a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 90, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionCall()
    {
        BiteResult result = ExecStatements( "function foo(a) { a = a + 1; } foo(1);" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void FunctionCallNoReturnShouldThrowErrorWhenAssigned()
    {
        // Or assign null or undefined?
        BiteResult result = ExecStatements( "function foo(a) { a = a + 1; } var a = 1; a = foo(a);" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void FunctionCallUseBeforeDeclaration()
    {
        BiteResult result = ExecStatements(
            "var a = foo(1); function foo(a) { return a + bar(a); } function bar(a) { return a + a; } a;" );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionCallWith3Arguments()
    {
        BiteResult result = ExecStatements( "function foo(a, b, c) { return a + b + c; } var a = foo(1,2,3); a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionCallWithMultipleArguments()
    {
        BiteResult result = ExecStatements( "function foo(a, b) { return a + b; } var a = foo(1, 2); a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionCallWithReturn()
    {
        BiteResult result = ExecStatements( "function foo(a) { a = a + 1; return a; } var a = foo(1); a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 2, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionDefinition()
    {
        BiteResult result = ExecStatements( "function foo(a) { a = a + 1; }" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void FunctionDefinitionWithReturn()
    {
        BiteResult result = ExecStatements( "function foo(a) { a = a + 1; return a; }" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void If()
    {
        BiteResult result = ExecStatements( "var a = true; var b = 0; if (a) { b = 123; } b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseFalse()
    {
        BiteResult result = ExecStatements( "var a = false; var b = 0; if (a) { b = 123; } else { b = 456; } b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseIfElse()
    {
        BiteResult result = ExecStatements(
            "var a = \"fee\"; var b = \"beer\"; var c = 789; if (a == \"foo\") { c = 123; } else if (b == \"bar\") { c = 456; } else { c = 111; } c;" );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 111, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseIfFalseThenFalse()
    {
        BiteResult result = ExecStatements(
            "var a = \"fee\"; var b = \"beer\"; var c = 789; if (a == \"foo\") { c = 123; } else if (b == \"bar\") { c = 456; } c;" );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 789, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseIfFalseThenTrue()
    {
        BiteResult result = ExecStatements(
            "var a = \"fee\"; var b = \"bar\"; var c = 789; if (a == \"foo\") { c = 123; } else if (b == \"bar\") { c = 456; } c;" );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseIfTrue()
    {
        BiteResult result = ExecStatements(
            "var a = \"foo\"; var b = \"bar\"; var c = 789; if (a == \"foo\") { c = 123; } else if (b == \"bar\") { c = 456; } c;" );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseTrue()
    {
        BiteResult result = ExecStatements( "var a = true; var b = 0; if (a) { b = 123; } else { b = 456; } b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseWithoutBracesFalse()
    {
        BiteResult result = ExecStatements( "var a = true; var b = 0; if (a) b = 123; else b = 456; b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseWithoutBracesTrue()
    {
        BiteResult result = ExecStatements( "var a = true; var b = 0; if (a) b = 123; else b = 456; b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfWithoutBraces()
    {
        BiteResult result = ExecStatements( "var a = true; var b = 0; if (a) b = 123; b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void MultiArgumentPostFix()
    {
        string statements = @"
            function foo(a, b){
              return a + b;
            }
            var a = 1;
            var b = 2;
            foo(a++, b++);";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void NestedFunctionCall()
    {
        BiteResult result = ExecStatements(
            "function bar(a) { return a + a; } function foo(a) { return a + bar(a); } var a = foo(1); a;" );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void NestedFunctionCallReverseDeclaration()
    {
        BiteResult result = ExecStatements(
            "function foo(a) { return a + bar(a); } function bar(a) { return a + a; } var a = foo(1); a;" );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixArgumentMultiple()
    {
        string statements = @"
            function foo(a){
              // nop
            }
            var a = 1;
            foo(a++);
            foo(a++);
            foo(a++);
            foo(a++);
            a;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixAssignment()
    {
        string statements = @"var a = 1;
            var b = a++;
            b;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostfixDecrement()
    {
        BiteResult result = ExecStatements( "var a = 7; a--;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 7, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostfixDecrementAsArgument()
    {
        BiteResult result = ExecStatements( "function fn(a) { return a; } var a = 456; var b = fn(a++); b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixInBinaryOperationAssignment()
    {
        string statements = @"var a = 1;
            var b = a++ + 2;
            b;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostfixIncrement()
    {
        BiteResult result = ExecStatements( "var a = 7; a++;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 7, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostfixIncrementAsArgument()
    {
        BiteResult result = ExecStatements( "function fn(a) { return a; } var a = 456; var b = fn(a++); b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixMultiple()
    {
        string statements = @"var a = 1;
            a++;
            a++;
            a++;
            a++;
            a;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixReturn()
    {
        // While this code probably doesn't make sense, 
        // i.e. you never return a postix, it is still valid code
        BiteResult result = ExecStatements( "var a = 1; a++;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixReturnFromFunction()
    {
        // While this code probably doesn't make sense, 
        // i.e. you never return a postix, it is still valid code
        BiteResult result = ExecStatements( "function fn(a) { return a++; } fn(1);" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PrefixDecrement()
    {
        BiteResult result = ExecStatements( "var a = 7; --a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PrefixDecrementAsArgument()
    {
        BiteResult result = ExecStatements( "function fn(a) { return a; } var a = 456; var b = fn(--a); b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 455, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PrefixIncrement()
    {
        BiteResult result = ExecStatements( "var a = 7; ++a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 8, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PrefixIncrementAsArgument()
    {
        BiteResult result = ExecStatements( "function fn(a) { return a; } var a = 456; var b = fn(++a); b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 457, result.ReturnValue.NumberData );
    }

    [Fact]
    public void TernaryFalse()
    {
        BiteResult result = ExecStatements( "var a = false; var b = a ? 123 : 456; b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void TernaryTrue()
    {
        BiteResult result = ExecStatements( "var a = true; var b = a ? 123 : 456; b;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void VariableDeclaration()
    {
        BiteResult result = ExecStatements( "var a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void VariableDefintition()
    {
        BiteResult result = ExecStatements( "var a = 12345; a;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 12345, result.ReturnValue.NumberData );
    }

    [Fact]
    public void WhileFalseNeverExecutes()
    {
        BiteResult result = ExecStatements( "var i = 12345; while(false) { i++; } i;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 12345, result.ReturnValue.NumberData );
    }

    [Fact]
    public void WhileLoop()
    {
        BiteResult result = ExecStatements( "var i = 0; while(i < 10) { i++; } i;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void WhileTrueLoopBreak()
    {
        BiteResult result = ExecStatements( "var i = 0; while(true) { i++; if (i == 5) { break; } } i;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }
}

}