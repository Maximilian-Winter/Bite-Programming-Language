using System;
using System.Collections.Generic;
using System.Globalization;
using Bite.Ast;

namespace Bite.Parser
{

public partial class BiteModuleParser
{
    #region Public

    public virtual IContext < ExpressionNode > _additive()
    {
        if ( Speculating )
        {
            var context = multiplicative();

            if ( context.Failed )
                return context;

            while ( LA( 1 ) == BiteLexer.MinusOperator || LA( 1 ) == BiteLexer.PlusOperator )
            {
                consume();
                context = multiplicative();

                if ( context.Failed )
                    return context;
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();

            var context = multiplicative();

            if ( context.Failed )
                return context;

            firstOperationNode.LeftOperand = context.Result;

            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.MinusOperator || LA( 1 ) == BiteLexer.PlusOperator )
            {
                if ( LA( 1 ) == BiteLexer.PlusOperator )
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Plus;
                }
                else
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Minus;
                }

                consume();

                context = multiplicative();

                if ( context.Failed )
                    return context;

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.MinusOperator || LA( 1 ) == BiteLexer.PlusOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < ArgumentsNode > _arguments()
    {
        if ( Speculating )
        {
            if ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
            {
                while ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
                {
                    if ( LA( 1 ) == BiteLexer.ReferenceOperator )
                    {
                        consume();
                    }

                    var context = expression();

                    if ( context.Failed )
                        return Context < ArgumentsNode >.AsFailed( context.Exception );

                    if ( LA( 1 ) == BiteLexer.CommaSeperator )
                    {
                        consume();
                    }
                }
            }
        }
        else
        {
            if ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
            {
                ArgumentsNode argumentsNode = null;
                int counter = 0;

                while ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
                {
                    if ( counter == 0 )
                    {
                        argumentsNode = new ArgumentsNode();
                        argumentsNode.Expressions = new List < ExpressionNode >();
                        argumentsNode.IsReference = new List < bool >();
                    }

                    if ( LA( 1 ) == BiteLexer.ReferenceOperator )
                    {
                        argumentsNode.IsReference.Add( true );
                        consume();
                    }
                    else
                    {
                        argumentsNode.IsReference.Add( false );
                    }

                    var context = expression();

                    if ( context.Failed )
                        return Context < ArgumentsNode >.AsFailed( context.Exception );

                    ExpressionNode expressionNode = context.Result;

                    argumentsNode.Expressions.Add( expressionNode );

                    if ( LA( 1 ) == BiteLexer.CommaSeperator )
                    {
                        consume();
                    }

                    counter++;
                }

                return new Context < ArgumentsNode >( argumentsNode );
            }
        }

        return null;
    }

    public virtual IContext < AssignmentNode > _assignment()
    {
        if ( speculate_assignment_assignment() )
        {
            // Console.WriteLine( "predict assignment assignment" );
            var context = assignment_assignment();

            if ( context.Failed )
                return Context < AssignmentNode >.AsFailed( context.Exception );

            return context;
        }

        if ( speculate_ternary() )
        {
            // Console.WriteLine( "predict logic or assignment" );
            AssignmentNode assignmentNode = new AssignmentNode();
            var context = ternary();

            if ( context.Failed )
                return Context < AssignmentNode >.AsFailed( context.Exception );

            HeteroAstNode node = context.Result;

            if ( node is AssignmentNode assignment )
            {
                assignmentNode.Type = AssignmentTypes.Assignment;
                assignmentNode.Call = assignment.Call;
                assignmentNode.OperatorType = assignment.OperatorType;
                assignmentNode.Assignment = assignment;
            }

            if ( node is BinaryOperationNode binaryOperationNode )
            {
                assignmentNode.Type = AssignmentTypes.Binary;
                assignmentNode.Binary = binaryOperationNode;
            }

            if ( node is TernaryOperationNode ternaryOperationNode )
            {
                assignmentNode.Type = AssignmentTypes.Ternary;
                assignmentNode.Ternary = ternaryOperationNode;
            }

            if ( node is UnaryPrefixOperation unaryPrefixOperation )
            {
                assignmentNode.Type = AssignmentTypes.UnaryPrefix;
                assignmentNode.UnaryPrefix = unaryPrefixOperation;
            }

            if ( node is UnaryPostfixOperation unaryPostfixOperation )
            {
                assignmentNode.Type = AssignmentTypes.UnaryPostfix;
                assignmentNode.UnaryPostfix = unaryPostfixOperation;
            }

            if ( node is CallNode callNode )
            {
                assignmentNode.Type = AssignmentTypes.Call;
                assignmentNode.Call = callNode;
            }

            if ( node is PrimaryNode primaryNode )
            {
                assignmentNode.Type = AssignmentTypes.Primary;
                assignmentNode.PrimaryNode = primaryNode;
            }

            return new Context < AssignmentNode >( assignmentNode );
        }

        return Context < AssignmentNode >.AsFailed(
            new NoViableAltException( "expected: assignment found: ", LT( 1 ) ) );

        //throw new NoViableAltException("");
    }

    public virtual IContext < AssignmentNode > _assignment_assignment()
    {
        if ( Speculating )
        {
            var context = call();

            if ( context.Failed )
                return Context < AssignmentNode >.AsFailed( context.Exception );

            if ( LA( 1 ) == BiteLexer.AssignOperator ||
                 LA( 1 ) == BiteLexer.MultiplyAssignOperator ||
                 LA( 1 ) == BiteLexer.DivideAssignOperator ||
                 LA( 1 ) == BiteLexer.MinusAssignOperator ||
                 LA( 1 ) == BiteLexer.PlusAssignOperator ||
                 LA( 1 ) == BiteLexer.MinusAssignOperator ||
                 LA( 1 ) == BiteLexer.ModuloAssignOperator ||
                 LA( 1 ) == BiteLexer.BitwiseAndAssignOperator ||
                 LA( 1 ) == BiteLexer.BitwiseOrAssignOperator ||
                 LA( 1 ) == BiteLexer.BitwiseXorAssignOperator ||
                 LA( 1 ) == BiteLexer.BitwiseLeftShiftAssignOperator ||
                 LA( 1 ) == BiteLexer.BitwiseRightShiftAssignOperator )
            {
                consume();

                var contextAssignment = assignment();

                if ( contextAssignment.Failed )
                    return Context < AssignmentNode >.AsFailed( context.Exception );
            }
            else
            {
                return Context < AssignmentNode >.AsFailed(
                    new NoViableAltException( "Expected assignment found: ", LT( 1 ) ) );
            }
        }
        else
        {
            AssignmentNode assignmentNode = new AssignmentNode();

            var context = call();

            if ( context.Failed )
                return Context < AssignmentNode >.AsFailed( context.Exception );

            assignmentNode.Call = context.Result;
            assignmentNode.Type = AssignmentTypes.Assignment;

            switch ( LA( 1 ) )
            {
                case BiteLexer.AssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.Assign;

                    break;

                case BiteLexer.MultiplyAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.MultAssign;

                    break;

                case BiteLexer.DivideAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.DivAssign;

                    break;

                case BiteLexer.MinusAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.MinusAssign;

                    break;

                case BiteLexer.PlusAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.PlusAssign;

                    break;

                case BiteLexer.ModuloAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.ModuloAssignOperator;

                    break;

                case BiteLexer.BitwiseAndAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.BitwiseAndAssignOperator;

                    break;

                case BiteLexer.BitwiseOrAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.BitwiseOrAssignOperator;

                    break;

                case BiteLexer.BitwiseXorAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.BitwiseXorAssignOperator;

                    break;

                case BiteLexer.BitwiseLeftShiftAssignOperator:
                    assignmentNode.OperatorType = AssignmentOperatorTypes.BitwiseLeftShiftAssignOperator;

                    break;

                case BiteLexer.BitwiseRightShiftAssignOperator:
                    assignmentNode.OperatorType =
                        AssignmentOperatorTypes.BitwiseRightShiftAssignOperator;

                    break;

                default:
                    return Context < AssignmentNode >.AsFailed(
                        new NoViableAltException( "expected: assignment found: ", LT( 1 ) ) );
            }

            consume();

            var contextAssignement = assignment();

            if ( contextAssignement.Failed )
                return Context < AssignmentNode >.AsFailed( context.Exception );

            assignmentNode.Assignment = contextAssignement.Result;

            return new Context < AssignmentNode >( assignmentNode );
        }

        return new Context < AssignmentNode >( null );
    }

    public virtual IContext < BlockStatementNode > _block()
    {
        IContext < BlockStatementNode > matchContext = null;

        if ( !match( BiteLexer.OpeningCurlyBracket, out matchContext ) )
            return matchContext;

        if ( Speculating == false )
        {
            BlockStatementNode blockStatementNode = new BlockStatementNode();
            blockStatementNode.Declarations = new DeclarationsNode();

            while ( LA( 1 ) != BiteLexer.ClosingCurlyBracket )
            {
                var context = declaration();

                if ( context.Failed )
                    return Context < BlockStatementNode >.AsFailed( context.Exception );

                HeteroAstNode decl = context.Result;

                switch ( decl )
                {
                    case ClassDeclarationNode classDeclarationNode:
                        blockStatementNode.Declarations.Classes.Add( classDeclarationNode );

                        break;

                    case FunctionDeclarationNode functionDeclarationNode:
                        blockStatementNode.Declarations.Functions.Add( functionDeclarationNode );

                        break;

                    case BlockStatementNode block:
                        blockStatementNode.Declarations.Statements.Add( block );

                        break;

                    case StructDeclarationNode structDeclaration:
                        blockStatementNode.Declarations.Structs.Add( structDeclaration );

                        break;

                    case VariableDeclarationNode variable:
                        blockStatementNode.Declarations.Variables.Add( variable );

                        break;

                    case ClassInstanceDeclarationNode classInstance:
                        blockStatementNode.Declarations.ClassInstances.Add( classInstance );

                        break;

                    case StatementNode statement:
                        blockStatementNode.Declarations.Statements.Add( statement );

                        break;
                }
            }

            if ( !match( BiteLexer.ClosingCurlyBracket, out matchContext ) )
                return matchContext;

            return new Context < BlockStatementNode >( blockStatementNode );
        }

        while ( LA( 1 ) != BiteLexer.ClosingCurlyBracket )
        {
            var context = declaration();

            if ( context.Failed )
                return Context < BlockStatementNode >.AsFailed( context.Exception );
        }

        if ( !match( BiteLexer.ClosingCurlyBracket, out matchContext ) )
            return matchContext;

        return new Context < BlockStatementNode >( null );
    }

    public virtual IContext < CallNode > _call()
    {
        IContext < CallNode > matchContext = null;

        if ( Speculating )
        {
            var context = primary();

            if ( context.Failed )
                return Context < CallNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.DotOperator ||
                    LA( 1 ) == BiteLexer.OpeningRoundBracket ||
                    LA( 1 ) == BiteLexer.SquarebracketLeft )
            {
                if ( LA( 1 ) == BiteLexer.DotOperator )
                {
                    while ( LA( 1 ) == BiteLexer.DotOperator )
                    {
                        if ( !match( BiteLexer.DotOperator, out matchContext ) )
                            return matchContext;

                        if ( !match( BiteLexer.Identifier, out matchContext ) )
                            return matchContext;
                    }
                }

                else if ( LA( 1 ) == BiteLexer.OpeningRoundBracket )
                {
                    if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                        return matchContext;

                    while ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
                    {
                        if ( LA( 1 ) == BiteLexer.ReferenceOperator )
                        {
                            consume();
                        }

                        var contextExpression = expression();

                        if ( contextExpression.Failed )
                            return Context < CallNode >.AsFailed( context.Exception );

                        if ( LA( 1 ) == BiteLexer.CommaSeperator )
                        {
                            if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                                return matchContext;
                        }
                    }

                    if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                        return matchContext;
                }

                else if ( LA( 1 ) == BiteLexer.SquarebracketLeft )
                {
                    while ( LA( 1 ) == BiteLexer.SquarebracketLeft )
                    {
                        if ( !match( BiteLexer.SquarebracketLeft, out matchContext ) )
                            return matchContext;

                        if ( LA( 1 ) == BiteLexer.StringLiteral ||
                             LA( 1 ) == BiteLexer.IntegerLiteral ||
                             LA( 1 ) == BiteLexer.Identifier )
                        {
                            consume();
                        }

                        if ( !match( BiteLexer.SquarebracketRight, out matchContext ) )
                            return matchContext;
                    }
                }
            }

            if ( LA( 1 ) == BiteLexer.PlusPlusOperator || LA( 1 ) == BiteLexer.MinusMinusOperator )
            {
                return Context < CallNode >.AsFailed(
                    new MismatchedTokenException( "Unary Operation! Not Call", LT( 1 ) ) );
            }
        }
        else
        {
            CallNode callNode = new CallNode();

            var context = primary();

            if ( context.Failed )
                return Context < CallNode >.AsFailed( context.Exception );

            callNode.Primary = context.Result;
            callNode.CallType = CallTypes.Primary;
            CallEntry currentCallEntry = null;

            while ( LA( 1 ) == BiteLexer.DotOperator ||
                    LA( 1 ) == BiteLexer.OpeningRoundBracket ||
                    LA( 1 ) == BiteLexer.SquarebracketLeft )
            {
                if ( LA( 1 ) == BiteLexer.DotOperator )
                {
                    if ( callNode.CallEntries == null )
                    {
                        callNode.CallEntries = new List < CallEntry >();
                    }

                    callNode.CallType = CallTypes.PrimaryCall;

                    while ( LA( 1 ) == BiteLexer.DotOperator )
                    {
                        if ( !match( BiteLexer.DotOperator, out matchContext ) )
                            return matchContext;

                        context = primary();

                        if ( context.Failed )
                            return Context < CallNode >.AsFailed( context.Exception );

                        PrimaryNode primaryNode = context.Result;
                        CallEntry callEntry = new CallEntry();
                        callEntry.Primary = primaryNode;
                        callNode.CallEntries.Add( callEntry );
                        currentCallEntry = callEntry;
                    }
                }

                if ( LA( 1 ) == BiteLexer.OpeningRoundBracket )
                {
                    callNode.CallType = CallTypes.PrimaryCall;

                    if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                        return matchContext;

                    if ( currentCallEntry != null )
                    {
                        currentCallEntry.Arguments = new ArgumentsNode();
                        currentCallEntry.Arguments.Expressions = new List < ExpressionNode >();
                        currentCallEntry.IsFunctionCall = true;
                        currentCallEntry.Arguments.IsReference = new List < bool >();
                    }
                    else
                    {
                        callNode.Arguments = new ArgumentsNode();
                        callNode.Arguments.Expressions = new List < ExpressionNode >();
                        callNode.IsFunctionCall = true;
                        callNode.Arguments.IsReference = new List < bool >();
                    }

                    while ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
                    {
                        if ( currentCallEntry != null )
                        {
                            if ( LA( 1 ) == BiteLexer.ReferenceOperator )
                            {
                                currentCallEntry.Arguments.IsReference.Add( true );
                                consume();
                            }
                            else
                            {
                                currentCallEntry.Arguments.IsReference.Add( false );
                            }

                            var contextExpression = expression();

                            if ( contextExpression.Failed )
                                return Context < CallNode >.AsFailed( context.Exception );

                            currentCallEntry.Arguments.Expressions.Add( contextExpression.Result );
                        }
                        else
                        {
                            if ( LA( 1 ) == BiteLexer.ReferenceOperator )
                            {
                                callNode.Arguments.IsReference.Add( true );
                                consume();
                            }
                            else
                            {
                                callNode.Arguments.IsReference.Add( false );
                            }

                            var contextExpression = expression();

                            if ( contextExpression.Failed )
                                return Context < CallNode >.AsFailed( context.Exception );

                            callNode.Arguments.Expressions.Add( contextExpression.Result );
                        }

                        if ( LA( 1 ) == BiteLexer.CommaSeperator )
                        {
                            if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                                return matchContext;
                        }
                    }

                    if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                        return matchContext;
                }

                if ( LA( 1 ) == BiteLexer.SquarebracketLeft )
                {
                    callNode.CallType = CallTypes.PrimaryCall;

                    if ( currentCallEntry != null )
                    {
                        currentCallEntry.ElementAccess = new List < CallElementEntry >();
                    }
                    else
                    {
                        callNode.ElementAccess = new List < CallElementEntry >();
                    }

                    while ( LA( 1 ) == BiteLexer.SquarebracketLeft )
                    {
                        CallElementEntry callElementEntry = new CallElementEntry();

                        if ( !match( BiteLexer.SquarebracketLeft, out matchContext ) )
                            return matchContext;

                        if ( LA( 1 ) == BiteLexer.StringLiteral )
                        {
                            callElementEntry.Identifier = LT( 1 ).text;
                            callElementEntry.CallElementType = CallElementTypes.StringLiteral;
                            consume();
                        }
                        else if ( LA( 1 ) == BiteLexer.IntegerLiteral )
                        {
                            callElementEntry.Identifier = LT( 1 ).text;

                            callElementEntry.CallElementType =
                                CallElementTypes.IntegerLiteral;

                            consume();
                        }
                        else if ( LA( 1 ) == BiteLexer.Identifier )
                        {
                            var contextCall = call();

                            if ( contextCall.Failed )
                                return Context < CallNode >.AsFailed( context.Exception );

                            callElementEntry.Call = contextCall.Result;
                            callElementEntry.CallElementType = CallElementTypes.Call;
                        }

                        if ( !match( BiteLexer.SquarebracketRight, out matchContext ) )
                            return matchContext;

                        if ( currentCallEntry != null )
                        {
                            currentCallEntry.ElementAccess.Add( callElementEntry );
                        }
                        else
                        {
                            callNode.ElementAccess.Add( callElementEntry );
                        }
                    }
                }
            }

            return new Context < CallNode >( callNode );
        }

        return new Context < CallNode >( null );
    }

