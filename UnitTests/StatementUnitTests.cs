using System.Collections.Generic;
using Bite.Compiler;
using Bite.Runtime;
using Bite.Runtime.CodeGen;
using Bite.Symbols;
using Xunit;

namespace UnitTests
{

public class StatementUnitTests
{
    private BiteResult ExecStatements( string statements, Dictionary < string, object > externalObjects = null )
    {
        var compiler = new BiteCompiler();

        BiteProgram program = compiler.CompileStatements( statements );

        return program.Run( externalObjects );
    }

    [Fact]
    public void ArrayInitializers()
    {
        string statements = @"
            function foo() {
                return 5;
            }

            var a = [ 10, 20, 30, ""Hello"", foo(), foo ];";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void DictionaryInitializers()
    {
        string statements = @"
            function foo() {
                return 5;
            }

            var a = {
                x: 10,
                y: 20,
                z: 30,
                a: ""Hello"",
                b: foo(),
                c: foo 
            };";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
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
    public void ClassConstructorMultipleArguments()
    {
        string statements = @"class TestClass
            {
                var x = 0;
                var y = 0;
                var z = 0;

                function TestClass( a, b, c )
                {
                    x = a;
                    y = b;
                    z = c;
                }
            }

            var a = new TestClass(1, 3, 7);

            a.x + a.y + a.z;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 11, result.ReturnValue.NumberData );
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
    public void ClassFieldsAssignment()
    {
        string statements = @"class TestClass
            {
                var x;
                var y;
            }

            var a = new TestClass();

            function foo () {
               return 5;
            }

            a.x = foo();
            a.y = ""five"";

            a.x + a.y;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "5five", result.ReturnValue.StringData );
    }

    [Fact]
    public void ClassExpressionAssignment()
    {
        string statements = @"class Foo {
              var x = 5;
            }

            class Bar {
               var foo;
            }

            var a = new Bar();

            a.foo = new Foo();

            a.foo.x;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassExpressionArgument()
    {
        string statements = @"class Foo {
              var x = 5;
            }

            function bar(foo) {
                return foo.x;
            }

            var a = bar(new Foo());
            
            a;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

        [Fact]
    public void ClassFieldsInitializers()
    {
        string statements = @"class TestClass
            {
                var x;
                var y;
            }

            function foo () {
               return 5;
            }

            var a = new TestClass() {
                x = ""Hello"",
                y = 10,
                z = foo()
            }

            a.x + a.y + a.z;";

        BiteResult result = ExecStatements( statements );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello105", result.ReturnValue.StringData );
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

            a[""foo""]();
            
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
    public void AddTwoNumbers()
    {
        string statements = @"var c = 1 + 2; c;";

        BiteResult result = ExecStatements( statements,
            new Dictionary<string, object>()
            {
                { "a", 1 },
                { "b", 2 }
            } );

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ExternalValues()
    {
        string statements = @"var c = a + b; c;";

        BiteResult result = ExecStatements( statements,
            new Dictionary < string, object >()
            {
                { "a", 1 },
                { "b", 2 }
            });

        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
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
    public void FunctionCallUseBeforeDeclarationShouldThrowException()
    {
        Assert.Throws < BiteSymbolTableException >( () =>
        {
            BiteResult result = ExecStatements(
                "var a = foo(1); function foo(a) { return a + bar(a); } function bar(a) { return a + a; } a;" );

        } );
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
        Assert.Throws < BiteSymbolTableException >( () =>
        {
            ExecStatements(
                "function foo(a) { return a + bar(a); } function bar(a) { return a + a; } var a = foo(1); a;" );
        } );
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

    //[Fact]
    //Corner Case
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

    //[Fact]
    //Corner Case
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

    //[Fact]
    //Corner Case
    public void PostFixReturn()
    {
        // While this code probably doesn't make sense, 
        // i.e. you never return a postix, it is still valid code
        BiteResult result = ExecStatements( "var a = 1; a++;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    //[Fact]
    //Corner Case
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
    public void NonStringInterpolation()
    {
        BiteResult result = ExecStatements( @"
            var Hello = ""Hello""; 
            var World = ""World""; 
            var s = ""\${Hello} \${World}"";
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "${Hello} ${World}", result.ReturnValue.StringData );
    }


    [Fact]
    public void StringInterpolationStringExpressions()
    {
        BiteResult result = ExecStatements( @"
            var Hello = ""Hello""; 
            var World = ""World""; 
            var s = ""${Hello} ${World}"";
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolationStringLiteralsWithClosingCurlyBrace()
    {
        BiteResult result = ExecStatements( @"
            var s = ""${""Hello}""} ${""{World""}"";
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello} {World", result.ReturnValue.StringData );
    }


        [Fact]
    public void StringInterpolationNumericExpressions()
    {
        BiteResult result = ExecStatements( @"
            var X = 200; 
            var Y = 100; 
            var s = ""${X}:${Y}"";
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "200:100", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolationFunctionExpressions()
    {
        BiteResult result = ExecStatements( @"
            function foo(a) {
                return a + 1;
            }

            function bar(b) {
                return b + 2;
            }

            var X = 200; 
            var Y = 100; 
            var s = ""${foo(X)}:${bar(Y)}"";
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "201:102", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolationAsFunctionArgument()
    {
        BiteResult result = ExecStatements( @"

            function bar(b) {
                return b + "" World"";
            }

            var Hello = ""Hello"";
            var s = bar(""${Hello}"");
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolatioWithStringLiterals()
    {
        BiteResult result = ExecStatements( @"
            var s = ""${""Hello""} ${""World""}"";
            s;" );
            Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolatioWithStringLiteralConcatenation()
    {
        BiteResult result = ExecStatements( @"
            var s = ""${""Hello"" + "" "" + ""World""}"";
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolatioWithMultiLine()
    {
        BiteResult result = ExecStatements( @"
            var s = ""${""Hello"" 
                    + "" "" 
                    + ""World""}"";
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolatioInsideStringInterpolation()
    {
        BiteResult result = ExecStatements( @"
            var x = ""Inception"";
            var s = ""${""${x}"" + "" "" + ""World""}"";
            s;" );
        Assert.Equal( BiteVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Inception World", result.ReturnValue.StringData );
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