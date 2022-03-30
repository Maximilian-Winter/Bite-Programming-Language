using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Srsl.Ast;

namespace Srsl.Parser
{
    public partial class SrslModuleParser
    {
        #region Public

        public virtual IContext<ExpressionNode> additive()
        {
            return ProcessRule("additive", _additive);
        }

        public virtual IContext<ArgumentsNode> arguments()
        {
            return ProcessRule("arguments", _arguments);
        }

        public virtual IContext<AssignmentNode> assignment()
        {
            return ProcessRule("assignment", _assignment);
        }

        public virtual IContext<AssignmentNode> assignment_assignment()
        {
            return ProcessRule("assignment_assignment", _assignment_assignment);
        }

        public virtual IContext<BlockStatementNode> block()
        {
            return ProcessRule("block", _block);
        }

        public virtual IContext<CallNode> call()
        {
            return ProcessRule("call", _call);
        }

        public virtual IContext<ClassDeclarationNode> classDeclaration()
        {
            return ProcessRule("classDeclaration", _classDeclaration);
        }

        public virtual IContext<ClassDeclarationNode> classDeclarationForward()
        {
            return ProcessRule("classDeclarationForward", _classDeclarationForward);
        }

        public virtual IContext<ClassInstanceDeclarationNode> classInstanceDeclaration()
        {
            return ProcessRule("classInstanceDeclaration", _classInstanceDeclaration);
        }

        public virtual IContext<HeteroAstNode> declaration()
        {
            return ProcessRule("declaration", _declaration);
        }

        public virtual IContext<ExpressionNode> equality()
        {
            return ProcessRule("equality", _equality);
        }

        public virtual IContext<ExpressionNode> expression()
        {
            return ProcessRule("expression", _expression);
        }

        public virtual IContext<ExpressionStatementNode> expressionStatement()
        {
            return ProcessRule("expressionStatement", _expressionStatement);
        }

        public virtual IContext<ForStatementNode> forStatement()
        {
            return ProcessRule("forStatement", _forStatement);
        }

        public virtual IContext<FunctionDeclarationNode> functionDeclaration()
        {
            return ProcessRule("functionDeclaration", _functionDeclaration);
        }

        public virtual IContext<FunctionDeclarationNode> functionDeclarationForward()
        {
            return ProcessRule("functionDeclarationForward", _functionDeclarationForward);
        }

        public virtual IContext<IfStatementNode> ifStatement()
        {
            return ProcessRule("ifStatement", _ifStatement);
        }

        public virtual IContext<ExpressionNode> logicAnd()
        {
            return ProcessRule("logicAnd", _logicAnd);
        }

        public virtual IContext<ExpressionNode> ternary()
        {
            return ProcessRule("ternary", _ternary);
        }

        public virtual IContext<ExpressionNode> bitwiseOr()
        {
            return ProcessRule("bitwiseOr", _bitwiseOr);
        }

        public virtual IContext<ExpressionNode> bitwiseXor()
        {
            return ProcessRule("bitwiseXor", _bitwiseXor);
        }

        public virtual IContext<ExpressionNode> bitwiseAnd()
        {
            return ProcessRule("bitwiseAnd", _bitwiseAnd);
        }

        public virtual IContext<ExpressionNode> shift()
        {
            return ProcessRule("shift", _shift);
        }

        public virtual IContext<ExpressionNode> logicOr()
        {
            return ProcessRule("logicOr", _logicOr);
        }

        public virtual IContext<ExpressionNode> multiplicative()
        {
            return ProcessRule("multiplicative", _multiplicative);
        }

        public virtual IContext<PrimaryNode> primary()
        {
            return ProcessRule("primary", _primary);
        }

        public virtual List<IContext<StatementNode>> statements()
        {
            List<StatementNode> statements = new List<StatementNode>();

            while (LA(1) != Lexer.EOF_TYPE)
            {
                var context = declaration();

                if (context.Failed)
                {
                    return new List<IContext<StatementNode>>()
                     {
                         Context<StatementNode>.AsFailed(context.Exception)
                     };
                }

                HeteroAstNode decl = context.Result;

                switch (decl)
                {
                    case ClassDeclarationNode classDeclarationNode:
                        statements.Add(classDeclarationNode);
                        break;
                    case FunctionDeclarationNode functionDeclarationNode:
                        statements.Add(functionDeclarationNode);
                        break;
                    case BlockStatementNode block:
                        statements.Add(block);
                        break;
                    case StructDeclarationNode structDeclaration:
                        statements.Add(structDeclaration);
                        break;
                    case VariableDeclarationNode variable:
                        statements.Add(variable);
                        break;
                    case ClassInstanceDeclarationNode classInstance:
                        statements.Add(classInstance);
                        break;
                    case StatementNode statement:
                        statements.Add(statement);
                        break;
                }
            }
            return statements.Select(s => new Context<StatementNode>(s)).ToList<IContext<StatementNode>>();
        }

        public virtual IContext<ModuleNode> module()
        {
            return ProcessRule("module", _module);
        }

        public virtual IContext<ExpressionNode> relational()
        {
            return ProcessRule("comparision", _relational);
        }

        public virtual IContext<ReturnStatementNode> returnStatement()
        {
            return ProcessRule("returnStatement", _returnStatement);
        }

        public virtual IContext<HeteroAstNode> statement()
        {
            return ProcessRule("statement", _statement);
        }

        public virtual IContext<StructDeclarationNode> structDeclaration()
        {
            return ProcessRule("structDeclaration", _structDeclaration);
        }

        public virtual IContext<ExpressionNode> unary()
        {
            return ProcessRule("unary", _unary);
        }

        public virtual IContext<ExpressionNode> unaryPostfix()
        {
            return ProcessRule("unaryPostfix", _unaryPostfix);
        }

        public virtual IContext<ExpressionNode> unaryPrefix()
        {
            return ProcessRule("unaryPrefix", _unaryPrefix);
        }

        public IContext<UsingStatementNode> usingStatement()
        {
            return ProcessRule("usingStatement", _usingStatement);
        }

        public virtual IContext<VariableDeclarationNode> variableDeclaration()
        {
            return ProcessRule("variableDeclaration", _variableDeclaration);
        }

        public virtual IContext<WhileStatementNode> whileStatement()
        {
            return ProcessRule("whileStatement", _whileStatement);
        }

        #endregion

    }

}
