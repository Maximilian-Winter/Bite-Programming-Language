using System;
using System.Collections.Generic;
using System.Globalization;
using Srsl.Ast;

namespace Srsl.Parser
{
    public partial class SrslModuleParser
    {
        #region Public
        public IContext<UsingStatementNode> usingStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<UsingStatementNode>("usingStatement");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _usingStatement();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "usingStatement", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> additive()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("additive");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _additive();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "additive", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ArgumentsNode> arguments()
        {
            bool failed = false;
            int startTokenIndex = index();
            
            var speculateResult = SpeculateRule<ArgumentsNode>("arguments");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _arguments();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "arguments", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<AssignmentNode> assignment()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<AssignmentNode>("assignment");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _assignment();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "assignment", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<AssignmentNode> assignment_assignment()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<AssignmentNode>("assignment_assignment");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _assignment_assignment();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "assignment_assignment", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<BlockStatementNode> block()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<BlockStatementNode>("block");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _block();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "block", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<CallNode> call()
        {
            bool failed = false;
            int startTokenIndex = index();


            var speculateResult = SpeculateRule<CallNode>("call");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _call();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "call", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ClassDeclarationNode> classDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ClassDeclarationNode>("classDeclaration");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _classDeclaration();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "classDeclaration", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ClassDeclarationNode> classDeclarationForward()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ClassDeclarationNode>("classDeclarationForward");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _classDeclarationForward();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "classDeclarationForward", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ClassInstanceDeclarationNode> classInstanceDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ClassInstanceDeclarationNode>("classInstanceDeclaration");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _classInstanceDeclaration();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "classInstanceDeclaration", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<HeteroAstNode> declaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<HeteroAstNode>("declaration");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _declaration();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "declaration", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> equality()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("equality");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _equality();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "equality", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> expression()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("expression");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _expression();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "expression", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionStatementNode> expressionStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionStatementNode>("expressionStatement");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _expressionStatement();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "expressionStatement", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ForStatementNode> forStatement()
        {
            bool failed = false;
            int startTokenIndex = index();


            var speculateResult = SpeculateRule<ForStatementNode>("forStatement");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _forStatement();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "forStatement", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<FunctionDeclarationNode> functionDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<FunctionDeclarationNode>("functionDeclaration");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _functionDeclaration();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "functionDeclaration", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<FunctionDeclarationNode> functionDeclarationForward()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<FunctionDeclarationNode>("functionDeclarationForward");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _functionDeclarationForward();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "functionDeclarationForward", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<IfStatementNode> ifStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<IfStatementNode>("ifStatement");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _ifStatement();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "ifStatement", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> logicAnd()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("logicAnd");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _logicAnd();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "logicAnd", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> ternary()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("ternary");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _ternary();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "ternary", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> bitwiseOr()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("bitwiseOr");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _bitwiseOr();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "bitwiseOr", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> bitwiseXor()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("bitwiseXor");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _bitwiseXor();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "bitwiseXor", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> bitwiseAnd()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("bitwiseAnd");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _bitwiseAnd();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "bitwiseAnd", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> shift()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("shift");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _shift();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "shift", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> logicOr()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("logicOr");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _logicOr();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "logicOr", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> multiplicative()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("multiplicative");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _multiplicative();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "multiplicative", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<PrimaryNode> primary()
        {
            bool failed = false;
            int startTokenIndex = index();
            
            var speculateResult = SpeculateRule<PrimaryNode>("primary");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _primary();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "primary", startTokenIndex, failed);
                }
            }
        }

        public virtual List<StatementNode> statements()
        {
            List<StatementNode> statements = new List<StatementNode>();
            while (LA(1) != Lexer.EOF_TYPE)
            {
                HeteroAstNode decl = declaration();

                if (decl is ClassDeclarationNode classDeclarationNode)
                {
                    statements.Add(classDeclarationNode);
                }

                else if (decl is FunctionDeclarationNode functionDeclarationNode)
                {
                    statements.Add(functionDeclarationNode);
                }

                else if (decl is BlockStatementNode block)
                {
                    statements.Add(block);
                }

                else if (decl is StructDeclarationNode structDeclaration)
                {
                    statements.Add(structDeclaration);
                }

                else if (decl is VariableDeclarationNode variable)
                {
                    statements.Add(variable);
                }

                else if (decl is ClassInstanceDeclarationNode classInstance)
                {
                    statements.Add(classInstance);
                }

                else if (decl is StatementNode statement)
                {
                    statements.Add(statement);
                }
            }
            return statements;
        }

        public virtual IContext<ModuleNode> module()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ModuleNode>("module");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }

            try
            {
                return _module();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "module", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> relational()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("comparision");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _relational();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "comparision", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ReturnStatementNode> returnStatement()
        {
            bool failed = false;
            int startTokenIndex = index();


            var speculateResult = SpeculateRule<ReturnStatementNode>("returnStatement");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _returnStatement();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "returnStatement", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<HeteroAstNode> statement()
        {
            bool failed = false;
            int startTokenIndex = index();
            
            var speculateResult = SpeculateRule<HeteroAstNode>("statement");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _statement();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "statement", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<StructDeclarationNode> structDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<StructDeclarationNode>("structDeclaration");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _structDeclaration();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "structDeclaration", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> unary()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("unary");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _unary();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "unary", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> unaryPostfix()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("unaryPostfix");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _unaryPostfix();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "unaryPostfix", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<ExpressionNode> unaryPrefix()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<ExpressionNode>("unaryPrefix");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _unaryPrefix();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "unaryPrefix", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<VariableDeclarationNode> variableDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<VariableDeclarationNode>("variableDeclaration");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _variableDeclaration();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "variableDeclaration", startTokenIndex, failed);
                }
            }
        }

        public virtual IContext<WhileStatementNode> whileStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            var speculateResult = SpeculateRule<WhileStatementNode>("whileStatement");

            if (speculateResult.ShouldReturn)
            {
                return speculateResult.Context;
            }
            
            try
            {
                return _whileStatement();
            }
            catch (RecognitionException re)
            {
                failed = true;

                throw re;
            }
            finally
            {
                if (Speculating)
                {
                    memoize(MemoizingDictionary, "whileStatement", startTokenIndex, failed);
                }
            }
        }

        #endregion

    }

}