    public virtual IContext < ClassDeclarationNode > _classDeclaration()
    {
        IContext < ClassDeclarationNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic || LA( 1 ) == BiteLexer.DeclareAbstract )
            {
                consume();
            }

            if ( !match( BiteLexer.DeclareClass, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.ColonOperator && LA( 2 ) == BiteLexer.Identifier )
            {
                if ( !match( BiteLexer.ColonOperator, out matchContext ) )
                    return matchContext;

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;

                while ( LA( 1 ) == BiteLexer.CommaSeperator && LA( 2 ) == BiteLexer.Identifier )
                {
                    if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                        return matchContext;

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;
                }
            }

            var context = block();

            if ( context.Failed )
                return Context < ClassDeclarationNode >.AsFailed( context.Exception );
        }
        else
        {
            Token accessMod = null;
            Token staticAbstractMod = null;
            string classIdentifier;

            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                accessMod = LT( 1 );
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic || LA( 1 ) == BiteLexer.DeclareAbstract )
            {
                staticAbstractMod = LT( 1 );
                consume();
            }

            if ( !match( BiteLexer.DeclareClass, out matchContext ) )
                return matchContext;

            classIdentifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            List < string > baseClasses = new List < string >();

            if ( LA( 1 ) == BiteLexer.ColonOperator && LA( 2 ) == BiteLexer.Identifier )
            {
                if ( !match( BiteLexer.ColonOperator, out matchContext ) )
                    return matchContext;

                baseClasses.Add( LT( 1 ).text );

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;

                while ( LA( 1 ) == BiteLexer.CommaSeperator && LA( 2 ) == BiteLexer.Identifier )
                {
                    if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                        return matchContext;

                    baseClasses.Add( LT( 1 ).text );

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;
                }
            }

