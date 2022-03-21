using MemoizeSharp;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Srsl_Parser
{

    public class SrslModuleParser : Parser
    {
        public readonly IDictionary<int, IDictionary<string, int>> MemoizingDictionary =
            new Dictionary<int, IDictionary<string, int>>();

        private bool MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;
        #region Public

        public SrslModuleParser(Lexer input) : base(input)
        {
        }

        public virtual ExpressionNode _additive()
        {
            if (Speculating)
            {
                multiplicative();

                while (LA(1) == SrslLexer.MinusOperator || LA(1) == SrslLexer.PlusOperator)
                {
                    consume();
                    multiplicative();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = multiplicative();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.MinusOperator || LA(1) == SrslLexer.PlusOperator)
                {
                    if (LA(1) == SrslLexer.PlusOperator)
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Plus;
                    }
                    else
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Minus;
                    }

                    consume();

                    ExpressionNode expressionNode = multiplicative();

                    if (LA(1) == SrslLexer.MinusOperator || LA(1) == SrslLexer.PlusOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ArgumentsNode _arguments()
        {
            if (Speculating)
            {
                if (LA(1) != SrslLexer.ClosingRoundBracket)
                {
                    while (LA(1) != SrslLexer.ClosingRoundBracket)
                    {
                        if (LA(1) == SrslLexer.ReferenceOperator)
                        {
                            consume();
                        }

                        expression();

                        if (LA(1) == SrslLexer.CommaSeperator)
                        {
                            consume();
                        }
                    }
                }
            }
            else
            {
                if (LA(1) != SrslLexer.ClosingRoundBracket)
                {
                    ArgumentsNode argumentsNode = null;
                    int counter = 0;

                    while (LA(1) != SrslLexer.ClosingRoundBracket)
                    {
                        if (counter == 0)
                        {
                            argumentsNode = new ArgumentsNode();
                            argumentsNode.Expressions = new List<ExpressionNode>();
                            argumentsNode.IsReference = new List<bool>();
                        }

                        if (LA(1) == SrslLexer.ReferenceOperator)
                        {
                            argumentsNode.IsReference.Add(true);
                            consume();
                        }
                        else
                        {
                            argumentsNode.IsReference.Add(false);
                        }

                        ExpressionNode expressionNode = expression();

                        argumentsNode.Expressions.Add(expressionNode);

                        if (LA(1) == SrslLexer.CommaSeperator)
                        {
                            consume();
                        }

                        counter++;
                    }

                    return argumentsNode;
                }
            }

            return null;
        }

        public virtual AssignmentNode _assignment()
        {
            if (speculate_assignment_assignment())
            {
                // Console.WriteLine( "predict assignment assignment" );

                return assignment_assignment();
            }

            if (speculate_ternary())
            {
                // Console.WriteLine( "predict logic or assignment" );
                AssignmentNode assignmentNode = new AssignmentNode();
                HeteroAstNode node = ternary();

                if (node is AssignmentNode assignment)
                {
                    assignmentNode.Type = AssignmentTypes.Assignment;
                    assignmentNode.Call = assignment.Call;
                    assignmentNode.OperatorType = assignment.OperatorType;
                    assignmentNode.Assignment = assignment;
                }

                if (node is BinaryOperationNode binaryOperationNode)
                {
                    assignmentNode.Type = AssignmentTypes.Binary;
                    assignmentNode.Binary = binaryOperationNode;
                }

                if (node is TernaryOperationNode ternaryOperationNode)
                {
                    assignmentNode.Type = AssignmentTypes.Ternary;
                    assignmentNode.Ternary = ternaryOperationNode;
                }

                if (node is UnaryPrefixOperation unaryPrefixOperation)
                {
                    assignmentNode.Type = AssignmentTypes.UnaryPrefix;
                    assignmentNode.UnaryPrefix = unaryPrefixOperation;
                }

                if (node is UnaryPostfixOperation unaryPostfixOperation)
                {
                    assignmentNode.Type = AssignmentTypes.UnaryPostfix;
                    assignmentNode.UnaryPostfix = unaryPostfixOperation;
                }

                if (node is CallNode callNode)
                {
                    assignmentNode.Type = AssignmentTypes.Call;
                    assignmentNode.Call = callNode;
                }

                if (node is PrimaryNode primaryNode)
                {
                    assignmentNode.Type = AssignmentTypes.Primary;
                    assignmentNode.PrimaryNode = primaryNode;
                }

                return assignmentNode;
            }

            throw new NoViableAltException("");
        }

        public virtual AssignmentNode _assignment_assignment()
        {
            if (Speculating)
            {
                call();

                if (LA(1) == SrslLexer.AssignOperator ||
                     LA(1) == SrslLexer.MultiplyAssignOperator ||
                     LA(1) == SrslLexer.DivideAssignOperator ||
                     LA(1) == SrslLexer.MinusAssignOperator ||
                     LA(1) == SrslLexer.PlusAssignOperator ||
                     LA(1) == SrslLexer.MinusAssignOperator ||
                     LA(1) == SrslLexer.ModuloAssignOperator ||
                     LA(1) == SrslLexer.BitwiseAndAssignOperator ||
                     LA(1) == SrslLexer.BitwiseOrAssignOperator ||
                     LA(1) == SrslLexer.BitwiseXorAssignOperator ||
                     LA(1) == SrslLexer.BitwiseLeftShiftAssignOperator ||
                     LA(1) == SrslLexer.BitwiseRightShiftAssignOperator)
                {
                    consume();
                    assignment();
                }
                else
                {
                    throw new NoViableAltException(LT(1).text);
                }
            }
            else
            {
                AssignmentNode assignmentNode = new AssignmentNode();
                assignmentNode.Call = call();
                assignmentNode.Type = AssignmentTypes.Assignment;

                switch (LA(1))
                {
                    case SrslLexer.AssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.Assign;

                        break;

                    case SrslLexer.MultiplyAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.MultAssign;

                        break;

                    case SrslLexer.DivideAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.DivAssign;

                        break;

                    case SrslLexer.MinusAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.MinusAssign;

                        break;

                    case SrslLexer.PlusAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.PlusAssign;

                        break;

                    case SrslLexer.ModuloAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.ModuloAssignOperator;

                        break;

                    case SrslLexer.BitwiseAndAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.BitwiseAndAssignOperator;

                        break;

                    case SrslLexer.BitwiseOrAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.BitwiseOrAssignOperator;

                        break;

                    case SrslLexer.BitwiseXorAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.BitwiseXorAssignOperator;

                        break;

                    case SrslLexer.BitwiseLeftShiftAssignOperator:
                        assignmentNode.OperatorType = AssignmentOperatorTypes.BitwiseLeftShiftAssignOperator;

                        break;

                    case SrslLexer.BitwiseRightShiftAssignOperator:
                        assignmentNode.OperatorType =
                            AssignmentOperatorTypes.BitwiseRightShiftAssignOperator;

                        break;

                    default:
                        throw new NoViableAltException(LT(1).text);
                }

                consume();

                assignmentNode.Assignment = assignment();

                return assignmentNode;
            }

            return null;
        }

        public virtual BlockStatementNode _block()
        {
            match(SrslLexer.OpeningCurlyBracket);

            if (Speculating == false)
            {
                BlockStatementNode blockStatementNode = new BlockStatementNode();
                blockStatementNode.Declarations = new DeclarationsNode();

                while (LA(1) != SrslLexer.ClosingCurlyBracket)
                {
                    HeteroAstNode decl = declaration();

                    if (decl is ClassDeclarationNode classDeclarationNode)
                    {
                        blockStatementNode.Declarations.Classes.Add(classDeclarationNode);
                    }

                    else if (decl is FunctionDeclarationNode functionDeclarationNode)
                    {
                        blockStatementNode.Declarations.Functions.Add(functionDeclarationNode);
                    }

                    else if (decl is BlockStatementNode block)
                    {
                        blockStatementNode.Declarations.Statements.Add(block);
                    }

                    else if (decl is StructDeclarationNode structDeclaration)
                    {
                        blockStatementNode.Declarations.Structs.Add(structDeclaration);
                    }

                    else if (decl is VariableDeclarationNode variable)
                    {
                        blockStatementNode.Declarations.Variables.Add(variable);
                    }

                    else if (decl is ClassInstanceDeclarationNode classInstance)
                    {
                        blockStatementNode.Declarations.ClassInstances.Add(classInstance);
                    }

                    else if (decl is StatementNode statement)
                    {
                        blockStatementNode.Declarations.Statements.Add(statement);
                    }
                }

                match(SrslLexer.ClosingCurlyBracket);

                return blockStatementNode;
            }

            while (LA(1) != SrslLexer.ClosingCurlyBracket)
            {
                declaration();
            }

            match(SrslLexer.ClosingCurlyBracket);

            return null;
        }

        public virtual CallNode _call()
        {
            if (Speculating)
            {
                primary();

                while (LA(1) == SrslLexer.DotOperator ||
                        LA(1) == SrslLexer.OpeningRoundBracket ||
                        LA(1) == SrslLexer.SquarebracketLeft)
                {
                    if (LA(1) == SrslLexer.DotOperator)
                    {
                        while (LA(1) == SrslLexer.DotOperator)
                        {
                            match(SrslLexer.DotOperator);
                            match(SrslLexer.Identifier);
                        }
                    }

                    else if (LA(1) == SrslLexer.OpeningRoundBracket)
                    {
                        match(SrslLexer.OpeningRoundBracket);

                        while (LA(1) != SrslLexer.ClosingRoundBracket)
                        {
                            if (LA(1) == SrslLexer.ReferenceOperator)
                            {
                                consume();
                            }

                            expression();

                            if (LA(1) == SrslLexer.CommaSeperator)
                            {
                                match(SrslLexer.CommaSeperator);
                            }
                        }

                        match(SrslLexer.ClosingRoundBracket);
                    }

                    else if (LA(1) == SrslLexer.SquarebracketLeft)
                    {
                        while (LA(1) == SrslLexer.SquarebracketLeft)
                        {
                            match(SrslLexer.SquarebracketLeft);

                            if (LA(1) == SrslLexer.StringLiteral ||
                                 LA(1) == SrslLexer.IntegerLiteral ||
                                 LA(1) == SrslLexer.Identifier)
                            {
                                consume();
                            }

                            match(SrslLexer.SquarebracketRight);
                        }
                    }
                }

                if (LA(1) == SrslLexer.PlusPlusOperator || LA(1) == SrslLexer.MinusMinusOperator)
                {
                    throw new MismatchedTokenException("Unary Operation! Not Call.");
                }
            }
            else
            {
                CallNode callNode = new CallNode();
                callNode.Primary = primary();
                callNode.CallType = CallTypes.Primary;
                CallEntry currentCallEntry = null;

                while (LA(1) == SrslLexer.DotOperator ||
                        LA(1) == SrslLexer.OpeningRoundBracket ||
                        LA(1) == SrslLexer.SquarebracketLeft)
                {
                    if (LA(1) == SrslLexer.DotOperator)
                    {
                        if (callNode.CallEntries == null)
                        {
                            callNode.CallEntries = new List<CallEntry>();
                        }

                        callNode.CallType = CallTypes.PrimaryCall;

                        while (LA(1) == SrslLexer.DotOperator)
                        {
                            match(SrslLexer.DotOperator);
                            PrimaryNode primaryNode = primary();
                            CallEntry callEntry = new CallEntry();
                            callEntry.Primary = primaryNode;
                            callNode.CallEntries.Add(callEntry);
                            currentCallEntry = callEntry;
                        }
                    }

                    if (LA(1) == SrslLexer.OpeningRoundBracket)
                    {
                        callNode.CallType = CallTypes.PrimaryCall;
                        match(SrslLexer.OpeningRoundBracket);

                        if (currentCallEntry != null)
                        {
                            currentCallEntry.Arguments = new ArgumentsNode();
                            currentCallEntry.Arguments.Expressions = new List<ExpressionNode>();
                            currentCallEntry.IsFunctionCall = true;
                            currentCallEntry.Arguments.IsReference = new List<bool>();
                        }
                        else
                        {
                            callNode.Arguments = new ArgumentsNode();
                            callNode.Arguments.Expressions = new List<ExpressionNode>();
                            callNode.IsFunctionCall = true;
                            callNode.Arguments.IsReference = new List<bool>();
                        }

                        while (LA(1) != SrslLexer.ClosingRoundBracket)
                        {
                            if (currentCallEntry != null)
                            {
                                if (LA(1) == SrslLexer.ReferenceOperator)
                                {
                                    currentCallEntry.Arguments.IsReference.Add(true);
                                    consume();
                                }
                                else
                                {
                                    currentCallEntry.Arguments.IsReference.Add(false);
                                }

                                currentCallEntry.Arguments.Expressions.Add(expression());
                            }
                            else
                            {
                                if (LA(1) == SrslLexer.ReferenceOperator)
                                {
                                    callNode.Arguments.IsReference.Add(true);
                                    consume();
                                }
                                else
                                {
                                    callNode.Arguments.IsReference.Add(false);
                                }

                                callNode.Arguments.Expressions.Add(expression());
                            }

                            if (LA(1) == SrslLexer.CommaSeperator)
                            {
                                match(SrslLexer.CommaSeperator);
                            }
                        }

                        match(SrslLexer.ClosingRoundBracket);
                    }

                    if (LA(1) == SrslLexer.SquarebracketLeft)
                    {
                        callNode.CallType = CallTypes.PrimaryCall;

                        if (currentCallEntry != null)
                        {
                            currentCallEntry.ElementAccess = new List<CallElementEntry>();
                        }
                        else
                        {
                            callNode.ElementAccess = new List<CallElementEntry>();
                        }

                        while (LA(1) == SrslLexer.SquarebracketLeft)
                        {
                            CallElementEntry callElementEntry = new CallElementEntry();

                            match(SrslLexer.SquarebracketLeft);

                            if (LA(1) == SrslLexer.StringLiteral)
                            {
                                callElementEntry.Identifier = LT(1).text;
                                callElementEntry.CallElementType = CallElementTypes.StringLiteral;
                                consume();
                            }
                            else if (LA(1) == SrslLexer.IntegerLiteral)
                            {
                                callElementEntry.Identifier = LT(1).text;

                                callElementEntry.CallElementType =
                                    CallElementTypes.IntegerLiteral;

                                consume();
                            }
                            else if (LA(1) == SrslLexer.Identifier)
                            {
                                callElementEntry.Call = call();
                                ;
                                callElementEntry.CallElementType = CallElementTypes.Call;
                            }

                            match(SrslLexer.SquarebracketRight);

                            if (currentCallEntry != null)
                            {
                                currentCallEntry.ElementAccess.Add(callElementEntry);
                            }
                            else
                            {
                                callNode.ElementAccess.Add(callElementEntry);
                            }
                        }
                    }
                }

                return callNode;
            }

            return null;
        }

        public virtual ClassDeclarationNode _classDeclaration()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic || LA(1) == SrslLexer.DeclareAbstract)
                {
                    consume();
                }

                match(SrslLexer.DeclareClass);
                match(SrslLexer.Identifier);

                if (LA(1) == SrslLexer.ColonOperator && LA(2) == SrslLexer.Identifier)
                {
                    match(SrslLexer.ColonOperator);
                    match(SrslLexer.Identifier);

                    while (LA(1) == SrslLexer.CommaSeperator && LA(2) == SrslLexer.Identifier)
                    {
                        match(SrslLexer.CommaSeperator);
                        match(SrslLexer.Identifier);
                    }
                }

                block();
            }
            else
            {
                Token accessMod = null;
                Token staticAbstractMod = null;
                string classIdentifier;

                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    accessMod = LT(1);
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic || LA(1) == SrslLexer.DeclareAbstract)
                {
                    staticAbstractMod = LT(1);
                    consume();
                }

                match(SrslLexer.DeclareClass);
                classIdentifier = LT(1).text;
                match(SrslLexer.Identifier);

                List<string> baseClasses = new List<string>();

                if (LA(1) == SrslLexer.ColonOperator && LA(2) == SrslLexer.Identifier)
                {
                    match(SrslLexer.ColonOperator);
                    baseClasses.Add(LT(1).text);
                    match(SrslLexer.Identifier);

                    while (LA(1) == SrslLexer.CommaSeperator && LA(2) == SrslLexer.Identifier)
                    {
                        match(SrslLexer.CommaSeperator);
                        baseClasses.Add(LT(1).text);
                        match(SrslLexer.Identifier);
                    }
                }

                ClassDeclarationNode classDeclarationNode = new ClassDeclarationNode();

                if (classIdentifier != null)
                {
                    classDeclarationNode.ClassId = new Identifier(classIdentifier);
                }
                else
                {
                    throw new Exception("Empty or null string as class identifier is forbidden!");
                }

                if (baseClasses.Count > 0)
                {
                    classDeclarationNode.Inheritance = new List<Identifier>();

                    foreach (string baseClass in baseClasses)
                    {
                        classDeclarationNode.Inheritance.Add(new Identifier(baseClass));
                    }
                }

                classDeclarationNode.Modifiers = new ModifiersNode(accessMod, staticAbstractMod);
                classDeclarationNode.BlockStatement = block();

                return classDeclarationNode;
            }

            return null;
        }

        public virtual ClassDeclarationNode _classDeclarationForward()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic || LA(1) == SrslLexer.DeclareAbstract)
                {
                    consume();
                }

                match(SrslLexer.DeclareClass);
                match(SrslLexer.Identifier);

                if (LA(1) == SrslLexer.ColonOperator && LA(2) == SrslLexer.Identifier)
                {
                    match(SrslLexer.ColonOperator);
                    match(SrslLexer.Identifier);

                    while (LA(1) == SrslLexer.CommaSeperator && LA(2) == SrslLexer.Identifier)
                    {
                        match(SrslLexer.CommaSeperator);
                        match(SrslLexer.Identifier);
                    }
                }

                match(SrslLexer.SemicolonSeperator);
            }
            else
            {
                Token accessMod = null;
                Token staticAbstractMod = null;
                string classIdentifier = null;

                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    accessMod = LT(1);
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic || LA(1) == SrslLexer.DeclareAbstract)
                {
                    staticAbstractMod = LT(1);
                    consume();
                }

                match(SrslLexer.DeclareClass);
                classIdentifier = LT(1).text;
                match(SrslLexer.Identifier);

                List<string> baseClasses = new List<string>();

                if (LA(1) == SrslLexer.ColonOperator && LA(2) == SrslLexer.Identifier)
                {
                    match(SrslLexer.ColonOperator);
                    baseClasses.Add(LT(1).text);
                    match(SrslLexer.Identifier);

                    while (LA(1) == SrslLexer.CommaSeperator && LA(2) == SrslLexer.Identifier)
                    {
                        match(SrslLexer.CommaSeperator);
                        baseClasses.Add(LT(1).text);
                        match(SrslLexer.Identifier);
                    }
                }

                ClassDeclarationNode classDeclarationNode = new ClassDeclarationNode();

                if (classIdentifier != null)
                {
                    classDeclarationNode.ClassId = new Identifier(classIdentifier);
                }
                else
                {
                    throw new Exception("Empty or null string as class identifier is forbidden!");
                }

                if (baseClasses.Count > 0)
                {
                    classDeclarationNode.Inheritance = new List<Identifier>();

                    foreach (string baseClass in baseClasses)
                    {
                        classDeclarationNode.Inheritance.Add(new Identifier(baseClass));
                    }
                }

                classDeclarationNode.Modifiers = new ModifiersNode(accessMod, staticAbstractMod);
                match(SrslLexer.SemicolonSeperator);

                return classDeclarationNode;
            }

            return null;
        }

        public virtual ClassInstanceDeclarationNode _classInstanceDeclaration()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic)
                {
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareVariable)
                {
                    match(SrslLexer.DeclareVariable);
                }

                match(SrslLexer.Identifier);
                match(SrslLexer.AssignOperator);
                match(SrslLexer.DeclareClassInstance);
                match(SrslLexer.Identifier);

                while (LA(1) == SrslLexer.DotOperator)
                {
                    match(SrslLexer.DotOperator);
                    match(SrslLexer.Identifier);
                }

                match(SrslLexer.OpeningRoundBracket);

                if (LA(1) != SrslLexer.ClosingRoundBracket)
                {
                    arguments();
                }

                match(SrslLexer.ClosingRoundBracket);

                if (MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration)
                {
                    match(SrslLexer.SemicolonSeperator);
                }

            }
            else
            {
                Token accessMod = null;
                Token staticAbstractMod = null;
                string classIdentifier = null;
                string identifier = null;

                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    accessMod = LT(1);
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic)
                {
                    staticAbstractMod = LT(1);
                    consume();
                }

                bool isRedeclaration = true;

                if (LA(1) == SrslLexer.DeclareVariable)
                {
                    isRedeclaration = false;
                    match(SrslLexer.DeclareVariable);
                }

                ClassInstanceDeclarationNode classInstanceDeclarationNode = new ClassInstanceDeclarationNode();
                classInstanceDeclarationNode.IsVariableRedeclaration = isRedeclaration;

                identifier = LT(1).text;
                match(SrslLexer.Identifier);
                match(SrslLexer.AssignOperator);
                match(SrslLexer.DeclareClassInstance);

                classIdentifier = LT(1).text;
                match(SrslLexer.Identifier);

                if (LA(1) == SrslLexer.DotOperator)
                {
                    classInstanceDeclarationNode.ClassPath = new List<Identifier>();
                }

                while (LA(1) == SrslLexer.DotOperator)
                {
                    classInstanceDeclarationNode.ClassPath.Add(new Identifier(classIdentifier));
                    match(SrslLexer.DotOperator);
                    classIdentifier = LT(1).text;
                    match(SrslLexer.Identifier);
                }

                classInstanceDeclarationNode.ClassName = new Identifier(classIdentifier);
                match(SrslLexer.OpeningRoundBracket);

                if (!string.IsNullOrEmpty(identifier))
                {
                    classInstanceDeclarationNode.InstanceId = new Identifier(identifier);
                }
                else
                {
                    throw new Exception("Empty or null string as class identifier is forbidden!");
                }

                classInstanceDeclarationNode.Modifiers = new ModifiersNode(accessMod, staticAbstractMod);

                if (LA(1) != SrslLexer.ClosingRoundBracket)
                {
                    classInstanceDeclarationNode.Arguments = arguments();
                }

                match(SrslLexer.ClosingRoundBracket);
                if (MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration)
                {
                    match(SrslLexer.SemicolonSeperator);
                }

                return classInstanceDeclarationNode;
            }

            return null;
        }

        public HeteroAstNode _declaration()
        {
            /*if ( speculate_declaration_namespace() )
            {
                // Console.WriteLine( "predict alternative namespace" );

                return namespaceDeclaration();
            }*/

            if (speculate_declaration_class())
            {
                // Console.WriteLine( "predict alternative class" );

                return classDeclaration();
            }

            if (speculate_declaration_class_forward())
            {
                // Console.WriteLine( "predict alternative class forward" );

                return classDeclarationForward();
            }

            if (speculate_declaration_struct())
            {
                // Console.WriteLine( "predict alternative struct" );

                return structDeclaration();
            }

            if (speculate_declaration_function())
            {
                // Console.WriteLine( "predict alternative function" );

                return functionDeclaration();
            }

            if (speculate_declaration_function_forward())
            {
                // Console.WriteLine( "predict alternative function forward" );

                return functionDeclarationForward();
            }

            if (speculate_declaration_class_instance())
            {
                // Console.WriteLine( "predict alternative class instance" );

                return classInstanceDeclaration();
            }

            if (speculate_declaration_variable())
            {
                // Console.WriteLine( "predict alternative variable declaration" );

                return variableDeclaration();
            }

            if (speculate_statement())
            {
                // Console.WriteLine( "predict alternative statement" );

                return statement();
            }

            throw new NoViableAltException("expecting declaration found " + LT(1));
        }

        public virtual ExpressionNode _equality()
        {
            if (Speculating)
            {
                relational();

                while (LA(1) == SrslLexer.UnequalOperator || LA(1) == SrslLexer.EqualOperator)
                {
                    consume();
                    relational();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = relational();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.UnequalOperator || LA(1) == SrslLexer.EqualOperator)
                {
                    if (LA(1) == SrslLexer.UnequalOperator)
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.NotEqual;
                    }
                    else
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Equal;
                    }

                    consume();
                    ExpressionNode expressionNode = relational();

                    if (LA(1) == SrslLexer.UnequalOperator || LA(1) == SrslLexer.EqualOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ExpressionNode _expression()
        {
            if (Speculating)
            {
                assignment();
            }
            else
            {
                ExpressionNode expressionNode = new ExpressionNode();
                expressionNode.Assignment = assignment();

                return expressionNode;
            }

            return null;
        }

        public virtual ExpressionStatementNode _expressionStatement()
        {
            if (Speculating)
            {
                expression();
                if (MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration)
                {
                    match(SrslLexer.SemicolonSeperator);
                }
            }
            else
            {
                ExpressionStatementNode expressionStatementNode = new ExpressionStatementNode();
                expressionStatementNode.Expression = expression();
                if (MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration)
                {
                    match(SrslLexer.SemicolonSeperator);
                }

                return expressionStatementNode;
            }

            return null;
        }

        public virtual ForStatementNode _forStatement()
        {
            if (Speculating)
            {
                match(SrslLexer.DeclareForLoop);
                match(SrslLexer.OpeningRoundBracket);

                if (LA(1) == SrslLexer.DeclareVariable)
                {
                    variableDeclaration();
                }
                else
                {
                    if (LA(1) != SrslLexer.SemicolonSeperator)
                    {
                        expressionStatement();
                    }
                    else
                    {
                        match(SrslLexer.SemicolonSeperator);
                    }
                }

                if (LA(1) != SrslLexer.SemicolonSeperator)
                {
                    expression();
                    match(SrslLexer.SemicolonSeperator);
                }
                else
                {
                    match(SrslLexer.SemicolonSeperator);
                }

                if (LA(1) != SrslLexer.ClosingRoundBracket)
                {
                    expression();
                }

                match(SrslLexer.ClosingRoundBracket);

                block();
            }
            else
            {
                ForStatementNode forStatementNode = new ForStatementNode();

                match(SrslLexer.DeclareForLoop);
                match(SrslLexer.OpeningRoundBracket);

                if (speculate_declaration_variable())
                {
                    // Console.WriteLine( "predict variable declaration" );

                    forStatementNode.VariableDeclaration = variableDeclaration();
                }
                else if (speculate_expression_statement())
                {
                    // Console.WriteLine( "predict alternative for expression statement" );

                    forStatementNode.ExpressionStatement = expressionStatement();
                }
                else
                {
                    match(SrslLexer.SemicolonSeperator);
                }

                if (speculate_expression())
                {
                    // Console.WriteLine( "predict alternative for expression" );

                    forStatementNode.Expression1 = expression();
                    match(SrslLexer.SemicolonSeperator);
                }
                else
                {
                    match(SrslLexer.SemicolonSeperator);
                }

                if (speculate_expression())
                {
                    // Console.WriteLine( "predict alternative for expression" );

                    forStatementNode.Expression2 = expression();
                }

                match(SrslLexer.ClosingRoundBracket);
                forStatementNode.Block = block();

                return forStatementNode;
            }

            return null;
        }

        public virtual FunctionDeclarationNode _functionDeclaration()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic || LA(1) == SrslLexer.DeclareAbstract)
                {
                    consume();
                }

                match(SrslLexer.DeclareFunction);
                match(SrslLexer.Identifier);
                match(SrslLexer.OpeningRoundBracket);

                if (LA(1) == SrslLexer.Identifier)
                {
                    match(SrslLexer.Identifier);

                    while (LA(1) != SrslLexer.ClosingRoundBracket)
                    {
                        match(SrslLexer.CommaSeperator);
                        match(SrslLexer.Identifier);
                    }
                }

                match(SrslLexer.ClosingRoundBracket);
                block();
            }
            else
            {
                Token accessMod = null;
                Token staticAbstractMod = null;
                string functionIdentifier = null;
                ParametersNode parametersNode = new ParametersNode();

                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    accessMod = LT(1);
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic || LA(1) == SrslLexer.DeclareAbstract)
                {
                    staticAbstractMod = LT(1);
                    consume();
                }

                match(SrslLexer.DeclareFunction);
                functionIdentifier = LT(1).text;
                match(SrslLexer.Identifier);

                match(SrslLexer.OpeningRoundBracket);

                if (LA(1) == SrslLexer.Identifier)
                {
                    parametersNode.Identifiers = new List<Identifier>();
                    parametersNode.Identifiers.Add(new Identifier(LT(1).text));
                    match(SrslLexer.Identifier);

                    while (LA(1) != SrslLexer.ClosingRoundBracket)
                    {
                        match(SrslLexer.CommaSeperator);
                        parametersNode.Identifiers.Add(new Identifier(LT(1).text));
                        match(SrslLexer.Identifier);
                    }
                }

                match(SrslLexer.ClosingRoundBracket);

                FunctionDeclarationNode functionDeclarationNode = new FunctionDeclarationNode();

                if (functionIdentifier != null)
                {
                    functionDeclarationNode.FunctionId = new Identifier(functionIdentifier);
                }
                else
                {
                    throw new Exception("Empty or null string as class identifier is forbidden!");
                }

                functionDeclarationNode.Modifiers = new ModifiersNode(accessMod, staticAbstractMod);
                functionDeclarationNode.FunctionBlock = block();
                functionDeclarationNode.Parameters = parametersNode;

                return functionDeclarationNode;
            }

            return null;
        }

        public virtual FunctionDeclarationNode _functionDeclarationForward()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic || LA(1) == SrslLexer.DeclareAbstract)
                {
                    consume();
                }

                match(SrslLexer.DeclareFunction);
                match(SrslLexer.Identifier);

                match(SrslLexer.OpeningRoundBracket);

                if (LA(1) == SrslLexer.Identifier)
                {
                    match(SrslLexer.Identifier);

                    while (LA(1) != SrslLexer.ClosingRoundBracket)
                    {
                        match(SrslLexer.CommaSeperator);
                        match(SrslLexer.Identifier);
                    }
                }

                match(SrslLexer.ClosingRoundBracket);

                match(SrslLexer.SemicolonSeperator);
            }
            else
            {
                Token accessMod = null;
                Token staticAbstractMod = null;
                string functionIdentifier = null;
                ParametersNode parametersNode = new ParametersNode();
                parametersNode.Identifiers = new List<Identifier>();

                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    accessMod = LT(1);
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic || LA(1) == SrslLexer.DeclareAbstract)
                {
                    staticAbstractMod = LT(1);
                    consume();
                }

                match(SrslLexer.DeclareFunction);
                functionIdentifier = LT(1).text;
                match(SrslLexer.Identifier);

                match(SrslLexer.OpeningRoundBracket);

                if (LA(1) == SrslLexer.Identifier)
                {
                    parametersNode.Identifiers.Add(new Identifier(LT(1).text));
                    match(SrslLexer.Identifier);

                    while (LA(1) != SrslLexer.ClosingRoundBracket)
                    {
                        match(SrslLexer.CommaSeperator);
                        parametersNode.Identifiers.Add(new Identifier(LT(1).text));
                        match(SrslLexer.Identifier);
                    }
                }

                match(SrslLexer.ClosingRoundBracket);

                FunctionDeclarationNode functionDeclarationNode = new FunctionDeclarationNode();

                if (functionIdentifier != null)
                {
                    functionDeclarationNode.FunctionId = new Identifier(functionIdentifier);
                }
                else
                {
                    throw new Exception("Empty or null string as class identifier is forbidden!");
                }

                functionDeclarationNode.Modifiers = new ModifiersNode(accessMod, staticAbstractMod);
                match(SrslLexer.SemicolonSeperator);

                return functionDeclarationNode;
            }

            return null;
        }

        public virtual IfStatementNode _ifStatement()
        {
            if (Speculating)
            {
                match(SrslLexer.ControlFlowIf);
                match(SrslLexer.OpeningRoundBracket);
                expression();
                match(SrslLexer.ClosingRoundBracket);
                block();
                while (LA(1) == SrslLexer.ControlFlowElse)
                {
                    match(SrslLexer.ControlFlowElse);

                    if (LA(1) == SrslLexer.ControlFlowIf)
                    {
                        match(SrslLexer.ControlFlowIf);
                        match(SrslLexer.OpeningRoundBracket);
                        expression();
                        match(SrslLexer.ClosingRoundBracket);
                        block();
                    }
                    else
                    {
                        block();
                    }
                }
            }
            else
            {
                match(SrslLexer.ControlFlowIf);
                match(SrslLexer.OpeningRoundBracket);
                IfStatementNode ifStatement = new IfStatementNode();
                ifStatement.Expression = expression();
                match(SrslLexer.ClosingRoundBracket);
                ifStatement.ThenBlock = block();
                ifStatement.IfStatementEntries = new List<IfStatementEntry>();
                while (LA(1) == SrslLexer.ControlFlowElse)
                {
                    IfStatementEntry ifStatementEntry = new IfStatementEntry();
                    match(SrslLexer.ControlFlowElse);

                    if (LA(1) == SrslLexer.ControlFlowIf)
                    {
                        ifStatementEntry.IfStatementType = IfStatementEntryType.ElseIf;
                        match(SrslLexer.ControlFlowIf);
                        match(SrslLexer.OpeningRoundBracket);
                        ifStatementEntry.ExpressionElseIf = expression();
                        match(SrslLexer.ClosingRoundBracket);
                        ifStatementEntry.ElseBlock = block();
                    }
                    else
                    {
                        ifStatementEntry.IfStatementType = IfStatementEntryType.Else;
                        ifStatementEntry.ElseBlock = block();
                    }

                    ifStatement.IfStatementEntries.Add(ifStatementEntry);
                }

                return ifStatement;
            }

            return null;
        }

        public virtual ExpressionNode _logicAnd()
        {
            if (Speculating)
            {
                bitwiseOr();

                while (LA(1) == SrslLexer.LogicalAndOperator)
                {
                    consume();
                    bitwiseOr();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = bitwiseOr();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.LogicalAndOperator)
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.And;
                    consume();
                    ExpressionNode expressionNode = bitwiseOr();

                    if (LA(1) == SrslLexer.LogicalAndOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ExpressionNode _logicOr()
        {
            if (Speculating)
            {
                logicAnd();

                while (LA(1) == SrslLexer.LogicalOrOperator)
                {
                    consume();
                    logicAnd();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = logicAnd();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.LogicalOrOperator)
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Or;
                    consume();
                    ExpressionNode expressionNode = logicAnd();

                    if (LA(1) == SrslLexer.LogicalOrOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ExpressionNode _multiplicative()
        {
            if (Speculating)
            {
                unary();

                while (LA(1) == SrslLexer.MultiplyOperator ||
                        LA(1) == SrslLexer.DivideOperator ||
                        LA(1) == SrslLexer.ModuloOperator)
                {
                    consume();
                    unary();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = unary();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.MultiplyOperator ||
                        LA(1) == SrslLexer.DivideOperator ||
                        LA(1) == SrslLexer.ModuloOperator)
                {
                    if (LA(1) == SrslLexer.MultiplyOperator)
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Mult;
                    }
                    else if (LA(1) == SrslLexer.DivideOperator)
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Div;
                    }
                    else
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Modulo;
                    }

                    consume();

                    ExpressionNode expressionNode = unary();

                    if (LA(1) == SrslLexer.MultiplyOperator ||
                         LA(1) == SrslLexer.DivideOperator ||
                         LA(1) == SrslLexer.ModuloOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual PrimaryNode _primary()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.BooleanLiteral ||
                     LA(1) == SrslLexer.NullReference ||
                     LA(1) == SrslLexer.ThisReference ||
                     LA(1) == SrslLexer.IntegerLiteral ||
                     LA(1) == SrslLexer.FloatingLiteral ||
                     LA(1) == SrslLexer.StringLiteral ||
                     LA(1) == SrslLexer.Identifier)
                {
                    consume();
                }
                else if (LA(1) == SrslLexer.OpeningRoundBracket)
                {
                    match(SrslLexer.OpeningRoundBracket);
                    MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = false;
                    declaration();
                    MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;
                    match(SrslLexer.ClosingRoundBracket);
                }
            }
            else
            {
                PrimaryNode primaryNode = new PrimaryNode();

                if (LA(1) == SrslLexer.BooleanLiteral)
                {
                    primaryNode.BooleanLiteral = bool.Parse(LT(1).text);
                    primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.BooleanLiteral;
                    consume();
                }
                else if (LA(1) == SrslLexer.NullReference)
                {
                    primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.NullReference;
                    consume();
                }
                else if (LA(1) == SrslLexer.ThisReference)
                {
                    primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.ThisReference;
                    consume();
                }
                else if (LA(1) == SrslLexer.IntegerLiteral)
                {
                    primaryNode.IntegerLiteral = int.Parse(LT(1).text);
                    primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.IntegerLiteral;
                    consume();
                }
                else if (LA(1) == SrslLexer.FloatingLiteral)
                {
                    primaryNode.FloatLiteral = double.Parse(LT(1).text, CultureInfo.InvariantCulture);
                    primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.FloatLiteral;
                    consume();
                }
                else if (LA(1) == SrslLexer.StringLiteral)
                {
                    primaryNode.StringLiteral = LT(1).text;
                    primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.StringLiteral;
                    consume();
                }
                else if (LA(1) == SrslLexer.Identifier)
                {
                    primaryNode.PrimaryId = new Identifier(LT(1).text);
                    primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Identifier;
                    consume();
                }
                else if (LA(1) == SrslLexer.OpeningRoundBracket)
                {
                    match(SrslLexer.OpeningRoundBracket);
                    MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = false;
                    primaryNode.Expression = declaration();
                    MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;
                    primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Expression;
                    match(SrslLexer.ClosingRoundBracket);
                }
                else
                {
                    throw new NoViableAltException(LT(1).text);
                }

                return primaryNode;
            }

            return null;
        }

        public virtual ExpressionNode _relational()
        {
            if (Speculating)
            {
                shift();

                while (LA(1) == SrslLexer.GreaterOperator ||
                        LA(1) == SrslLexer.GreaterEqualOperator ||
                        LA(1) == SrslLexer.SmallerOperator ||
                        LA(1) == SrslLexer.SmallerEqualOperator)
                {
                    consume();
                    shift();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = shift();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.GreaterOperator ||
                        LA(1) == SrslLexer.GreaterEqualOperator ||
                        LA(1) == SrslLexer.SmallerOperator ||
                        LA(1) == SrslLexer.SmallerEqualOperator)
                {
                    if (LA(1) == SrslLexer.GreaterOperator)
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Greater;
                    }
                    else if (LA(1) == SrslLexer.GreaterEqualOperator)
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.GreaterOrEqual;
                    }
                    else if (LA(1) == SrslLexer.SmallerOperator)
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Less;
                    }
                    else
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.LessOrEqual;
                    }

                    consume();

                    ExpressionNode expressionNode = shift();

                    if (LA(1) == SrslLexer.GreaterOperator ||
                         LA(1) == SrslLexer.GreaterEqualOperator ||
                         LA(1) == SrslLexer.SmallerOperator ||
                         LA(1) == SrslLexer.SmallerEqualOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ReturnStatementNode _returnStatement()
        {
            if (Speculating)
            {
                match(SrslLexer.FunctionReturn);
                expressionStatement();
            }
            else
            {
                match(SrslLexer.FunctionReturn);
                ReturnStatementNode returnStatementNode = new ReturnStatementNode();
                returnStatementNode.ExpressionStatement = expressionStatement();

                return returnStatementNode;
            }

            return null;
        }

        public HeteroAstNode _statement()
        {
            if (speculate_using_statement())
            {
                // Console.WriteLine( "predict expression statement" );

                return usingStatement();
            }

            if (speculate_expression_statement())
            {
                // Console.WriteLine( "predict expression statement" );

                return expressionStatement();
            }

            if (speculate_for_statement())
            {
                // Console.WriteLine( "predict alternative for statement" );

                return forStatement();
            }

            if (speculate_if_statement())
            {
                // Console.WriteLine( "predict alternative if statement" );

                return ifStatement();
            }

            if (speculate_return_statement())
            {
                // Console.WriteLine( "predict alternative return statement" );

                return returnStatement();
            }

            if (speculate_while_statement())
            {
                // Console.WriteLine( "predict alternative while statement" );

                return whileStatement();
            }

            if (speculate_block())
            {
                // Console.WriteLine( "predict alternative block statement" );

                return block();
            }

            throw new NoViableAltException("expecting declaration found " + LT(1));
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

        public UsingStatementNode usingStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "usingStatement"))
            {
                return null;
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

        public UsingStatementNode _usingStatement()
        {
            if (!Speculating)
            {
                match(SrslLexer.UsingDirective);
                match(SrslLexer.OpeningRoundBracket);

                UsingStatementNode usingStatementNode = new UsingStatementNode();
                MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = false;
                usingStatementNode.UsingNode = declaration();
                MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;
                match(SrslLexer.ClosingRoundBracket);
                usingStatementNode.UsingBlock = block();

                return usingStatementNode;
            }

            match(SrslLexer.UsingDirective);
            match(SrslLexer.OpeningRoundBracket);
            MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = false;
            declaration();
            MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;
            match(SrslLexer.ClosingRoundBracket);
            block();

            return null;
        }

        public virtual StructDeclarationNode _structDeclaration()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    consume();
                }

                match(SrslLexer.DeclareStruct);
                match(SrslLexer.Identifier);

                block();
            }
            else
            {
                Token accessMod = null;
                Token staticAbstractMod = null;
                string classIdentifier = null;

                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    accessMod = LT(1);
                    consume();
                }

                match(SrslLexer.DeclareStruct);
                classIdentifier = LT(1).text;
                match(SrslLexer.Identifier);

                StructDeclarationNode structDeclarationNode = new StructDeclarationNode();

                if (classIdentifier != null)
                {
                    structDeclarationNode.StructId = new Identifier(classIdentifier);
                }
                else
                {
                    throw new Exception("Empty or null string as class identifier is forbidden!");
                }

                structDeclarationNode.Modifiers = new ModifiersNode(accessMod, staticAbstractMod);
                structDeclarationNode.Block = block();

                return structDeclarationNode;
            }

            return null;
        }

        public virtual ExpressionNode _unary()
        {
            if (speculate_unary_prefix())
            {
                // Console.WriteLine( "predict unary prefix" );

                return unaryPrefix();
            }

            if (speculate_call())
            {
                // Console.WriteLine( "predict call" );

                return call();
            }

            if (speculate_unary_postfix())
            {
                // Console.WriteLine( "predict unary postfix" );

                return unaryPostfix();
            }



            throw new NoViableAltException("");
        }

        public virtual ExpressionNode _unaryPostfix()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.Identifier)
                {
                    consume();
                }

                if (LA(1) == SrslLexer.MinusMinusOperator ||
                     LA(1) == SrslLexer.PlusPlusOperator)
                {
                    consume();
                }
                else
                {
                    throw new NoViableAltException("");
                }
            }
            else
            {
                UnaryPostfixOperation unaryPostfix = new UnaryPostfixOperation();
                unaryPostfix.Primary = call();


                if (LA(1) == SrslLexer.MinusMinusOperator ||
                     LA(1) == SrslLexer.PlusPlusOperator)
                {
                    if (LA(1) == SrslLexer.MinusMinusOperator)
                    {
                        match(SrslLexer.MinusMinusOperator);
                        unaryPostfix.Operator = UnaryPostfixOperation.UnaryPostfixOperatorType.MinusMinus;
                    }
                    else if (LA(1) == SrslLexer.PlusPlusOperator)
                    {
                        match(SrslLexer.PlusPlusOperator);
                        unaryPostfix.Operator = UnaryPostfixOperation.UnaryPostfixOperatorType.PlusPlus;
                    }


                }

                return unaryPostfix;
            }

            return null;
        }

        public virtual ExpressionNode _unaryPrefix()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.MinusMinusOperator ||
                     LA(1) == SrslLexer.PlusPlusOperator ||
                     LA(1) == SrslLexer.LogicalNegationOperator ||
                     LA(1) == SrslLexer.MinusOperator ||
                     LA(1) == SrslLexer.ComplimentOperator ||
                     LA(1) == SrslLexer.PlusOperator)
                {
                    consume();
                    unary();
                }
                else
                {
                    throw new NoViableAltException("");
                }
            }
            else
            {
                UnaryPrefixOperation unaryPrefix = new UnaryPrefixOperation();

                if (LA(1) == SrslLexer.MinusMinusOperator ||
                     LA(1) == SrslLexer.PlusPlusOperator ||
                     LA(1) == SrslLexer.LogicalNegationOperator ||
                     LA(1) == SrslLexer.MinusOperator ||
                     LA(1) == SrslLexer.ComplimentOperator ||
                     LA(1) == SrslLexer.PlusOperator)
                {
                    if (LA(1) == SrslLexer.MinusMinusOperator)
                    {
                        match(SrslLexer.MinusMinusOperator);
                        unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.MinusMinus;
                    }
                    else if (LA(1) == SrslLexer.PlusPlusOperator)
                    {
                        match(SrslLexer.PlusPlusOperator);
                        unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.PlusPlus;
                    }
                    else if (LA(1) == SrslLexer.LogicalNegationOperator)
                    {
                        match(SrslLexer.LogicalNegationOperator);
                        unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.LogicalNot;
                    }
                    else if (LA(1) == SrslLexer.ComplimentOperator)
                    {
                        match(SrslLexer.ComplimentOperator);
                        unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.Compliment;
                    }
                    else if (LA(1) == SrslLexer.PlusOperator)
                    {
                        match(SrslLexer.PlusOperator);
                        unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.Plus;
                    }
                    else
                    {
                        match(SrslLexer.MinusOperator);
                        unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.Negate;
                    }

                    unaryPrefix.Primary = unary();
                }

                return unaryPrefix;
            }

            return null;
        }

        public virtual VariableDeclarationNode _variableDeclaration()
        {
            if (Speculating)
            {
                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic)
                {
                    consume();
                }

                match(SrslLexer.DeclareVariable);
                match(SrslLexer.Identifier);

                if (LA(1) == SrslLexer.AssignOperator)
                {
                    consume();

                    if (LA(1) == SrslLexer.DeclareClassInstance)
                    {
                        throw new MismatchedTokenException("Got " + LA(1) + ". Expected Variable Declaration");
                    }

                    expressionStatement();
                }
                else
                {
                    if (MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration)
                    {
                        match(SrslLexer.SemicolonSeperator);
                    }
                }
            }
            else
            {
                Token accessMod = null;
                Token staticAbstractMod = null;
                string identifier = null;

                if (LA(1) == SrslLexer.DeclarePrivate || LA(1) == SrslLexer.DeclarePublic)
                {
                    accessMod = LT(1);
                    consume();
                }

                if (LA(1) == SrslLexer.DeclareStatic)
                {
                    staticAbstractMod = LT(1);
                    consume();
                }

                match(SrslLexer.DeclareVariable);
                identifier = LT(1).text;
                match(SrslLexer.Identifier);
                ExpressionStatementNode init = null;

                if (LA(1) == SrslLexer.AssignOperator)
                {
                    consume();

                    if (LA(1) == SrslLexer.DeclareClassInstance)
                    {
                        throw new MismatchedTokenException("Got " + LA(1) + ". Expected Variable Declaration");
                    }

                    init = expressionStatement();
                }
                else
                {
                    if (MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration)
                    {
                        match(SrslLexer.SemicolonSeperator);
                    }
                }

                VariableDeclarationNode variableDeclarationNode = new VariableDeclarationNode();

                if (!string.IsNullOrEmpty(identifier))
                {
                    variableDeclarationNode.VarId = new Identifier(identifier);
                }
                else
                {
                    throw new Exception("Empty or null string as class identifier is forbidden!");
                }

                variableDeclarationNode.Modifiers = new ModifiersNode(accessMod, staticAbstractMod);

                if (init != null)
                {
                    variableDeclarationNode.Initializer = new InitializerNode();
                    variableDeclarationNode.Initializer.Expression = init;
                }

                return variableDeclarationNode;
            }

            return null;
        }

        public virtual WhileStatementNode _whileStatement()
        {
            if (Speculating)
            {
                match(SrslLexer.DeclareWhileLoop);
                match(SrslLexer.OpeningRoundBracket);
                expression();
                match(SrslLexer.ClosingRoundBracket);
                block();
            }
            else
            {
                match(SrslLexer.DeclareWhileLoop);
                match(SrslLexer.OpeningRoundBracket);
                WhileStatementNode whileStatementNode = new WhileStatementNode();
                whileStatementNode.Expression = expression();
                match(SrslLexer.ClosingRoundBracket);
                whileStatementNode.WhileBlock = block();

                return whileStatementNode;
            }

            return null;
        }

        public virtual ExpressionNode additive()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "additive"))
            {
                return null;
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

        public virtual ArgumentsNode arguments()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "arguments"))
            {
                return null;
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

        public virtual AssignmentNode assignment()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "assignment"))
            {
                return null;
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

        public virtual AssignmentNode assignment_assignment()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "assignment_assignment"))
            {
                return null;
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

        public virtual BlockStatementNode block()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "block"))
            {
                return null;
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

        public virtual CallNode call()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "call"))
            {
                return null;
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

        public virtual ClassDeclarationNode classDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "classDeclaration"))
            {
                return null;
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

        public virtual ClassDeclarationNode classDeclarationForward()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "classDeclarationForward"))
            {
                return null;
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

        public virtual ClassInstanceDeclarationNode classInstanceDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "classInstanceDeclaration"))
            {
                return null;
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

        public override void clearMemo()
        {
            MemoizingDictionary.Clear();
        }

        public virtual HeteroAstNode declaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "declaration"))
            {
                return null;
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

        public virtual ExpressionNode equality()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "equality"))
            {
                return null;
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

        public virtual ExpressionNode expression()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "expression"))
            {
                return null;
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

        public virtual ExpressionStatementNode expressionStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "expressionStatement"))
            {
                return null;
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

        public virtual ForStatementNode forStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "forStatement"))
            {
                return null;
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

        public virtual FunctionDeclarationNode functionDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "functionDeclaration"))
            {
                return null;
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

        public virtual FunctionDeclarationNode functionDeclarationForward()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "functionDeclarationForward"))
            {
                return null;
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

        public virtual IfStatementNode ifStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "ifStatement"))
            {
                return null;
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

        public virtual ExpressionNode logicAnd()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "logicAnd"))
            {
                return null;
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

        public virtual ExpressionNode ternary()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "ternary"))
            {
                return null;
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

        private ExpressionNode _ternary()
        {
            if (Speculating)
            {
                logicOr();

                while (LA(1) == SrslLexer.QuestionMarkOperator)
                {
                    consume();
                    logicOr();
                    match(SrslLexer.ColonOperator);
                    logicOr();
                }
            }
            else
            {
                TernaryOperationNode firstOperationNode = new TernaryOperationNode();
                firstOperationNode.LeftOperand = logicOr();
                TernaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.QuestionMarkOperator)
                {
                    consume();
                    currentOperationNode.MidOperand = logicOr();
                    match(SrslLexer.ColonOperator);
                    ExpressionNode right = logicOr();

                    if (LA(1) == SrslLexer.QuestionMarkOperator)
                    {
                        TernaryOperationNode ternaryOperationNode = new TernaryOperationNode();
                        currentOperationNode.RightOperand = ternaryOperationNode;
                        currentOperationNode = ternaryOperationNode;
                        currentOperationNode.LeftOperand = ternaryOperationNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = right;
                    }
                }

                if (firstOperationNode.RightOperand != null || firstOperationNode.MidOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ExpressionNode bitwiseOr()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "bitwiseOr"))
            {
                return null;
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

        private ExpressionNode _bitwiseOr()
        {
            if (Speculating)
            {
                bitwiseXor();

                while (LA(1) == SrslLexer.BitwiseOrOperator)
                {
                    consume();
                    bitwiseXor();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = bitwiseXor();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.BitwiseOrOperator)
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseOr;
                    consume();
                    ExpressionNode expressionNode = bitwiseXor();

                    if (LA(1) == SrslLexer.BitwiseOrOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ExpressionNode bitwiseXor()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "bitwiseXor"))
            {
                return null;
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

        private ExpressionNode _bitwiseXor()
        {
            if (Speculating)
            {
                bitwiseAnd();

                while (LA(1) == SrslLexer.BitwiseXorOperator)
                {
                    consume();
                    bitwiseAnd();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = bitwiseAnd();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.BitwiseXorOperator)
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseXor;
                    consume();
                    ExpressionNode expressionNode = bitwiseAnd();

                    if (LA(1) == SrslLexer.BitwiseXorOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ExpressionNode bitwiseAnd()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "bitwiseAnd"))
            {
                return null;
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

        private ExpressionNode _bitwiseAnd()
        {
            if (Speculating)
            {
                equality();

                while (LA(1) == SrslLexer.BitwiseAndOperator)
                {
                    consume();
                    equality();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = equality();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.BitwiseAndOperator)
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseAnd;
                    consume();
                    ExpressionNode expressionNode = equality();

                    if (LA(1) == SrslLexer.BitwiseAndOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ExpressionNode shift()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "shift"))
            {
                return null;
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

        private ExpressionNode _shift()
        {
            if (Speculating)
            {
                additive();

                while (LA(1) == SrslLexer.ShiftLeftOperator || LA(1) == SrslLexer.ShiftRightOperator)
                {
                    consume();
                    additive();
                }
            }
            else
            {
                BinaryOperationNode firstOperationNode = new BinaryOperationNode();
                firstOperationNode.LeftOperand = additive();
                BinaryOperationNode currentOperationNode = firstOperationNode;

                while (LA(1) == SrslLexer.ShiftLeftOperator || LA(1) == SrslLexer.ShiftRightOperator)
                {
                    if (LA(1) == SrslLexer.ShiftLeftOperator)
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.ShiftLeft;
                    }
                    else
                    {
                        currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.ShiftRight;
                    }

                    consume();

                    ExpressionNode expressionNode = additive();

                    if (LA(1) == SrslLexer.ShiftLeftOperator || LA(1) == SrslLexer.ShiftRightOperator)
                    {
                        BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                        currentOperationNode.RightOperand = binaryOperationNode;
                        currentOperationNode = binaryOperationNode;
                        currentOperationNode.LeftOperand = expressionNode;
                    }
                    else
                    {
                        currentOperationNode.RightOperand = expressionNode;
                    }
                }

                if (firstOperationNode.RightOperand != null)
                {
                    return firstOperationNode;
                }

                return firstOperationNode.LeftOperand;
            }

            return null;
        }

        public virtual ExpressionNode logicOr()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "logicOr"))
            {
                return null;
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

        public virtual ExpressionNode multiplicative()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "multiplicative"))
            {
                return null;
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

        public virtual PrimaryNode primary()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "primary"))
            {
                return null;
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

        public virtual ModuleNode module()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "module"))
            {
                return null;
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

        private ModuleNode _module()
        {
            if (!Speculating)
            {
                match(SrslLexer.DeclareModule);
                string moduleIdentifier = LT(1).text;
                match(SrslLexer.Identifier);
                List<string> parentModules = new List<string>();
                List<ModuleIdentifier> importedModules = new List<ModuleIdentifier>();
                List<ModuleIdentifier> usedModules = new List<ModuleIdentifier>();

                ModuleNode moduleNode = new ModuleNode();

                while (LA(1) == SrslLexer.DotOperator)
                {
                    parentModules.Add(moduleIdentifier);
                    match(SrslLexer.DotOperator);
                    moduleIdentifier = LT(1).text;
                    match(SrslLexer.Identifier);
                }

                match(SrslLexer.SemicolonSeperator);
                moduleNode.ModuleIdent = new ModuleIdentifier(moduleIdentifier, parentModules);

                if (LA(1) == SrslLexer.ImportDirective || LA(1) == SrslLexer.UsingDirective)
                {
                    while (LA(1) == SrslLexer.ImportDirective || LA(1) == SrslLexer.UsingDirective)
                    {
                        if (LA(1) == SrslLexer.ImportDirective)
                        {
                            match(SrslLexer.ImportDirective);
                            string nextIdentifier = LT(1).text;
                            match(SrslLexer.Identifier);
                            List<string> pModules = new List<string>();

                            while (LA(1) == SrslLexer.DotOperator)
                            {
                                pModules.Add(nextIdentifier);
                                match(SrslLexer.DotOperator);
                                nextIdentifier = LT(1).text;
                                match(SrslLexer.Identifier);
                            }

                            match(SrslLexer.SemicolonSeperator);
                            importedModules.Add(new ModuleIdentifier(nextIdentifier, pModules));
                        }
                        else
                        {
                            match(SrslLexer.UsingDirective);
                            string nextIdentifier = LT(1).text;
                            match(SrslLexer.Identifier);
                            List<string> pModules = new List<string>();

                            while (LA(1) == SrslLexer.DotOperator)
                            {
                                pModules.Add(nextIdentifier);
                                match(SrslLexer.DotOperator);
                                nextIdentifier = LT(1).text;
                                match(SrslLexer.Identifier);
                            }

                            match(SrslLexer.SemicolonSeperator);
                            usedModules.Add(new ModuleIdentifier(nextIdentifier, pModules));
                        }
                    }
                }

                moduleNode.ImportedModules = importedModules;
                moduleNode.UsedModules = usedModules;

                moduleNode.Statements = new List<StatementNode>();

                while (LA(1) != Lexer.EOF_TYPE)
                {
                    HeteroAstNode decl = declaration();

                    if (decl is ClassDeclarationNode classDeclarationNode)
                    {
                        moduleNode.Statements.Add(classDeclarationNode);
                    }

                    else if (decl is FunctionDeclarationNode functionDeclarationNode)
                    {
                        moduleNode.Statements.Add(functionDeclarationNode);
                    }

                    else if (decl is BlockStatementNode block)
                    {
                        moduleNode.Statements.Add(block);
                    }

                    else if (decl is StructDeclarationNode structDeclaration)
                    {
                        moduleNode.Statements.Add(structDeclaration);
                    }

                    else if (decl is VariableDeclarationNode variable)
                    {
                        moduleNode.Statements.Add(variable);
                    }

                    else if (decl is ClassInstanceDeclarationNode classInstance)
                    {
                        moduleNode.Statements.Add(classInstance);
                    }

                    else if (decl is StatementNode statement)
                    {
                        moduleNode.Statements.Add(statement);
                    }
                }

                return moduleNode;
            }

            match(SrslLexer.DeclareModule);
            match(SrslLexer.Identifier);

            while (LA(1) == SrslLexer.DotOperator)
            {
                match(SrslLexer.DotOperator);
                match(SrslLexer.Identifier);
            }

            match(SrslLexer.SemicolonSeperator);

            if (LA(1) == SrslLexer.ImportDirective || LA(1) == SrslLexer.UsingDirective)
            {
                while (LA(1) == SrslLexer.ImportDirective || LA(1) == SrslLexer.UsingDirective)
                {
                    if (LA(1) == SrslLexer.ImportDirective)
                    {
                        match(SrslLexer.ImportDirective);
                        match(SrslLexer.Identifier);

                        while (LA(1) == SrslLexer.DotOperator)
                        {
                            match(SrslLexer.DotOperator);
                            match(SrslLexer.Identifier);
                        }

                        match(SrslLexer.SemicolonSeperator);
                    }
                    else
                    {
                        match(SrslLexer.UsingDirective);
                        match(SrslLexer.Identifier);

                        while (LA(1) == SrslLexer.DotOperator)
                        {
                            match(SrslLexer.DotOperator);
                            match(SrslLexer.Identifier);
                        }

                        match(SrslLexer.SemicolonSeperator);
                    }
                }
            }

            while (LA(1) != Lexer.EOF_TYPE)
            {
                declaration();
            }

            return null;
        }

        public virtual ExpressionNode relational()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "comparision"))
            {
                return null;
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

        public virtual ReturnStatementNode returnStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "returnStatement"))
            {
                return null;
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

        public virtual HeteroAstNode statement()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "statement"))
            {
                return null;
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

        public virtual StructDeclarationNode structDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "structDeclaration"))
            {
                return null;
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

        public virtual ExpressionNode unary()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "unary"))
            {
                return null;
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

        public virtual ExpressionNode unaryPostfix()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "unaryPostfix"))
            {
                return null;
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

        public virtual ExpressionNode unaryPrefix()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "unaryPrefix"))
            {
                return null;
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

        public virtual VariableDeclarationNode variableDeclaration()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "variableDeclaration"))
            {
                return null;
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

        public virtual WhileStatementNode whileStatement()
        {
            bool failed = false;
            int startTokenIndex = index();

            if (Speculating && alreadyParsedRule(MemoizingDictionary, "whileStatement"))
            {
                return null;
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
