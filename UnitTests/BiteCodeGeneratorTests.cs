// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Bite.NewCodeGen;
using Bite.Compiler;
using Bite.Runtime;
using Xunit;

namespace UnitTests
{
    public class BiteCodeGeneratorTests
    {
        private readonly BiteCompiler m_Compiler = new BiteCompiler();
        private readonly BiteVmCore m_Vm = new BiteVmCore();
        private readonly BiteCodeGenerator m_CodeGen = new BiteCodeGenerator();

        [Fact]
        public void CodeGen_EmptyProgram()
        {
            string code = @"
                module Test;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            
            Assert.NotNull(program);
            Assert.True(program.Chunks.Count > 0);
        }

        [Fact]
        public void CodeGen_ConstantNumber()
        {
            string code = @"
                module Test;
                42;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
        }

        [Fact]
        public void CodeGen_LocalVariable()
        {
            string code = @"
                module Test;
                var x = 10;
                x;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
        }

        [Fact]
        public void CodeGen_ArithmeticExpression()
        {
            string code = @"
                module Test;
                10 + 20 * 3;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
        }

        [Fact]
        public void CodeGen_VariableAssignment()
        {
            string code = @"
                module Test;
                var x = 5;
                x = 10;
                x;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
        }

        [Fact]
        public void CodeGen_IfStatement()
        {
            string code = @"
                module Test;
                var x = 0;
                if (true) {
                    x = 1;
                } else {
                    x = 2;
                }
                x;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
        }

        [Fact]
        public void CodeGen_WhileLoop()
        {
            string code = @"
                module Test;
                var i = 0;
                var sum = 0;
                while (i < 5) {
                    sum = sum + i;
                    i = i + 1;
                }
                sum;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
            // sum should be 0+1+2+3+4 = 10
            Assert.Equal(10.0, m_Vm.LastResult.AsNumber);
        }

        [Fact]
        public void CodeGen_ReturnStatement()
        {
            string code = @"
                module Test;
                return 42;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            
            var result = m_Vm.Execute(program.MainChunk );
            
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(42.0, m_Vm.LastResult.AsNumber);
        }

        [Fact]
        public void CodeGen_FunctionDefinition()
        {
            string code = @"
                module Test;
                function add(a, b) {
                    return a + b;
                }
                return 0;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            
            Assert.True(program.Chunks.Count >= 2); // Module + function
        }

        [Fact]
        public void CodeGen_ComparisonOperators()
        {
            string code = @"
                module Test;
                var a = 10;
                var b = 20;
                var result = a < b;
                result;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(m_Vm.LastResult.AsBool);
        }

        [Fact]
        public void CodeGen_LogicalOperators()
        {
            string code = @"
                module Test;
                var a = true;
                var b = false;
                var result = a && !b;
                result;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(m_Vm.LastResult.AsBool);
        }

        [Fact]
        public void CodeGen_StringLiteral()
        {
            string code = @"
                module Test;
                ""hello world"";
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
        }

        [Fact]
        public void CodeGen_NullLiteral()
        {
            string code = @"
                module Test;
                null;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
            Assert.True(m_Vm.LastResult.IsNull);
        }

        [Fact]
        public void CodeGen_Negation()
        {
            string code = @"
                module Test;
                var x = 42;
                -x;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(-42.0, m_Vm.LastResult.AsNumber);
        }

        [Fact]
        public void CodeGen_Fibonacci()
        {
            // Simple fibonacci: fib(10) = 55
            string code = @"
                module Test;
                function fib(n) {
                    if (n <= 1) {
                        return n;
                    }
                    return fib(n - 1) + fib(n - 2);
                }
                return 0; // TODO: Call fib when Call is implemented
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            
            // Just verify it compiles for now
            Assert.True(program.Chunks.Count >= 2);
        }

        [Fact]
        public void CodeGen_NestedBlocks()
        {
            string code = @"
                module Test;
                var x = 0;
                {
                    var y = 10;
                    x = y;
                }
                x;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(10.0, m_Vm.LastResult.AsNumber);
        }

        [Fact]
        public void CodeGen_BitwiseOperators()
        {
            string code = @"
                module Test;
                var a = 6;   // 110
                var b = 3;   // 011
                var result = a & b;  // 010 = 2
                result;
            ";
            
            var ast = m_Compiler.ParseModule(code);
            var program = m_CodeGen.Compile(ast);
            var result = m_Vm.Execute(program.MainChunk);
            
            Assert.Equal(BiteVmResult.Success, result);
            Assert.Equal(2.0, m_Vm.LastResult.AsNumber);
        }
    }
}