            ClassDeclarationNode classDeclarationNode = new ClassDeclarationNode();

            if ( classIdentifier != null )
            {
                classDeclarationNode.ClassId = new Identifier( classIdentifier );
            }
            else
            {
                throw new Exception( "Empty or null string as class identifier is forbidden!" );
            }

            if ( baseClasses.Count > 0 )
            {
                classDeclarationNode.Inheritance = new List < Identifier >();

                foreach ( string baseClass in baseClasses )
                {
                    classDeclarationNode.Inheritance.Add( new Identifier( baseClass ) );
                }
            }

            classDeclarationNode.Modifiers = new ModifiersNode( accessMod, staticAbstractMod );

            var context = block();

            if ( context.Failed )
                return Context < ClassDeclarationNode >.AsFailed( context.Exception );

            classDeclarationNode.BlockStatement = context.Result;

            return new Context < ClassDeclarationNode >( classDeclarationNode );
        }

        return new Context < ClassDeclarationNode >( null );
    }

    public virtual IContext < ClassDeclarationNode > _classDeclarationForward()
    {
        IContext < ClassDeclarationNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic || LA( 1 ) == BiteLexer.DeclareAbstract )
            {
                consume();
            }

            if ( !match( BiteLexer.DeclareClass, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.ColonOperator && LA( 2 ) == BiteLexer.Identifier )
            {
                if ( !match( BiteLexer.ColonOperator, out matchContext ) )
                    return matchContext;

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;

                while ( LA( 1 ) == BiteLexer.CommaSeperator && LA( 2 ) == BiteLexer.Identifier )
                {
                    if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                        return matchContext;

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;
                }
            }

            if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                return matchContext;
        }
        else
        {
            Token accessMod = null;
            Token staticAbstractMod = null;
            string classIdentifier = null;

            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                accessMod = LT( 1 );
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic || LA( 1 ) == BiteLexer.DeclareAbstract )
            {
                staticAbstractMod = LT( 1 );
                consume();
            }

            if ( !match( BiteLexer.DeclareClass, out matchContext ) )
                return matchContext;

            classIdentifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            List < string > baseClasses = new List < string >();

            if ( LA( 1 ) == BiteLexer.ColonOperator && LA( 2 ) == BiteLexer.Identifier )
            {
                if ( !match( BiteLexer.ColonOperator, out matchContext ) )
                    return matchContext;

                baseClasses.Add( LT( 1 ).text );

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;

                while ( LA( 1 ) == BiteLexer.CommaSeperator && LA( 2 ) == BiteLexer.Identifier )
                {
                    if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                        return matchContext;

                    baseClasses.Add( LT( 1 ).text );

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;
                }
            }

            ClassDeclarationNode classDeclarationNode = new ClassDeclarationNode();

            if ( classIdentifier != null )
            {
                classDeclarationNode.ClassId = new Identifier( classIdentifier );
            }
            else
            {
                throw new Exception( "Empty or null string as class identifier is forbidden!" );
            }

            if ( baseClasses.Count > 0 )
            {
                classDeclarationNode.Inheritance = new List < Identifier >();

                foreach ( string baseClass in baseClasses )
                {
                    classDeclarationNode.Inheritance.Add( new Identifier( baseClass ) );
                }
            }

            classDeclarationNode.Modifiers = new ModifiersNode( accessMod, staticAbstractMod );

            if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                return matchContext;

            return new Context < ClassDeclarationNode >( classDeclarationNode );
        }

        return new Context < ClassDeclarationNode >( null );
    }

    public virtual IContext < ClassInstanceDeclarationNode > _classInstanceDeclaration()
    {
        IContext < ClassInstanceDeclarationNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic )
            {
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareVariable )
            {
                if ( !match( BiteLexer.DeclareVariable, out matchContext ) )
                    return matchContext;
            }

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.AssignOperator, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.DeclareClassInstance, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            while ( LA( 1 ) == BiteLexer.DotOperator )
            {
                if ( !match( BiteLexer.DotOperator, out matchContext ) )
                    return matchContext;

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;
            }

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
            {
                var context = arguments();

                if ( context.Failed )
                    return Context < ClassInstanceDeclarationNode >.AsFailed( context.Exception );
            }

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            if ( MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration )
            {
                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }
        }
        else
        {
            Token accessMod = null;
            Token staticAbstractMod = null;
            string classIdentifier = null;
            string identifier = null;

            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                accessMod = LT( 1 );
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic )
            {
                staticAbstractMod = LT( 1 );
                consume();
            }

            bool isRedeclaration = true;

            if ( LA( 1 ) == BiteLexer.DeclareVariable )
            {
                isRedeclaration = false;

                if ( !match( BiteLexer.DeclareVariable, out matchContext ) )
                    return matchContext;
            }

            ClassInstanceDeclarationNode classInstanceDeclarationNode = new ClassInstanceDeclarationNode();
            classInstanceDeclarationNode.IsVariableRedeclaration = isRedeclaration;

            identifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.AssignOperator, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.DeclareClassInstance, out matchContext ) )
                return matchContext;

            classIdentifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.DotOperator )
            {
                classInstanceDeclarationNode.ClassPath = new List < Identifier >();
            }

            while ( LA( 1 ) == BiteLexer.DotOperator )
            {
                classInstanceDeclarationNode.ClassPath.Add( new Identifier( classIdentifier ) );

                if ( !match( BiteLexer.DotOperator, out matchContext ) )
                    return matchContext;

                classIdentifier = LT( 1 ).text;

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;
            }

            classInstanceDeclarationNode.ClassName = new Identifier( classIdentifier );

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            if ( !string.IsNullOrEmpty( identifier ) )
            {
                classInstanceDeclarationNode.InstanceId = new Identifier( identifier );
            }
            else
            {
                throw new Exception( "Empty or null string as class identifier is forbidden!" );
            }

            classInstanceDeclarationNode.Modifiers = new ModifiersNode( accessMod, staticAbstractMod );

            if ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
            {
                var context = arguments();

                if ( context.Failed )
                    return Context < ClassInstanceDeclarationNode >.AsFailed( context.Exception );

                classInstanceDeclarationNode.Arguments = context.Result;
            }

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            if ( MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration )
            {
                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }

            return new Context < ClassInstanceDeclarationNode >( classInstanceDeclarationNode );
        }

        return new Context < ClassInstanceDeclarationNode >( null );
    }

    public IContext < HeteroAstNode > _declaration()
    {
        /*if ( speculate_declaration_namespace() )
        {
            // Console.WriteLine( "predict alternative namespace" );

            return namespaceDeclaration();
        }*/

        if ( speculate_declaration_class() )
        {
            // Console.WriteLine( "predict alternative class" );

            return classDeclaration();
        }

        if ( speculate_declaration_class_forward() )
        {
            // Console.WriteLine( "predict alternative class forward" );

            return classDeclarationForward();
        }

        if ( speculate_declaration_struct() )
        {
            // Console.WriteLine( "predict alternative struct" );

            return structDeclaration();
        }

        if ( speculate_declaration_function() )
        {
            // Console.WriteLine( "predict alternative function" );

            return functionDeclaration();
        }

        if ( speculate_declaration_function_forward() )
        {
            // Console.WriteLine( "predict alternative function forward" );

            return functionDeclarationForward();
        }

        if ( speculate_declaration_class_instance() )
        {
            // Console.WriteLine( "predict alternative class instance" );

            return classInstanceDeclaration();
        }

        if ( speculate_declaration_variable() )
        {
            // Console.WriteLine( "predict alternative variable declaration" );

            return variableDeclaration();
        }

        if ( speculate_statement() )
        {
            // Console.WriteLine( "predict alternative statement" );

            return statement();
        }

        return Context < AssignmentNode >.AsFailed(
            new NoViableAltException( "expecting declaration found ", LT( 1 ) ) );

        // throw new NoViableAltException("expecting declaration found " + LT(1));
    }

    public virtual IContext < ExpressionNode > _equality()
    {
        if ( Speculating )
        {
            var context = relational();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.UnequalOperator || LA( 1 ) == BiteLexer.EqualOperator )
            {
                consume();
                context = relational();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();

            var context = relational();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;

            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.UnequalOperator || LA( 1 ) == BiteLexer.EqualOperator )
            {
                if ( LA( 1 ) == BiteLexer.UnequalOperator )
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.NotEqual;
                }
                else
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Equal;
                }

                consume();

                context = relational();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.UnequalOperator || LA( 1 ) == BiteLexer.EqualOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < ExpressionNode > _expression()
    {
        if ( Speculating )
        {
            var context = assignment();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );
        }
        else
        {
            ExpressionNode expressionNode = new ExpressionNode();

            var context = assignment();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            expressionNode.Assignment = context.Result;

            return new Context < ExpressionNode >( expressionNode );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < ExpressionStatementNode > _expressionStatement()
    {
        IContext < ExpressionStatementNode > matchContext = null;

        if ( Speculating )
        {
            var context = expression();

            if ( context.Failed )
                return Context < ExpressionStatementNode >.AsFailed( context.Exception );

            if ( MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration )
            {
                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }
        }
        else
        {
            ExpressionStatementNode expressionStatementNode = new ExpressionStatementNode();

            var context = expression();

            if ( context.Failed )
                return Context < ExpressionStatementNode >.AsFailed( context.Exception );

            expressionStatementNode.Expression = context.Result;

            if ( MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration )
            {
                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }

            return new Context < ExpressionStatementNode >( expressionStatementNode );
        }

        return new Context < ExpressionStatementNode >( null );
    }

    public virtual IContext < ForStatementNode > _forStatement()
    {
        IContext < ForStatementNode > matchContext = null;

        if ( Speculating )
        {
            if ( !match( BiteLexer.DeclareForLoop, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.DeclareVariable )
            {
                var context = variableDeclaration();

                if ( context.Failed )
                    return Context < ForStatementNode >.AsFailed( context.Exception );
            }
            else
            {
                if ( LA( 1 ) != BiteLexer.SemicolonSeperator )
                {
                    var context = expressionStatement();

                    if ( context.Failed )
                        return Context < ForStatementNode >.AsFailed( context.Exception );
                }
                else
                {
                    if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                        return matchContext;
                }
            }

            if ( LA( 1 ) != BiteLexer.SemicolonSeperator )
            {
                var context = expression();

                if ( context.Failed )
                    return Context < ForStatementNode >.AsFailed( context.Exception );

                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }
            else
            {
                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }

            if ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
            {
                var context = expression();

                if ( context.Failed )
                    return Context < ForStatementNode >.AsFailed( context.Exception );
            }

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < ForStatementNode >.AsFailed( contextBlock.Exception );
        }
        else
        {
            ForStatementNode forStatementNode = new ForStatementNode();

            if ( !match( BiteLexer.DeclareForLoop, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            if ( speculate_declaration_variable() )
            {
                // Console.WriteLine( "predict variable declaration" );
                var context = variableDeclaration();

                if ( context.Failed )
                    return Context < ForStatementNode >.AsFailed( context.Exception );

                forStatementNode.VariableDeclaration = context.Result;
            }
            else if ( speculate_expression_statement() )
            {
                // Console.WriteLine( "predict alternative for expression statement" );
                var context = expressionStatement();

                if ( context.Failed )
                    return Context < ForStatementNode >.AsFailed( context.Exception );

                forStatementNode.ExpressionStatement = context.Result;
            }
            else
            {
                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }

            if ( speculate_expression() )
            {
                // Console.WriteLine( "predict alternative for expression" );
                var context = expression();

                if ( context.Failed )
                    return Context < ForStatementNode >.AsFailed( context.Exception );

                forStatementNode.Expression1 = context.Result;

                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }
            else
            {
                if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                    return matchContext;
            }

            if ( speculate_expression() )
            {
                // Console.WriteLine( "predict alternative for expression" );
                var context = expression();

                if ( context.Failed )
                    return Context < ForStatementNode >.AsFailed( context.Exception );

                forStatementNode.Expression2 = context.Result;
            }

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < ForStatementNode >.AsFailed( contextBlock.Exception );

            forStatementNode.Block = contextBlock.Result;

            return new Context < ForStatementNode >( forStatementNode );
        }

        return new Context < ForStatementNode >( null );
    }

    public virtual IContext < FunctionDeclarationNode > _functionDeclaration()
    {
        IContext < FunctionDeclarationNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic || LA( 1 ) == BiteLexer.DeclareAbstract )
            {
                consume();
            }

            if ( !match( BiteLexer.DeclareFunction, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.Identifier )
            {
                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;

                while ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
                {
                    if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                        return matchContext;

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;
                }
            }

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < FunctionDeclarationNode >.AsFailed( contextBlock.Exception );
        }
        else
        {
            Token accessMod = null;
            Token staticAbstractMod = null;
            string functionIdentifier = null;
            ParametersNode parametersNode = new ParametersNode();

            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                accessMod = LT( 1 );
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic || LA( 1 ) == BiteLexer.DeclareAbstract )
            {
                staticAbstractMod = LT( 1 );
                consume();
            }

            if ( !match( BiteLexer.DeclareFunction, out matchContext ) )
                return matchContext;

            functionIdentifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.Identifier )
            {
                parametersNode.Identifiers = new List < Identifier >();
                parametersNode.Identifiers.Add( new Identifier( LT( 1 ).text ) );

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;

                while ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
                {
                    if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                        return matchContext;

                    parametersNode.Identifiers.Add( new Identifier( LT( 1 ).text ) );

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;
                }
            }

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            FunctionDeclarationNode functionDeclarationNode = new FunctionDeclarationNode();

            if ( functionIdentifier != null )
            {
                functionDeclarationNode.FunctionId = new Identifier( functionIdentifier );
            }
            else
            {
                throw new Exception( "Empty or null string as class identifier is forbidden!" );
            }

            functionDeclarationNode.Modifiers = new ModifiersNode( accessMod, staticAbstractMod );

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < FunctionDeclarationNode >.AsFailed( contextBlock.Exception );

            functionDeclarationNode.FunctionBlock = contextBlock.Result;
            functionDeclarationNode.Parameters = parametersNode;

            return new Context < FunctionDeclarationNode >( functionDeclarationNode );
        }

        return new Context < FunctionDeclarationNode >( null );
    }

    public virtual IContext < FunctionDeclarationNode > _functionDeclarationForward()
    {
        IContext < FunctionDeclarationNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic || LA( 1 ) == BiteLexer.DeclareAbstract )
            {
                consume();
            }

            if ( !match( BiteLexer.DeclareFunction, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.Identifier )
            {
                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;

                while ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
                {
                    if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                        return matchContext;

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;
                }
            }

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                return matchContext;
        }
        else
        {
            Token accessMod = null;
            Token staticAbstractMod = null;
            string functionIdentifier = null;
            ParametersNode parametersNode = new ParametersNode();
            parametersNode.Identifiers = new List < Identifier >();

            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                accessMod = LT( 1 );
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic || LA( 1 ) == BiteLexer.DeclareAbstract )
            {
                staticAbstractMod = LT( 1 );
                consume();
            }

            if ( !match( BiteLexer.DeclareFunction, out matchContext ) )
                return matchContext;

            functionIdentifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.Identifier )
            {
                parametersNode.Identifiers.Add( new Identifier( LT( 1 ).text ) );

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;

                while ( LA( 1 ) != BiteLexer.ClosingRoundBracket )
                {
                    if ( !match( BiteLexer.CommaSeperator, out matchContext ) )
                        return matchContext;

                    parametersNode.Identifiers.Add( new Identifier( LT( 1 ).text ) );

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;
                }
            }

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            FunctionDeclarationNode functionDeclarationNode = new FunctionDeclarationNode();

            if ( functionIdentifier != null )
            {
                functionDeclarationNode.FunctionId = new Identifier( functionIdentifier );
            }
            else
            {
                throw new Exception( "Empty or null string as class identifier is forbidden!" );
            }

            functionDeclarationNode.Modifiers = new ModifiersNode( accessMod, staticAbstractMod );

            if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                return matchContext;

            return new Context < FunctionDeclarationNode >( functionDeclarationNode );
        }

        return new Context < FunctionDeclarationNode >( null );
    }

    public virtual IContext < IfStatementNode > _ifStatement()
    {
        IContext < IfStatementNode > matchContext = null;

        if ( Speculating )
        {
            if ( !match( BiteLexer.ControlFlowIf, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            var context = expression();

            if ( context.Failed )
                return Context < IfStatementNode >.AsFailed( context.Exception );

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < IfStatementNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.ControlFlowElse )
            {
                if ( !match( BiteLexer.ControlFlowElse, out matchContext ) )
                    return matchContext;

                if ( LA( 1 ) == BiteLexer.ControlFlowIf )
                {
                    if ( !match( BiteLexer.ControlFlowIf, out matchContext ) )
                        return matchContext;

                    if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                        return matchContext;

                    context = expression();

                    if ( context.Failed )
                        return Context < IfStatementNode >.AsFailed( context.Exception );

                    if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                        return matchContext;

                    contextBlock = block();

                    if ( contextBlock.Failed )
                        return Context < IfStatementNode >.AsFailed( context.Exception );
                }
                else
                {
                    contextBlock = block();

                    if ( contextBlock.Failed )
                        return Context < IfStatementNode >.AsFailed( context.Exception );
                }
            }
        }
        else
        {
            if ( !match( BiteLexer.ControlFlowIf, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            IfStatementNode ifStatement = new IfStatementNode();

            var context = expression();

            if ( context.Failed )
                return Context < IfStatementNode >.AsFailed( context.Exception );

            ifStatement.Expression = context.Result;

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < IfStatementNode >.AsFailed( context.Exception );

            ifStatement.ThenBlock = contextBlock.Result;

            ifStatement.IfStatementEntries = new List < IfStatementEntry >();

            while ( LA( 1 ) == BiteLexer.ControlFlowElse )
            {
                IfStatementEntry ifStatementEntry = new IfStatementEntry();

                if ( !match( BiteLexer.ControlFlowElse, out matchContext ) )
                    return matchContext;

                if ( LA( 1 ) == BiteLexer.ControlFlowIf )
                {
                    ifStatementEntry.IfStatementType = IfStatementEntryType.ElseIf;

                    if ( !match( BiteLexer.ControlFlowIf, out matchContext ) )
                        return matchContext;

                    if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                        return matchContext;

                    var context2 = expression();

                    if ( context2.Failed )
                        return Context < IfStatementNode >.AsFailed( context.Exception );

                    ifStatementEntry.ExpressionElseIf = context2.Result;

                    if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                        return matchContext;

                    var contextBlock2 = block();

                    if ( contextBlock2.Failed )
                        return Context < IfStatementNode >.AsFailed( context.Exception );

                    ifStatementEntry.ElseBlock = contextBlock2.Result;
                }
                else
                {
                    ifStatementEntry.IfStatementType = IfStatementEntryType.Else;

                    var contextBlock2 = block();

                    if ( contextBlock2.Failed )
                        return Context < IfStatementNode >.AsFailed( context.Exception );

                    ifStatementEntry.ElseBlock = contextBlock2.Result;
                }

                ifStatement.IfStatementEntries.Add( ifStatementEntry );
            }

            return new Context < IfStatementNode >( ifStatement );
        }

        return new Context < IfStatementNode >( null );
    }

    public virtual IContext < ExpressionNode > _logicAnd()
    {
        if ( Speculating )
        {
            var context = bitwiseOr();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.LogicalAndOperator )
            {
                consume();
                context = bitwiseOr();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();

            var context = bitwiseOr();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;

            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.LogicalAndOperator )
            {
                currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.And;
                consume();

                context = bitwiseOr();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.LogicalAndOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < ExpressionNode > _logicOr()
    {
        if ( Speculating )
        {
            var context = logicAnd();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.LogicalOrOperator )
            {
                consume();
                context = logicAnd();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();

            var context = logicAnd();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;

            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.LogicalOrOperator )
            {
                currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Or;
                consume();

                context = logicAnd();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.LogicalOrOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < ExpressionNode > _multiplicative()
    {
        if ( Speculating )
        {
            var context = unary();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.MultiplyOperator ||
                    LA( 1 ) == BiteLexer.DivideOperator ||
                    LA( 1 ) == BiteLexer.ModuloOperator )
            {
                consume();
                context = unary();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();

            var context = unary();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;
            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.MultiplyOperator ||
                    LA( 1 ) == BiteLexer.DivideOperator ||
                    LA( 1 ) == BiteLexer.ModuloOperator )
            {
                if ( LA( 1 ) == BiteLexer.MultiplyOperator )
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Mult;
                }
                else if ( LA( 1 ) == BiteLexer.DivideOperator )
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Div;
                }
                else
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Modulo;
                }

                consume();

                context = unary();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.MultiplyOperator ||
                     LA( 1 ) == BiteLexer.DivideOperator ||
                     LA( 1 ) == BiteLexer.ModuloOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < PrimaryNode > _primary()
    {
        IContext < PrimaryNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.BooleanLiteral ||
                 LA( 1 ) == BiteLexer.NullReference ||
                 LA( 1 ) == BiteLexer.ThisReference ||
                 LA( 1 ) == BiteLexer.IntegerLiteral ||
                 LA( 1 ) == BiteLexer.FloatingLiteral ||
                 LA( 1 ) == BiteLexer.StringLiteral ||
                 LA( 1 ) == BiteLexer.Identifier )
            {
                consume();
            }
            else if ( LA( 1 ) == BiteLexer.OpeningRoundBracket )
            {
                if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                    return matchContext;

                MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = false;

                var context = declaration();

                if ( context.Failed )
                    return Context < PrimaryNode >.AsFailed( context.Exception );

                MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;

                if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                    return matchContext;
            }
        }
        else
        {
            PrimaryNode primaryNode = new PrimaryNode();

            if ( LA( 1 ) == BiteLexer.BooleanLiteral )
            {
                primaryNode.BooleanLiteral = bool.Parse( LT( 1 ).text );
                primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.BooleanLiteral;
                consume();
            }
            else if ( LA( 1 ) == BiteLexer.NullReference )
            {
                primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.NullReference;
                consume();
            }
            else if ( LA( 1 ) == BiteLexer.ThisReference )
            {
                primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.ThisReference;
                consume();
            }
            else if ( LA( 1 ) == BiteLexer.IntegerLiteral )
            {
                primaryNode.IntegerLiteral = int.Parse( LT( 1 ).text );
                primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.IntegerLiteral;
                consume();
            }
            else if ( LA( 1 ) == BiteLexer.FloatingLiteral )
            {
                primaryNode.FloatLiteral = double.Parse( LT( 1 ).text, CultureInfo.InvariantCulture );
                primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.FloatLiteral;
                consume();
            }
            else if ( LA( 1 ) == BiteLexer.StringLiteral )
            {
                primaryNode.StringLiteral = LT( 1 ).text;
                primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.StringLiteral;
                consume();
            }
            else if ( LA( 1 ) == BiteLexer.Identifier )
            {
                primaryNode.PrimaryId = new Identifier( LT( 1 ).text );
                primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Identifier;
                consume();
            }
            else if ( LA( 1 ) == BiteLexer.OpeningRoundBracket )
            {
                if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                    return matchContext;

                MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = false;

                var context = declaration();

                if ( context.Failed )
                    return Context < PrimaryNode >.AsFailed( context.Exception );

                primaryNode.Expression = context.Result;
                MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;
                primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Expression;

                if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                    return matchContext;
            }
            else
            {
                return Context < PrimaryNode >.AsFailed(
                    new NoViableAltException( "expecting primary found ", LT( 1 ) ) );

                //throw new NoViableAltException(LT(1).text);
            }

            return new Context < PrimaryNode >( primaryNode );
        }

        return new Context < PrimaryNode >( null );
    }

    public virtual IContext < ExpressionNode > _relational()
    {
        if ( Speculating )
        {
            var context = shift();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.GreaterOperator ||
                    LA( 1 ) == BiteLexer.GreaterEqualOperator ||
                    LA( 1 ) == BiteLexer.SmallerOperator ||
                    LA( 1 ) == BiteLexer.SmallerEqualOperator )
            {
                consume();
                context = shift();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();

            var context = shift();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;

            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.GreaterOperator ||
                    LA( 1 ) == BiteLexer.GreaterEqualOperator ||
                    LA( 1 ) == BiteLexer.SmallerOperator ||
                    LA( 1 ) == BiteLexer.SmallerEqualOperator )
            {
                if ( LA( 1 ) == BiteLexer.GreaterOperator )
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Greater;
                }
                else if ( LA( 1 ) == BiteLexer.GreaterEqualOperator )
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.GreaterOrEqual;
                }
                else if ( LA( 1 ) == BiteLexer.SmallerOperator )
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Less;
                }
                else
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.LessOrEqual;
                }

                consume();

                context = shift();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.GreaterOperator ||
                     LA( 1 ) == BiteLexer.GreaterEqualOperator ||
                     LA( 1 ) == BiteLexer.SmallerOperator ||
                     LA( 1 ) == BiteLexer.SmallerEqualOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < ReturnStatementNode > _returnStatement()
    {
        IContext < ReturnStatementNode > matchContext = null;

        if ( Speculating )
        {
            if ( !match( BiteLexer.FunctionReturn, out matchContext ) )
                return matchContext;

            var context = expressionStatement();

            if ( context.Failed )
                return Context < ReturnStatementNode >.AsFailed( context.Exception );
        }
        else
        {
            if ( !match( BiteLexer.FunctionReturn, out matchContext ) )
                return matchContext;

            ReturnStatementNode returnStatementNode = new ReturnStatementNode();
            var context = expressionStatement();

            if ( context.Failed )
                return Context < ReturnStatementNode >.AsFailed( context.Exception );

            returnStatementNode.ExpressionStatement = context.Result;

            return new Context < ReturnStatementNode >( returnStatementNode );
        }

        return new Context < ReturnStatementNode >( null );
    }
    
    public virtual IContext < BreakStatementNode > _breakStatement()
    {
        IContext < BreakStatementNode > matchContext = null;

        if ( Speculating )
        {
            if ( !match( BiteLexer.Break, out matchContext ) )
                return matchContext;
            
            if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                return matchContext;
        }
        else
        {
            if ( !match( BiteLexer.Break, out matchContext ) )
                return matchContext;
    
            if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                return matchContext;
            BreakStatementNode returnStatementNode = new BreakStatementNode();

            return new Context < BreakStatementNode >( returnStatementNode );
        }
        return new Context < BreakStatementNode >( null );
    }

    public IContext < HeteroAstNode > _statement()
    {
        if ( speculate_using_statement() )
        {
            // Console.WriteLine( "predict expression statement" );

            return usingStatement();
        }

        if ( speculate_expression_statement() )
        {
            // Console.WriteLine( "predict expression statement" );

            return expressionStatement();
        }

        if ( speculate_for_statement() )
        {
            // Console.WriteLine( "predict alternative for statement" );

            return forStatement();
        }

        if ( speculate_if_statement() )
        {
            // Console.WriteLine( "predict alternative if statement" );

            return ifStatement();
        }

        if ( speculate_return_statement() )
        {
            // Console.WriteLine( "predict alternative return statement" );

            return returnStatement();
        }
        
        if ( speculate_break_statement() )
        {
            // Console.WriteLine( "predict alternative break statement" );

            return breakStatement();
        }

        if ( speculate_while_statement() )
        {
            // Console.WriteLine( "predict alternative while statement" );

            return whileStatement();
        }

        if ( speculate_block() )
        {
            // Console.WriteLine( "predict alternative block statement" );

            return block();
        }

        return Context < HeteroAstNode >.AsFailed(
            new NoViableAltException( "expecting declaration found ", LT( 1 ) ) );

        //throw new NoViableAltException("expecting declaration found " + LT(1));
    }

    public IContext < UsingStatementNode > _usingStatement()
    {
        IContext < UsingStatementNode > matchContext = null;

        if ( !Speculating )
        {
            if ( !match( BiteLexer.UsingDirective, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            UsingStatementNode usingStatementNode = new UsingStatementNode();
            MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = false;

            var context = declaration();

            if ( context.Failed )
                return Context < UsingStatementNode >.AsFailed( context.Exception );

            usingStatementNode.UsingNode = context.Result;

            MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < UsingStatementNode >.AsFailed( context.Exception );

            usingStatementNode.UsingBlock = contextBlock.Result;

            return new Context < UsingStatementNode >( usingStatementNode );
        }

        if ( !match( BiteLexer.UsingDirective, out matchContext ) )
            return matchContext;

        if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
            return matchContext;

        MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = false;

        var context2 = declaration();

        if ( context2.Failed )
            return Context < UsingStatementNode >.AsFailed( context2.Exception );

        MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;

        if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
            return matchContext;

        var contextBlock2 = block();

        if ( contextBlock2.Failed )
            return Context < UsingStatementNode >.AsFailed( contextBlock2.Exception );

        return new Context < UsingStatementNode >( null );
    }

    public virtual IContext < StructDeclarationNode > _structDeclaration()
    {
        IContext < StructDeclarationNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                consume();
            }

            if ( !match( BiteLexer.DeclareStruct, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            var context = block();

            if ( context.Failed )
                return Context < StructDeclarationNode >.AsFailed( context.Exception );
        }
        else
        {
            Token accessMod = null;
            Token staticAbstractMod = null;
            string classIdentifier = null;

            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                accessMod = LT( 1 );
                consume();
            }

            if ( !match( BiteLexer.DeclareStruct, out matchContext ) )
                return matchContext;

            classIdentifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            StructDeclarationNode structDeclarationNode = new StructDeclarationNode();

            if ( classIdentifier != null )
            {
                structDeclarationNode.StructId = new Identifier( classIdentifier );
            }
            else
            {
                throw new Exception( "Empty or null string as class identifier is forbidden!" );
            }

            structDeclarationNode.Modifiers = new ModifiersNode( accessMod, staticAbstractMod );

            var context = block();

            if ( context.Failed )
                return Context < StructDeclarationNode >.AsFailed( context.Exception );

            structDeclarationNode.Block = context.Result;

            return new Context < StructDeclarationNode >( structDeclarationNode );
        }

        return null;
    }

    public virtual IContext < ExpressionNode > _unary()
    {
        if ( speculate_unary_prefix() )
        {
            // Console.WriteLine( "predict unary prefix" );

            return unaryPrefix();
        }

        if ( speculate_call() )
        {
            // Console.WriteLine( "predict call" );

            return call();
        }

        if ( speculate_unary_postfix() )
        {
            // Console.WriteLine( "predict unary postfix" );

            return unaryPostfix();
        }

        return Context < ExpressionNode >.AsFailed( new NoViableAltException( "expecting unary found ", LT( 1 ) ) );

        //throw new NoViableAltException("");
    }

    public virtual IContext < ExpressionNode > _unaryPostfix()
    {
        IContext < ExpressionNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.Identifier )
            {
                consume();
            }

            if ( LA( 1 ) == BiteLexer.MinusMinusOperator ||
                 LA( 1 ) == BiteLexer.PlusPlusOperator )
            {
                consume();
            }
            else
            {
                //throw new NoViableAltException("");
                return Context < ExpressionNode >.AsFailed( new NoViableAltException( "", LT( 1 ) ) );
            }
        }
        else
        {
            UnaryPostfixOperation unaryPostfix = new UnaryPostfixOperation();

            var context = call();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            unaryPostfix.Primary = context.Result;

            if ( LA( 1 ) == BiteLexer.MinusMinusOperator ||
                 LA( 1 ) == BiteLexer.PlusPlusOperator )
            {
                if ( LA( 1 ) == BiteLexer.MinusMinusOperator )
                {
                    if ( !match( BiteLexer.MinusMinusOperator, out matchContext ) )
                        return matchContext;

                    unaryPostfix.Operator = UnaryPostfixOperation.UnaryPostfixOperatorType.MinusMinus;
                }
                else if ( LA( 1 ) == BiteLexer.PlusPlusOperator )
                {
                    if ( !match( BiteLexer.PlusPlusOperator, out matchContext ) )
                        return matchContext;

                    unaryPostfix.Operator = UnaryPostfixOperation.UnaryPostfixOperatorType.PlusPlus;
                }
            }

            return new Context < ExpressionNode >( unaryPostfix );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < ExpressionNode > _unaryPrefix()
    {
        IContext < ExpressionNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.MinusMinusOperator ||
                 LA( 1 ) == BiteLexer.PlusPlusOperator ||
                 LA( 1 ) == BiteLexer.LogicalNegationOperator ||
                 LA( 1 ) == BiteLexer.MinusOperator ||
                 LA( 1 ) == BiteLexer.ComplimentOperator ||
                 LA( 1 ) == BiteLexer.PlusOperator )
            {
                consume();

                var context = unary();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
            else
            {
                return Context < ExpressionNode >.AsFailed( new NoViableAltException( "", LT( 1 ) ) );

                //throw new NoViableAltException("");
            }
        }
        else
        {
            UnaryPrefixOperation unaryPrefix = new UnaryPrefixOperation();

            if ( LA( 1 ) == BiteLexer.MinusMinusOperator ||
                 LA( 1 ) == BiteLexer.PlusPlusOperator ||
                 LA( 1 ) == BiteLexer.LogicalNegationOperator ||
                 LA( 1 ) == BiteLexer.MinusOperator ||
                 LA( 1 ) == BiteLexer.ComplimentOperator ||
                 LA( 1 ) == BiteLexer.PlusOperator )
            {
                if ( LA( 1 ) == BiteLexer.MinusMinusOperator )
                {
                    if ( !match( BiteLexer.MinusMinusOperator, out matchContext ) )
                        return matchContext;

                    unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.MinusMinus;
                }
                else if ( LA( 1 ) == BiteLexer.PlusPlusOperator )
                {
                    if ( !match( BiteLexer.PlusPlusOperator, out matchContext ) )
                        return matchContext;

                    unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.PlusPlus;
                }
                else if ( LA( 1 ) == BiteLexer.LogicalNegationOperator )
                {
                    if ( !match( BiteLexer.LogicalNegationOperator, out matchContext ) )
                        return matchContext;

                    unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.LogicalNot;
                }
                else if ( LA( 1 ) == BiteLexer.ComplimentOperator )
                {
                    if ( !match( BiteLexer.ComplimentOperator, out matchContext ) )
                        return matchContext;

                    unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.Compliment;
                }
                else if ( LA( 1 ) == BiteLexer.PlusOperator )
                {
                    if ( !match( BiteLexer.PlusOperator, out matchContext ) )
                        return matchContext;

                    unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.Plus;
                }
                else
                {
                    if ( !match( BiteLexer.MinusOperator, out matchContext ) )
                        return matchContext;

                    unaryPrefix.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.Negate;
                }

                var context = unary();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                unaryPrefix.Primary = context.Result;
            }

            return new Context < ExpressionNode >( unaryPrefix );
        }

        return new Context < ExpressionNode >( null );
    }

    public virtual IContext < VariableDeclarationNode > _variableDeclaration()
    {
        IContext < VariableDeclarationNode > matchContext = null;

        if ( Speculating )
        {
            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic )
            {
                consume();
            }

            if ( !match( BiteLexer.DeclareVariable, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            if ( LA( 1 ) == BiteLexer.AssignOperator )
            {
                consume();

                if ( LA( 1 ) == BiteLexer.DeclareClassInstance )
                {
                    //throw new MismatchedTokenException("Got " + LA(1) + ". Expected Variable Declaration");
                    return Context < VariableDeclarationNode >.AsFailed(
                        new MismatchedTokenException( "Expected variableDeclaration found", LT( 1 ) ) );
                }

                var context = expressionStatement();

                if ( context.Failed )
                    return Context < VariableDeclarationNode >.AsFailed( context.Exception );
            }
            else
            {
                if ( MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration )
                {
                    if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                        return matchContext;
                }
            }
        }
        else
        {
            Token accessMod = null;
            Token staticAbstractMod = null;
            string identifier = null;

            if ( LA( 1 ) == BiteLexer.DeclarePrivate || LA( 1 ) == BiteLexer.DeclarePublic )
            {
                accessMod = LT( 1 );
                consume();
            }

            if ( LA( 1 ) == BiteLexer.DeclareStatic )
            {
                staticAbstractMod = LT( 1 );
                consume();
            }

            if ( !match( BiteLexer.DeclareVariable, out matchContext ) )
                return matchContext;

            identifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            ExpressionStatementNode init = null;

            if ( LA( 1 ) == BiteLexer.AssignOperator )
            {
                consume();

                if ( LA( 1 ) == BiteLexer.DeclareClassInstance )
                {
                    //throw new MismatchedTokenException("Got " + LA(1) + ". Expected Variable Declaration");
                    return Context < VariableDeclarationNode >.AsFailed(
                        new MismatchedTokenException( "Expected variableDeclaration found", LT( 1 ) ) );
                }

                var context = expressionStatement();

                if ( context.Failed )
                    return Context < VariableDeclarationNode >.AsFailed( context.Exception );

                init = context.Result;
            }
            else
            {
                if ( MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration )
                {
                    if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                        return matchContext;
                }
            }

            VariableDeclarationNode variableDeclarationNode = new VariableDeclarationNode();

            if ( !string.IsNullOrEmpty( identifier ) )
            {
                variableDeclarationNode.VarId = new Identifier( identifier );
            }
            else
            {
                throw new Exception( "Empty or null string as class identifier is forbidden!" );
            }

            variableDeclarationNode.Modifiers = new ModifiersNode( accessMod, staticAbstractMod );

            if ( init != null )
            {
                variableDeclarationNode.Initializer = new InitializerNode();
                variableDeclarationNode.Initializer.Expression = init;
            }

            return new Context < VariableDeclarationNode >( variableDeclarationNode );
        }

        return new Context < VariableDeclarationNode >( null );
    }

    public virtual IContext < WhileStatementNode > _whileStatement()
    {
        IContext < WhileStatementNode > matchContext = null;

        if ( Speculating )
        {
            if ( !match( BiteLexer.DeclareWhileLoop, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            var context = expression();

            if ( context.Failed )
                return Context < WhileStatementNode >.AsFailed( context.Exception );

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < WhileStatementNode >.AsFailed( context.Exception );
        }
        else
        {
            if ( !match( BiteLexer.DeclareWhileLoop, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.OpeningRoundBracket, out matchContext ) )
                return matchContext;

            WhileStatementNode whileStatementNode = new WhileStatementNode();

            var context = expression();

            if ( context.Failed )
                return Context < WhileStatementNode >.AsFailed( context.Exception );

            whileStatementNode.Expression = context.Result;

            if ( !match( BiteLexer.ClosingRoundBracket, out matchContext ) )
                return matchContext;

            var contextBlock = block();

            if ( contextBlock.Failed )
                return Context < WhileStatementNode >.AsFailed( context.Exception );

            whileStatementNode.WhileBlock = contextBlock.Result;

            return new Context < WhileStatementNode >( whileStatementNode );
        }

        return new Context < WhileStatementNode >( null );
    }

    public IContext < ExpressionNode > _ternary()
    {
        IContext < ExpressionNode > matchContext = null;

        if ( Speculating )
        {
            var context = logicOr();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.QuestionMarkOperator )
            {
                consume();

                context = logicOr();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                if ( !match( BiteLexer.ColonOperator, out matchContext ) )
                    return matchContext;

                context = logicOr();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            TernaryOperationNode firstOperationNode = new TernaryOperationNode();

            var context = logicOr();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;

            TernaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.QuestionMarkOperator )
            {
                consume();

                context = logicOr();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                currentOperationNode.MidOperand = context.Result;

                if ( !match( BiteLexer.ColonOperator, out matchContext ) )
                    return matchContext;

                context = logicOr();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode right = context.Result;

                if ( LA( 1 ) == BiteLexer.QuestionMarkOperator )
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

            if ( firstOperationNode.RightOperand != null || firstOperationNode.MidOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    private IContext < ExpressionNode > _bitwiseOr()
    {
        if ( Speculating )
        {
            var context = bitwiseXor();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.BitwiseOrOperator )
            {
                consume();
                context = bitwiseXor();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();
            var context = bitwiseXor();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;
            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.BitwiseOrOperator )
            {
                currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseOr;
                consume();
                context = bitwiseXor();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.BitwiseOrOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    private IContext < ExpressionNode > _bitwiseXor()
    {
        if ( Speculating )
        {
            var context = bitwiseAnd();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.BitwiseXorOperator )
            {
                consume();
                context = bitwiseAnd();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();
            var context = bitwiseAnd();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;
            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.BitwiseXorOperator )
            {
                currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseXor;
                consume();

                context = bitwiseAnd();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.BitwiseXorOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    private IContext < ExpressionNode > _bitwiseAnd()
    {
        if ( Speculating )
        {
            var context = equality();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.BitwiseAndOperator )
            {
                consume();
                context = equality();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();
            var context = equality();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;
            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.BitwiseAndOperator )
            {
                currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseAnd;
                consume();
                context = equality();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.BitwiseAndOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    private IContext < ExpressionNode > _shift()
    {
        if ( Speculating )
        {
            var context = additive();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            while ( LA( 1 ) == BiteLexer.ShiftLeftOperator || LA( 1 ) == BiteLexer.ShiftRightOperator )
            {
                consume();
                context = additive();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );
            }
        }
        else
        {
            BinaryOperationNode firstOperationNode = new BinaryOperationNode();
            var context = additive();

            if ( context.Failed )
                return Context < ExpressionNode >.AsFailed( context.Exception );

            firstOperationNode.LeftOperand = context.Result;
            BinaryOperationNode currentOperationNode = firstOperationNode;

            while ( LA( 1 ) == BiteLexer.ShiftLeftOperator || LA( 1 ) == BiteLexer.ShiftRightOperator )
            {
                if ( LA( 1 ) == BiteLexer.ShiftLeftOperator )
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.ShiftLeft;
                }
                else
                {
                    currentOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.ShiftRight;
                }

                consume();

                context = additive();

                if ( context.Failed )
                    return Context < ExpressionNode >.AsFailed( context.Exception );

                ExpressionNode expressionNode = context.Result;
                currentOperationNode.RightOperand = expressionNode;

                if ( LA( 1 ) == BiteLexer.ShiftLeftOperator || LA( 1 ) == BiteLexer.ShiftRightOperator )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentOperationNode;
                    currentOperationNode = binaryOperationNode;
                }
                else
                {
                    firstOperationNode = currentOperationNode;
                }
            }

            if ( firstOperationNode.RightOperand != null )
            {
                return new Context < ExpressionNode >( firstOperationNode );
            }

            return new Context < ExpressionNode >( firstOperationNode.LeftOperand );
        }

        return new Context < ExpressionNode >( null );
    }

    private IContext < ModuleNode > _module()
    {
        IContext < ModuleNode > matchContext = null;

        if ( !Speculating )
        {
            if ( !match( BiteLexer.DeclareModule, out matchContext ) )
                return matchContext;

            string moduleIdentifier = LT( 1 ).text;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;

            List < string > parentModules = new List < string >();
            List < ModuleIdentifier > importedModules = new List < ModuleIdentifier >();
            List < ModuleIdentifier > usedModules = new List < ModuleIdentifier >();

            ModuleNode moduleNode = new ModuleNode();

            while ( LA( 1 ) == BiteLexer.DotOperator )
            {
                parentModules.Add( moduleIdentifier );

                if ( !match( BiteLexer.DotOperator, out matchContext ) )
                    return matchContext;

                moduleIdentifier = LT( 1 ).text;

                if ( !match( BiteLexer.Identifier, out matchContext ) )
                    return matchContext;
            }

            if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                return matchContext;

            moduleNode.ModuleIdent = new ModuleIdentifier( moduleIdentifier, parentModules );

            if ( LA( 1 ) == BiteLexer.ImportDirective || LA( 1 ) == BiteLexer.UsingDirective )
            {
                while ( LA( 1 ) == BiteLexer.ImportDirective || LA( 1 ) == BiteLexer.UsingDirective && LA( 2 ) != BiteLexer.OpeningRoundBracket )
                {
                    if ( LA( 1 ) == BiteLexer.ImportDirective )
                    {
                        if ( !match( BiteLexer.ImportDirective, out matchContext ) )
                            return matchContext;

                        string nextIdentifier = LT( 1 ).text;

                        if ( !match( BiteLexer.Identifier, out matchContext ) )
                            return matchContext;

                        List < string > pModules = new List < string >();

                        while ( LA( 1 ) == BiteLexer.DotOperator )
                        {
                            pModules.Add( nextIdentifier );

                            if ( !match( BiteLexer.DotOperator, out matchContext ) )
                                return matchContext;

                            nextIdentifier = LT( 1 ).text;

                            if ( !match( BiteLexer.Identifier, out matchContext ) )
                                return matchContext;
                        }

                        if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                            return matchContext;

                        importedModules.Add( new ModuleIdentifier( nextIdentifier, pModules ) );
                    }
                    else
                    {
                        if ( !match( BiteLexer.UsingDirective, out matchContext ) )
                            return matchContext;

                        string nextIdentifier = LT( 1 ).text;

                        if ( !match( BiteLexer.Identifier, out matchContext ) )
                            return matchContext;

                        List < string > pModules = new List < string >();

                        while ( LA( 1 ) == BiteLexer.DotOperator )
                        {
                            pModules.Add( nextIdentifier );

                            if ( !match( BiteLexer.DotOperator, out matchContext ) )
                                return matchContext;

                            nextIdentifier = LT( 1 ).text;

                            if ( !match( BiteLexer.Identifier, out matchContext ) )
                                return matchContext;
                        }

                        if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                            return matchContext;

                        usedModules.Add( new ModuleIdentifier( nextIdentifier, pModules ) );
                    }
                }
            }

            moduleNode.ImportedModules = importedModules;
            moduleNode.UsedModules = usedModules;

            var _statements = new List < StatementNode >();

            while ( LA( 1 ) != Lexer.EOF_TYPE )
            {
                var context = declaration();

                if ( context.Failed )
                    return Context < ModuleNode >.AsFailed( context.Exception );

                _statements.Add( ( StatementNode ) context.Result );

                //HeteroAstNode decl = context.Result;

                //switch (decl)
                //{
                //    case ClassDeclarationNode classDeclarationNode:
                //        moduleNode.Statements.Add(classDeclarationNode);
                //        break;
                //    case FunctionDeclarationNode functionDeclarationNode:
                //        moduleNode.Statements.Add(functionDeclarationNode);
                //        break;
                //    case BlockStatementNode block:
                //        moduleNode.Statements.Add(block);
                //        break;
                //    case StructDeclarationNode structDeclaration:
                //        moduleNode.Statements.Add(structDeclaration);
                //        break;
                //    case VariableDeclarationNode variable:
                //        moduleNode.Statements.Add(variable);
                //        break;
                //    case ClassInstanceDeclarationNode classInstance:
                //        moduleNode.Statements.Add(classInstance);
                //        break;
                //    case StatementNode statement:
                //        moduleNode.Statements.Add(statement);
                //        break;
                //}
            }

            moduleNode.Statements = _statements;

            return new Context < ModuleNode >( moduleNode );
        }

        if ( !match( BiteLexer.DeclareModule, out matchContext ) )
            return matchContext;

        if ( !match( BiteLexer.Identifier, out matchContext ) )
            return matchContext;

        while ( LA( 1 ) == BiteLexer.DotOperator )
        {
            if ( !match( BiteLexer.DotOperator, out matchContext ) )
                return matchContext;

            if ( !match( BiteLexer.Identifier, out matchContext ) )
                return matchContext;
        }

        if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
            return matchContext;

        if ( LA( 1 ) == BiteLexer.ImportDirective || LA( 1 ) == BiteLexer.UsingDirective )
        {
            while ( LA( 1 ) == BiteLexer.ImportDirective || LA( 1 ) == BiteLexer.UsingDirective )
            {
                if ( LA( 1 ) == BiteLexer.ImportDirective )
                {
                    if ( !match( BiteLexer.ImportDirective, out matchContext ) )
                        return matchContext;

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;

                    while ( LA( 1 ) == BiteLexer.DotOperator )
                    {
                        if ( !match( BiteLexer.DotOperator, out matchContext ) )
                            return matchContext;

                        if ( !match( BiteLexer.Identifier, out matchContext ) )
                            return matchContext;
                    }

                    if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                        return matchContext;
                }
                else
                {
                    if ( !match( BiteLexer.UsingDirective, out matchContext ) )
                        return matchContext;

                    if ( !match( BiteLexer.Identifier, out matchContext ) )
                        return matchContext;

                    while ( LA( 1 ) == BiteLexer.DotOperator )
                    {
                        if ( !match( BiteLexer.DotOperator, out matchContext ) )
                            return matchContext;

                        if ( !match( BiteLexer.Identifier, out matchContext ) )
                            return matchContext;
                    }

                    if ( !match( BiteLexer.SemicolonSeperator, out matchContext ) )
                        return matchContext;
                }
            }
        }

        while ( LA( 1 ) != Lexer.EOF_TYPE )
        {
            var context = declaration();

            if ( context.Failed )
                return Context < ModuleNode >.AsFailed( context.Exception );
        }

        return new Context < ModuleNode >( null );
    }

    #endregion
}

}
