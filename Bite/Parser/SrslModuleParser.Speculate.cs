using System;
using System.Collections.Generic;
using System.Globalization;
using Bite.Ast;

namespace Bite.Parser
{
    public partial class SrslModuleParser
    {
        #region Public

        private bool speculate<TNode>(Func<IContext<TNode>> rule) where TNode : HeteroAstNode
        {
            bool success = true;
            mark();

            var context = rule();

            if (context.Failed)
            {
                success = false;
            }

            release();

            return success;
        }
        
        public virtual bool speculate_assignment_assignment()
        {
            // Console.WriteLine( "attempt alternative assignment assignment" );
            return speculate(assignment_assignment);
        }

        public virtual bool speculate_block()
        {
            // Console.WriteLine( "attempt alternative block" );
            return speculate(block);
        }

        public virtual bool speculate_call()
        {
            // Console.WriteLine( "attempt alternative call" );
            return speculate(call);
        }

        public virtual bool speculate_declaration_class()
        {
            // Console.WriteLine( "attempt alternative class" );
            return speculate(classDeclaration);
        }

        public virtual bool speculate_declaration_class_forward()
        {
            // Console.WriteLine( "attempt alternative class forward" );
            return speculate(classDeclarationForward);
        }

        public virtual bool speculate_declaration_class_instance()
        {
            // Console.WriteLine( "attempt alternative class instance declaration" );
            return speculate(classInstanceDeclaration);
        }

        public virtual bool speculate_declaration_function()
        {
            // Console.WriteLine( "attempt alternative function" );
            return speculate(functionDeclaration);
        }

        public virtual bool speculate_declaration_function_forward()
        {
            // Console.WriteLine( "attempt alternative function forward" );
            return speculate(functionDeclarationForward);
        }

        public virtual bool speculate_declaration_struct()
        {
            // Console.WriteLine( "attempt alternative struct" );
            return speculate(structDeclaration);
        }

        public virtual bool speculate_declaration_variable()
        {
            // Console.WriteLine( "attempt alternative variable declaration" );
            return speculate(variableDeclaration);
        }

        public virtual bool speculate_expression()
        {
            // Console.WriteLine( "attempt alternative expression" );
            return speculate(expression);
        }

        public virtual bool speculate_expression_statement()
        {
            // Console.WriteLine( "attempt alternative expression statement" );
            return speculate(expressionStatement);
        }

        public virtual bool speculate_for_statement()
        {
            // Console.WriteLine( "attempt alternative for statement" );
            return speculate(forStatement);
        }

        public virtual bool speculate_if_statement()
        {
            // Console.WriteLine( "attempt alternative if statement" );
            return speculate(ifStatement);
        }

        public virtual bool speculate_logicOr()
        {
            // Console.WriteLine( "attempt alternative logicOr assignment" );
            return speculate(logicOr);
        }

        public virtual bool speculate_module()
        {
            // Console.WriteLine( "attempt alternative assignment assignment" );
            return speculate(module);
        }

        public virtual bool speculate_return_statement()
        {
            // Console.WriteLine( "attempt alternative return statement" );
            return speculate(returnStatement);
        }
        
        public virtual bool speculate_break_statement()
        {
            // Console.WriteLine( "attempt alternative return statement" );
            return speculate(breakStatement);
        }

        public virtual bool speculate_statement()
        {
            // Console.WriteLine( "attempt alternative statement" );
            return speculate(statement);
        }

        public virtual bool speculate_ternary()
        {
            // Console.WriteLine( "attempt alternative ternary" );
            return speculate(ternary);
        }

        public virtual bool speculate_unary_postfix()
        {
            // Console.WriteLine( "attempt alternative unary postfix" );
            return speculate(unaryPostfix);
        }

        public virtual bool speculate_unary_prefix()
        {
            // Console.WriteLine( "attempt alternative unary prefix" );
            return speculate(unaryPrefix);
        }

        public bool speculate_using_statement()
        {
            // Console.WriteLine( "attempt alternative expression statement" );
            return speculate(usingStatement);
        }

        public virtual bool speculate_while_statement()
        {
            // Console.WriteLine( "attempt alternative while statement" );
            return speculate(whileStatement);
        }

        #endregion
    }

}
