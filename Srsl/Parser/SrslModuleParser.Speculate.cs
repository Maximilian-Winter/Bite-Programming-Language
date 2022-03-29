using System;
using System.Collections.Generic;
using System.Globalization;
using Srsl.Ast;

namespace Srsl.Parser
{
    public partial class SrslModuleParser
    {
        #region Public
        
        public virtual bool speculate_assignment_assignment()
        {
            // Console.WriteLine( "attempt alternative assignment assignment" );
            bool success = true;
            mark();

            try
            {
                assignment_assignment();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_block()
        {
            // Console.WriteLine( "attempt alternative block" );
            bool success = true;
            mark();

            try
            {
                block();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_call()
        {
            // Console.WriteLine( "attempt alternative call" );
            bool success = true;
            mark();

            try
            {
                call();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_declaration_class()
        {
            // Console.WriteLine( "attempt alternative class" );
            bool success = true;
            mark();

            try
            {
                classDeclaration();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_declaration_class_forward()
        {
            // Console.WriteLine( "attempt alternative class forward" );
            bool success = true;
            mark();

            try
            {
                classDeclarationForward();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_declaration_class_instance()
        {
            // Console.WriteLine( "attempt alternative class instance declaration" );
            bool success = true;
            mark();

            try
            {
                classInstanceDeclaration();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_declaration_function()
        {
            // Console.WriteLine( "attempt alternative function" );
            bool success = true;
            mark();

            try
            {
                functionDeclaration();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_declaration_function_forward()
        {
            // Console.WriteLine( "attempt alternative function forward" );
            bool success = true;
            mark();

            try
            {
                functionDeclarationForward();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_declaration_struct()
        {
            // Console.WriteLine( "attempt alternative struct" );
            bool success = true;
            mark();

            try
            {
                structDeclaration();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_declaration_variable()
        {
            // Console.WriteLine( "attempt alternative variable declaration" );
            bool success = true;
            mark();

            try
            {
                variableDeclaration();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_expression()
        {
            // Console.WriteLine( "attempt alternative expression" );
            bool success = true;
            mark();

            try
            {
                expression();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_expression_statement()
        {
            // Console.WriteLine( "attempt alternative expression statement" );
            bool success = true;
            mark();

            try
            {
                expressionStatement();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_for_statement()
        {
            // Console.WriteLine( "attempt alternative for statement" );
            bool success = true;
            mark();

            try
            {
                forStatement();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_if_statement()
        {
            // Console.WriteLine( "attempt alternative if statement" );
            bool success = true;
            mark();

            try
            {
                ifStatement();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_logicOr()
        {
            // Console.WriteLine( "attempt alternative logicOr assignment" );
            bool success = true;
            mark();

            try
            {
                logicOr();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_module()
        {
            // Console.WriteLine( "attempt alternative assignment assignment" );
            bool success = true;
            mark();

            try
            {
                module();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_return_statement()
        {
            // Console.WriteLine( "attempt alternative return statement" );
            bool success = true;
            mark();

            try
            {
                returnStatement();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_statement()
        {
            // Console.WriteLine( "attempt alternative statement" );
            bool success = true;
            mark();

            try
            {
                statement();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_ternary()
        {
            // Console.WriteLine( "attempt alternative ternary" );
            bool success = true;
            mark();

            try
            {
                ternary();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_unary_postfix()
        {
            // Console.WriteLine( "attempt alternative unary postfix" );
            bool success = true;
            mark();

            try
            {
                unaryPostfix();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_unary_prefix()
        {
            // Console.WriteLine( "attempt alternative unary prefix" );
            bool success = true;
            mark();

            try
            {
                unaryPrefix();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public bool speculate_using_statement()
        {
            // Console.WriteLine( "attempt alternative expression statement" );
            bool success = true;
            mark();

            try
            {
                usingStatement();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        public virtual bool speculate_while_statement()
        {
            // Console.WriteLine( "attempt alternative while statement" );
            bool success = true;
            mark();

            try
            {
                whileStatement();
            }
            catch (RecognitionException)
            {
                success = false;
            }

            release();

            return success;
        }

        #endregion
    }

}
