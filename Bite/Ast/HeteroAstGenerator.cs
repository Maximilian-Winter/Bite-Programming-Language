using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using AntlrBiteParser;
using Bite.Ast;

namespace MemoizeSharp
{

public class HeteroAstGenerator : BITEBaseVisitor < HeteroAstNode >
{
    private ProgramNode ProgramNode = new ProgramNode( "MainModule" );

    private bool m_IsLookingForAssingPart = false;

    #region Public

    public ProgramNode CreateAst( BITEParser.ProgramContext programContext, string mainModule )
    {
        ProgramNode = new ProgramNode( mainModule );

        return ( ProgramNode ) VisitProgram( programContext );
    }

    public override HeteroAstNode VisitAbstractModifier( BITEParser.AbstractModifierContext context )
    {
        return base.VisitAbstractModifier( context );
    }

    public override HeteroAstNode VisitAdditive( BITEParser.AdditiveContext context )
    {
        if ( context.multiplicative().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.MultiplicativeContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitMultiplicative( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "-" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Minus;
                    }

                    if ( terminalNode.GetText() == "+" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Plus;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.MultiplicativeContext rhsContext )
                    {
                        currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitMultiplicative( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitMultiplicative( context.multiplicative( 0 ) );
    }

    public override HeteroAstNode VisitArgumentExpression( BITEParser.ArgumentExpressionContext context )
    {
        return base.VisitArgumentExpression( context );
    }

    public override HeteroAstNode VisitArguments( BITEParser.ArgumentsContext context )
    {
        ArgumentsNode arguments = new ArgumentsNode();
        arguments.Expressions = new List < ExpressionNode >();
        arguments.IsReference = new List < bool >();
        arguments.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        arguments.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        arguments.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        arguments.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        foreach ( BITEParser.ArgumentExpressionContext expressionContext in context.argumentExpression() )
        {
            if ( expressionContext.ReferenceOperator() != null )
            {
                arguments.IsReference.Add( true );
            }
            else
            {
                arguments.IsReference.Add( false );
            }

            arguments.Expressions.Add( ( ExpressionNode ) VisitExpression( expressionContext.expression() ) );
        }

        return arguments;
    }

    public override HeteroAstNode VisitAssignment( BITEParser.AssignmentContext context )
    {
        if ( context.call() != null && context.assignment() != null )
        {
            AssignmentNode assignmentNode = new AssignmentNode();
            assignmentNode.Type = AssignmentTypes.Assignment;
            assignmentNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            assignmentNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            assignmentNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            assignmentNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
            m_IsLookingForAssingPart = true;
            assignmentNode.Call = ( CallNode ) VisitCall( context.call() );
            m_IsLookingForAssingPart = false;

            if ( context.AssignOperator() != null )
            {
                assignmentNode.OperatorType = AssignmentOperatorTypes.Assign;
            }

            if ( context.DivideAssignOperator() != null )
            {
                assignmentNode.OperatorType = AssignmentOperatorTypes.DivAssign;
            }

            if ( context.MultiplyAssignOperator() != null )
            {
                assignmentNode.OperatorType = AssignmentOperatorTypes.MultAssign;
            }

            if ( context.PlusAssignOperator() != null )
            {
                assignmentNode.OperatorType = AssignmentOperatorTypes.PlusAssign;
            }

            if ( context.MinusAssignOperator() != null )
            {
                assignmentNode.OperatorType = AssignmentOperatorTypes.MinusAssign;
            }

            assignmentNode.Assignment = ( AssignmentNode ) VisitAssignment( context.assignment() );

            return assignmentNode;
        }

        if ( context.ternary() != null )
        {
            AssignmentNode assignmentNode = new AssignmentNode();
            HeteroAstNode node = VisitTernary( context.ternary() );
            assignmentNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            assignmentNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            assignmentNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            assignmentNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

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

            if (node is TernaryOperationNode ternaryOperationNode)
            {
                assignmentNode.Type = AssignmentTypes.Ternary;
                assignmentNode.Ternary = ternaryOperationNode;
            }

            return assignmentNode;
        }

        return null;
    }

    public override HeteroAstNode VisitBitwiseAnd( BITEParser.BitwiseAndContext context )
    {
        if ( context.equality().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.EqualityContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitEquality( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "&" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseAnd;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.EqualityContext rhsContext )
                    {
                        currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitEquality( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( "&" ) )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitEquality( context.equality( 0 ) );
    }

    public override HeteroAstNode VisitBitwiseOr( BITEParser.BitwiseOrContext context )
    {
        if ( context.bitwiseXor().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.BitwiseXorContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitBitwiseXor( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "|" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseOr;
                    }
                }
                else if ( context.GetChild( counter ) is BITEParser.BitwiseXorContext rhsContext )
                {
                    currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                    currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                    currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                    currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                    currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitBitwiseXor( rhsContext );
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( " |" ) )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitBitwiseXor( context.bitwiseXor( 0 ) );
    }

    public override HeteroAstNode VisitBitwiseXor( BITEParser.BitwiseXorContext context )
    {
        if ( context.bitwiseAnd().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.BitwiseAndContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitBitwiseAnd( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "^" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.BitwiseXor;
                    }
                }
                else if ( context.GetChild( counter ) is BITEParser.BitwiseAndContext rhsContext )
                {
                    currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                    currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                    currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                    currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                    currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitBitwiseAnd( rhsContext );
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( "^" ) )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitBitwiseAnd( context.bitwiseAnd( 0 ) );
    }

    public override HeteroAstNode VisitBlock( BITEParser.BlockContext context )
    {
        BlockStatementNode blockStatementNode = new BlockStatementNode();
        blockStatementNode.Declarations = new DeclarationsNode();
        blockStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        blockStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        blockStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        blockStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        foreach ( BITEParser.DeclarationContext declarationContext in context.declaration() )
        {
            HeteroAstNode decl = VisitDeclaration( declarationContext );

            if ( decl is ClassDeclarationNode classDeclarationNode )
            {
                blockStatementNode.Declarations.Classes.Add( classDeclarationNode );
            }
            else if ( decl is FunctionDeclarationNode functionDeclarationNode )
            {
                blockStatementNode.Declarations.Functions.Add( functionDeclarationNode );
            }
            else if ( decl is StructDeclarationNode structDeclaration )
            {
                blockStatementNode.Declarations.Structs.Add( structDeclaration );
            }
            else if ( decl is VariableDeclarationNode variable )
            {
                blockStatementNode.Declarations.Variables.Add( variable );
            }
            else if ( decl is ClassInstanceDeclarationNode classInstance )
            {
                blockStatementNode.Declarations.ClassInstances.Add( classInstance );
            }
            else if ( decl is StatementNode statement )
            {
                blockStatementNode.Declarations.Statements.Add( statement );
            }
        }

        return blockStatementNode;
    }

    public override HeteroAstNode VisitBreakStatement( BITEParser.BreakStatementContext context )
    {
        if ( context.Break() != null )
        {
            BreakStatementNode breakStatementNode = new BreakStatementNode();
            breakStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            breakStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            breakStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            breakStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            return breakStatementNode;
        }

        return base.VisitBreakStatement( context );
    }

    public override HeteroAstNode VisitCall( BITEParser.CallContext context )
    {
        if ( context.ChildCount > 0 )
        {
            int childCounter = 0;
            CallNode callNode = new CallNode();
            callNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            callNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            callNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            callNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
            IParseTree tree = context.GetChild( childCounter );

            if ( tree is BITEParser.PrimaryContext )
            {
                CallEntry currentIdentifier = null;
                callNode.Primary = ( PrimaryNode ) VisitPrimary( context.primary() );
                childCounter++;
                tree = context.GetChild( childCounter );

                while ( childCounter < context.ChildCount )
                {
                    if ( tree is ITerminalNode dotOperator && dotOperator.Symbol.Text == "." )
                    {
                        childCounter++;
                    }
                    else if ( tree is ITerminalNode indentifier && indentifier.Symbol.Text != "." )
                    {
                        if ( callNode.CallEntries == null )
                        {
                            callNode.CallEntries = new List < CallEntry >();
                        }

                        PrimaryNode primaryNode = new PrimaryNode();
                        primaryNode.PrimaryId = new Identifier();
                        primaryNode.PrimaryId.Id = indentifier.Symbol.Text;
                        primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Identifier;
                        CallEntry callEntry = new CallEntry();
                        callEntry.Primary = primaryNode;
                        callNode.CallEntries.Add( callEntry );
                        currentIdentifier = callEntry;
                        childCounter++;
                    }
                    else if ( tree is BITEParser.CallArgumentsContext callArgumentsContext )
                    {
                        if ( currentIdentifier != null )
                        {
                            currentIdentifier.IsFunctionCall = true;
                        }
                        else
                        {
                            callNode.IsFunctionCall = true;
                        }

                        if ( callArgumentsContext.arguments() != null )
                        {
                            if ( currentIdentifier != null &&
                                 callArgumentsContext.
                                     arguments() !=
                                 null )
                            {
                                currentIdentifier.Arguments = new ArgumentsNode();
                                currentIdentifier.Arguments.Expressions = new List < ExpressionNode >();
                                currentIdentifier.Arguments.IsReference = new List < bool >();

                                foreach ( BITEParser.ArgumentExpressionContext expressionContext in
                                         callArgumentsContext.
                                             arguments().
                                             argumentExpression() )
                                {
                                    if ( expressionContext.ReferenceOperator() != null )
                                    {
                                        currentIdentifier.Arguments.IsReference.Add( true );
                                    }
                                    else
                                    {
                                        currentIdentifier.Arguments.IsReference.Add( false );
                                    }

                                    currentIdentifier.Arguments.
                                                      Expressions.Add(
                                                          ( ExpressionNode ) VisitExpression(
                                                              expressionContext.expression() ) );
                                }

                                childCounter++;
                            }
                            else
                            {
                                callNode.Arguments = new ArgumentsNode();
                                callNode.Arguments.Expressions = new List < ExpressionNode >();
                                callNode.Arguments.IsReference = new List < bool >();

                                foreach ( BITEParser.ArgumentExpressionContext expressionContext in
                                         callArgumentsContext.
                                             arguments().
                                             argumentExpression() )
                                {
                                    if ( expressionContext.ReferenceOperator() != null )
                                    {
                                        callNode.Arguments.IsReference.Add( true );
                                    }
                                    else
                                    {
                                        callNode.Arguments.IsReference.Add( false );
                                    }

                                    callNode.Arguments.Expressions.Add(
                                        ( ExpressionNode ) VisitExpression( expressionContext.expression() ) );
                                }
                            }
                        }

                        childCounter++;
                    }
                    else if ( tree is BITEParser.ElementAccessContext elementAccess )
                    {
                        if ( currentIdentifier != null )
                        {
                            currentIdentifier.ElementAccess = new List < CallElementEntry >();

                            foreach ( BITEParser.ElementIdentifierContext elementIdentifierContext in elementAccess.
                                         elementIdentifier() )
                            {
                                CallElementEntry callElementEntry = new CallElementEntry();

                                if ( elementIdentifierContext.call() != null )
                                {
                                    callElementEntry.Call = ( CallNode ) VisitCall( elementIdentifierContext.call() );

                                    callElementEntry.CallElementType =
                                        CallElementTypes.Call;
                                }

                                if ( elementIdentifierContext.StringLiteral() != null )
                                {
                                    string literal = elementIdentifierContext.StringLiteral().Symbol.Text;

                                    callElementEntry.Identifier = literal.Substring( 1, literal.Length - 2 );

                                    callElementEntry.CallElementType =
                                        CallElementTypes.StringLiteral;
                                }

                                if ( elementIdentifierContext.IntegerLiteral() != null )
                                {
                                    callElementEntry.Identifier = elementIdentifierContext.IntegerLiteral().Symbol.Text;

                                    callElementEntry.CallElementType =
                                        CallElementTypes.IntegerLiteral;
                                }

                                currentIdentifier.ElementAccess.Add( callElementEntry );
                            }
                        }
                        else
                        {
                            callNode.ElementAccess = new List < CallElementEntry >();

                            foreach ( BITEParser.ElementIdentifierContext elementIdentifierContext in elementAccess.
                                         elementIdentifier() )
                            {
                                CallElementEntry callElementEntry = new CallElementEntry();

                                if ( elementIdentifierContext.call() != null )
                                {
                                    callElementEntry.Call = ( CallNode ) VisitCall( elementIdentifierContext.call() );

                                    callElementEntry.CallElementType =
                                        CallElementTypes.Call;
                                }

                                if ( elementIdentifierContext.StringLiteral() != null )
                                {
                                    string literal = elementIdentifierContext.StringLiteral().Symbol.Text;

                                    callElementEntry.Identifier = literal.Substring( 1, literal.Length - 2 );

                                    callElementEntry.CallElementType =
                                        CallElementTypes.StringLiteral;
                                }

                                if ( elementIdentifierContext.IntegerLiteral() != null )
                                {
                                    callElementEntry.Identifier = elementIdentifierContext.IntegerLiteral().Symbol.Text;

                                    callElementEntry.CallElementType =
                                        CallElementTypes.IntegerLiteral;
                                }

                                callNode.ElementAccess.Add( callElementEntry );
                            }
                        }

                        childCounter++;
                    }
                    else
                    {
                        childCounter++;
                    }

                    tree = context.GetChild( childCounter );
                }
            }

            if ( callNode.CallEntries != null && callNode.Arguments != null )
            {
                callNode.CallType = CallTypes.PrimaryCall;
            }
            else if ( callNode.CallEntries != null )
            {
                callNode.CallType = CallTypes.PrimaryCall;
            }
            else if ( callNode.Arguments != null )
            {
                callNode.CallType = CallTypes.PrimaryCall;
            }
            else if ( callNode.IsFunctionCall )
            {
                callNode.CallType = CallTypes.PrimaryCall;
            }
            else if ( callNode.ElementAccess != null )
            {
                callNode.CallType = CallTypes.PrimaryCall;
            }
            else
            {
                callNode.CallType = CallTypes.Primary;
            }

            return callNode;
        }

        return null;
    }

    public override HeteroAstNode VisitCallArguments( BITEParser.CallArgumentsContext context )
    {
        return base.VisitCallArguments( context );
    }

    public override HeteroAstNode VisitClassDeclaration( BITEParser.ClassDeclarationContext context )
    {
        ClassDeclarationNode classDeclarationNode = new ClassDeclarationNode();
        classDeclarationNode.ClassId = new Identifier();
        classDeclarationNode.ClassId.Id = context.Identifier().Symbol.Text;
        classDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        classDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        classDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        classDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.block() != null )
        {
            classDeclarationNode.BlockStatement = ( BlockStatementNode ) VisitBlock( context.block() );
        }

        string accessToken = null;
        string abstractStaticMod = null;

        if ( context.publicModifier() != null )
        {
            accessToken = "public";
        }
        else
        {
            accessToken = "private";
        }

        if ( context.abstractModifier() != null )
        {
            abstractStaticMod = "abstract";
        }
        else if ( context.staticModifier() != null )
        {
            abstractStaticMod = "static";
        }

        ModifiersNode Modifiers = new ModifiersNode( accessToken, abstractStaticMod );
        classDeclarationNode.Modifiers = Modifiers;

        if ( context.inheritance() != null )
        {
            classDeclarationNode.Inheritance = new List < Identifier >();

            foreach ( ITerminalNode terminalNode in context.inheritance().Identifier() )
            {
                Identifier id = new Identifier();
                id.Id = terminalNode.Symbol.Text;
                classDeclarationNode.Inheritance.Add( id );
            }
        }

        return classDeclarationNode;
    }

    public override HeteroAstNode VisitClassInstanceDeclaration( BITEParser.ClassInstanceDeclarationContext context )
    {
        ClassInstanceDeclarationNode classInstanceDeclarationNode = new ClassInstanceDeclarationNode();
        classInstanceDeclarationNode.InstanceId = new Identifier();
        classInstanceDeclarationNode.InstanceId.Id = context.Identifier( 0 ).Symbol.Text;
        classInstanceDeclarationNode.ClassName = new Identifier();
        classInstanceDeclarationNode.ClassName.Id = context.Identifier( 1 ).Symbol.Text;
        classInstanceDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        classInstanceDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        classInstanceDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        classInstanceDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.DeclareVariable() != null )
        {
            classInstanceDeclarationNode.IsVariableRedeclaration = false;
        }
        else
        {
            classInstanceDeclarationNode.IsVariableRedeclaration = true;
        }

        if ( context.arguments() != null )
        {
            classInstanceDeclarationNode.Arguments = ( ArgumentsNode ) VisitArguments( context.arguments() );
        }

        string accessToken = null;
        string abstractStaticMod = null;

        if ( context.publicModifier() != null )
        {
            accessToken = "public";
        }
        else
        {
            accessToken = "private";
        }


        ModifiersNode Modifiers = new ModifiersNode( accessToken, abstractStaticMod );
        classInstanceDeclarationNode.Modifiers = Modifiers;

        return classInstanceDeclarationNode;
    }

    public override HeteroAstNode VisitDeclaration( BITEParser.DeclarationContext context )
    {
        if ( context.classDeclaration() != null )
        {
            return VisitClassDeclaration( context.classDeclaration() );
        }

        if ( context.structDeclaration() != null )
        {
            return VisitStructDeclaration( context.structDeclaration() );
        }

        if ( context.functionDeclaration() != null )
        {
            return VisitFunctionDeclaration( context.functionDeclaration() );
        }

        if ( context.classInstanceDeclaration() != null )
        {
            return VisitClassInstanceDeclaration( context.classInstanceDeclaration() );
        }

        if ( context.variableDeclaration() != null )
        {
            return VisitVariableDeclaration( context.variableDeclaration() );
        }

        if ( context.statement() != null )
        {
            StatementNode statement = VisitStatement( context.statement() ) as StatementNode;
            statement.DebugInfoAstNode.LineNumberStart = context.statement().Start.Line;
            statement.DebugInfoAstNode.LineNumberEnd = context.statement().Stop.Line;

            statement.DebugInfoAstNode.ColumnNumberStart = context.statement().Start.Column;
            statement.DebugInfoAstNode.ColumnNumberEnd = context.statement().Stop.Column;

            return statement;
        }

        return null;
    }

    public override HeteroAstNode VisitElementAccess( BITEParser.ElementAccessContext context )
    {
        return base.VisitElementAccess( context );
    }

    public override HeteroAstNode VisitElementIdentifier( BITEParser.ElementIdentifierContext context )
    {
        return base.VisitElementIdentifier( context );
    }

    public override HeteroAstNode VisitEquality( BITEParser.EqualityContext context )
    {
        if ( context.relational().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.RelationalContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitRelational( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "==" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Equal;
                    }

                    if ( terminalNode.GetText() == "!=" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.NotEqual;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.RelationalContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitRelational( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitRelational( context.relational( 0 ) );
    }

    public override HeteroAstNode VisitExpression( BITEParser.ExpressionContext context )
    {
        ExpressionNode expressionNode = new ExpressionNode();
        expressionNode.Assignment = ( AssignmentNode ) VisitAssignment( context.assignment() );
        expressionNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        expressionNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        expressionNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        expressionNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        return expressionNode;
    }

    public override HeteroAstNode VisitExprStatement( BITEParser.ExprStatementContext context )
    {
        ExpressionStatementNode expressionStatementNode = new ExpressionStatementNode();
        expressionStatementNode.Expression = ( ExpressionNode ) VisitExpression( context.expression() );
        expressionStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        expressionStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        expressionStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        expressionStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        return expressionStatementNode;
    }

    public override HeteroAstNode VisitLocalVarDeclaration( BITEParser.LocalVarDeclarationContext context )
    {
        LocalVariableDeclarationNode variableDeclarationNode = new LocalVariableDeclarationNode();
        variableDeclarationNode.VarId = new Identifier( context.Identifier().Symbol.Text );
        variableDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        variableDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        variableDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        variableDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.expression() != null )
        {
            variableDeclarationNode.Expression = ( ExpressionNode ) VisitExpression( context.expression() );
        }

        string accessToken = "private";

        ModifiersNode Modifiers = new ModifiersNode( accessToken, null );
        variableDeclarationNode.Modifiers = Modifiers;

        return variableDeclarationNode;
    }

    public override HeteroAstNode VisitLocalVarInitializer( BITEParser.LocalVarInitializerContext context )
    {
        LocalVariableInitializerNode localVariableInitializerNode = new LocalVariableInitializerNode();

        var variableDeclarationNodes = new List < LocalVariableDeclarationNode >();

        var localVarDeclarations = context.localVarDeclaration();

        if ( localVarDeclarations != null )
        {
            foreach ( var localVarDeclarationContext in context.localVarDeclaration() )
            {
                variableDeclarationNodes.Add(
                    ( LocalVariableDeclarationNode ) VisitLocalVarDeclaration( localVarDeclarationContext ) );
            }
        }

        localVariableInitializerNode.VariableDeclarations = variableDeclarationNodes;

        return localVariableInitializerNode;
    }

    public override HeteroAstNode VisitForInitializer( BITEParser.ForInitializerContext context )
    {
        ForInitializerNode forInitializerNode = new ForInitializerNode();

        var expressions = context.expression();

        if ( expressions != null && expressions.Length > 0 )
        {
            forInitializerNode.Expressions = new ExpressionNode[expressions.Length];
            int i = 0;

            foreach ( var expression in expressions )
            {
                forInitializerNode.Expressions[i++] = ( ExpressionNode ) VisitExpression( expression );
            }
        }

        var localVarInitializer = context.localVarInitializer();

        if ( localVarInitializer != null )
        {
            forInitializerNode.LocalVariableInitializer =
                ( LocalVariableInitializerNode ) VisitLocalVarInitializer( localVarInitializer );
        }

        return forInitializerNode;
    }

    public override HeteroAstNode VisitForStatement( BITEParser.ForStatementContext context )
    {
        ForStatementNode forStatementNode = new ForStatementNode();
        forStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        forStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        forStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        forStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        var initializer = context.forInitializer();

        if ( initializer != null )
        {
            forStatementNode.Initializer = ( ForInitializerNode ) VisitForInitializer( initializer );
        }

        if ( context.condition != null )
        {
            forStatementNode.Condition = ( ExpressionNode ) VisitExpression( context.condition );
        }

        var iterators = context.forIterator();

        if ( iterators != null )
        {
            var expressions = iterators.expression();

            forStatementNode.Iterators = new ExpressionNode[expressions.Length];
            var i = 0;

            foreach ( var iterator in expressions )
            {
                forStatementNode.Iterators[i++] = ( ExpressionNode ) VisitExpression( iterator );
            }
        }

        forStatementNode.Statement = ( StatementNode ) VisitStatement( context.statement() );

        return forStatementNode;
    }

    public override HeteroAstNode VisitFunctionDeclaration( BITEParser.FunctionDeclarationContext context )
    {
        FunctionDeclarationNode functionDeclarationNode = new FunctionDeclarationNode();
        functionDeclarationNode.FunctionId = new Identifier();
        functionDeclarationNode.FunctionId.Id = context.Identifier().Symbol.Text;
        functionDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        functionDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        functionDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        functionDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.block() != null )
        {
            functionDeclarationNode.FunctionBlock = ( BlockStatementNode ) VisitBlock( context.block() );
        }

        string accessToken;
        string abstractStaticMod = null;

        if ( context.publicModifier() != null )
        {
            accessToken = "public";
        }
        else
        {
            accessToken = "private";
        }

        if ( context.abstractModifier() != null )
        {
            abstractStaticMod = "abstract";
        }
        else if ( context.staticModifier() != null )
        {
            abstractStaticMod = "static";
        }

        ModifiersNode Modifiers = new ModifiersNode( accessToken, abstractStaticMod );
        functionDeclarationNode.Modifiers = Modifiers;

        if ( context.parameters() != null )
        {
            functionDeclarationNode.Parameters = ( ParametersNode ) VisitParameters( context.parameters() );
        }

        return functionDeclarationNode;
    }

    public override HeteroAstNode VisitIfStatement( BITEParser.IfStatementContext context )
    {
        IfStatementNode ifStatementNode = new IfStatementNode();
        ifStatementNode.Expression = ( ExpressionNode ) VisitExpression( context.expression() );
        ifStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        ifStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        ifStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        ifStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.trueStatement != null )
        {
            ifStatementNode.ThenStatement = ( StatementNode ) VisitStatement( context.trueStatement );
        }

        if ( context.falseStatement != null )
        {
            ifStatementNode.ElseStatement = ( StatementNode ) VisitStatement( context.falseStatement );
        }

        return ifStatementNode;
    }

    public override HeteroAstNode VisitImportDirective( BITEParser.ImportDirectiveContext context )
    {
        return base.VisitImportDirective( context );
    }

    public override HeteroAstNode VisitInheritance( BITEParser.InheritanceContext context )
    {
        return base.VisitInheritance( context );
    }

    public override HeteroAstNode VisitLogicAnd( BITEParser.LogicAndContext context )
    {
        if ( context.bitwiseOr().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.BitwiseOrContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitBitwiseOr( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "&&" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.And;
                    }
                }
                else if ( context.GetChild( counter ) is BITEParser.BitwiseOrContext rhsContext )
                {
                    currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                    currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                    currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                    currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                    currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitBitwiseOr( rhsContext );
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( " &&" ) )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitBitwiseOr( context.bitwiseOr( 0 ) );
    }

    public override HeteroAstNode VisitLogicOr( BITEParser.LogicOrContext context )
    {
        if ( context.logicAnd().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.LogicAndContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitLogicAnd( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "||" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Or;
                    }
                }
                else if ( context.GetChild( counter ) is BITEParser.LogicAndContext rhsContext )
                {
                    currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                    currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                    currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                    currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                    currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitLogicAnd( rhsContext );
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( " ||" ) )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitLogicAnd( context.logicAnd( 0 ) );
    }

    public override HeteroAstNode VisitModule( BITEParser.ModuleContext context )
    {
        ModuleNode moduleNode = new ModuleNode();
        string moduleName = "";
        List < string > parentModules = new List < string >();
        List < ModuleIdentifier > importedModules = new List < ModuleIdentifier >();
        List < ModuleIdentifier > usedModules = new List < ModuleIdentifier >();
        moduleName = context.moduleDeclaration().Identifier( 0 ).Symbol.Text;

        for ( int i = 1; i < context.moduleDeclaration().Identifier().Length; i++ )
        {
            parentModules.Add( moduleName );
            moduleName = context.moduleDeclaration().Identifier( i ).Symbol.Text;
        }

        moduleNode.ModuleIdent = new ModuleIdentifier( moduleName, parentModules );

        for ( int i = 0; i < context.importDirective().Length; i++ )
        {
            string nextIdentifier = context.importDirective( i ).Identifier( 0 ).Symbol.Text;
            List < string > pModules = new List < string >();

            for ( int t = 1; t < context.importDirective( i ).Identifier().Length; t++ )
            {
                pModules.Add( moduleName );
                moduleName = context.importDirective( i ).Identifier( t ).Symbol.Text;
            }

            importedModules.Add( new ModuleIdentifier( nextIdentifier, pModules ) );
        }

        for ( int i = 0; i < context.usingDirective().Length; i++ )
        {
            string nextIdentifier = context.usingDirective( i ).Identifier( 0 ).Symbol.Text;
            List < string > pModules = new List < string >();

            for ( int t = 1; t < context.usingDirective( i ).Identifier().Length; t++ )
            {
                pModules.Add( moduleName );
                moduleName = context.usingDirective( i ).Identifier( t ).Symbol.Text;
            }

            usedModules.Add( new ModuleIdentifier( nextIdentifier, pModules ) );
        }

        moduleNode.ImportedModules = importedModules;
        moduleNode.UsedModules = usedModules;

        List < StatementNode > _statements = new List < StatementNode >();

        foreach ( BITEParser.DeclarationContext declarationContext in context.declaration() )
        {
            _statements.Add( ( StatementNode ) VisitDeclaration( declarationContext ) );
        }

        moduleNode.Statements = _statements;

        return moduleNode;
    }

    public override HeteroAstNode VisitMultiplicative( BITEParser.MultiplicativeContext context )
    {
        if ( context.unary().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.UnaryContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitUnary( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "/" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Div;
                    }

                    if ( terminalNode.GetText() == "*" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Mult;
                    }

                    if ( terminalNode.GetText() == "%" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Modulo;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.UnaryContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitUnary( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitUnary( context.unary( 0 ) );
    }

    public override HeteroAstNode VisitParameters( BITEParser.ParametersContext context )
    {
        ParametersNode parametersNode = new ParametersNode();
        parametersNode.Identifiers = new List < Identifier >();
        parametersNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        parametersNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        parametersNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        parametersNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        foreach ( BITEParser.ParametersIdentifierContext parametersIdentifier in context.parametersIdentifier() )
        {
            parametersNode.Identifiers.Add( new Identifier( parametersIdentifier.Identifier().Symbol.Text ) );
        }

        return parametersNode;
    }

    public override HeteroAstNode VisitPrimary( BITEParser.PrimaryContext context )
    {
        PrimaryNode primaryNode = new PrimaryNode();
        primaryNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        primaryNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        primaryNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        primaryNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.expression() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Expression;
            primaryNode.Expression = ( ExpressionNode ) VisitExpression( context.expression() );
        }

        if ( context.BooleanLiteral() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.BooleanLiteral;
            primaryNode.BooleanLiteral = bool.Parse( context.BooleanLiteral().Symbol.Text );
        }

        if ( context.NullReference() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.NullReference;
            primaryNode.PrimaryId = new Identifier( context.NullReference().Symbol.Text );
        }

        if ( context.ThisReference() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.ThisReference;
            primaryNode.PrimaryId = new Identifier( context.ThisReference().Symbol.Text );
        }

        if ( context.IntegerLiteral() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.IntegerLiteral;
            primaryNode.IntegerLiteral = int.Parse( context.IntegerLiteral().Symbol.Text );
        }

        if ( context.FloatingLiteral() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.FloatLiteral;
            primaryNode.FloatLiteral = double.Parse( context.FloatingLiteral().Symbol.Text );
        }

        if ( context.StringLiteral() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.StringLiteral;

            primaryNode.StringLiteral = context.StringLiteral().
                                                Symbol.Text.Substring(
                                                    1,
                                                    context.StringLiteral().Symbol.Text.Length - 2 );
        }

        if ( context.Identifier() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Identifier;
            primaryNode.PrimaryId = new Identifier( context.Identifier().Symbol.Text );
        }

        return primaryNode;
    }

    public override HeteroAstNode VisitProgram( BITEParser.ProgramContext context )
    {
        ProgramNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        ProgramNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        ProgramNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        ProgramNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        foreach ( BITEParser.ModuleContext declarationContext in context.module() )
        {
            HeteroAstNode decl = VisitModule( declarationContext );

            ProgramNode.AddModule( decl as ModuleNode );
        }

        return ProgramNode;
    }

    public override HeteroAstNode VisitRelational( BITEParser.RelationalContext context )
    {
        if ( context.shift().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.ShiftContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitShift( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "<" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Less;
                    }

                    if ( terminalNode.GetText() == ">" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.Greater;
                    }

                    if ( terminalNode.GetText() == ">=" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.GreaterOrEqual;
                    }

                    if ( terminalNode.GetText() == "<=" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.LessOrEqual;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.ShiftContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitShift( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitShift( context.shift( 0 ) );
    }

    public override HeteroAstNode VisitReturnStatement( BITEParser.ReturnStatementContext context )
    {
        ReturnStatementNode returnStatementNode = new ReturnStatementNode();
        returnStatementNode.ExpressionStatement = new ExpressionStatementNode();
        returnStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        returnStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        returnStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        returnStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        returnStatementNode.ExpressionStatement.Expression = ( ExpressionNode ) VisitExpression( context.expression() );

        return returnStatementNode;
    }

    public override HeteroAstNode VisitShift( BITEParser.ShiftContext context )
    {
        if ( context.additive().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.AdditiveContext lhsContext )
            {
                firstBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitAdditive( lhsContext );
            }

            counter++;

            BinaryOperationNode currentBinaryOperationNode = firstBinaryOperationNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "<<" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.ShiftLeft;
                    }

                    if ( terminalNode.GetText() == ">>" )
                    {
                        currentBinaryOperationNode.Operator = BinaryOperationNode.BinaryOperatorType.ShiftRight;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.AdditiveContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitAdditive( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationNode binaryOperationNode = new BinaryOperationNode();
                    binaryOperationNode.LeftOperand = currentBinaryOperationNode;
                    currentBinaryOperationNode = binaryOperationNode;
                }
                else
                {
                    firstBinaryOperationNode = currentBinaryOperationNode;
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }

        return VisitAdditive( context.additive( 0 ) );
    }

    /// <summary>
    /// This is primarily for unit testing
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override HeteroAstNode VisitStatements( BITEParser.StatementsContext context )
    {
        DeclarationsNode declarations = new DeclarationsNode()
        {
            Statements = new List < StatementNode >()
        };

        foreach ( var declaration in context.declaration() )
        {
            declarations.Statements.Add( ( StatementNode ) VisitDeclaration( declaration ) );
        }

        return declarations;
    }

    public override HeteroAstNode VisitStatement( BITEParser.StatementContext context )
    {
        if ( context.exprStatement() != null )
        {
            return VisitExprStatement( context.exprStatement() );
        }

        if ( context.forStatement() != null )
        {
            return VisitForStatement( context.forStatement() );
        }

        if ( context.ifStatement() != null )
        {
            return VisitIfStatement( context.ifStatement() );
        }

        if ( context.usingStatement() != null )
        {
            return VisitUsingStatement( context.usingStatement() );
        }

        if ( context.breakStatement() != null )
        {
            return VisitBreakStatement( context.breakStatement() );
        }

        if ( context.whileStatement() != null )
        {
            return VisitWhileStatement( context.whileStatement() );
        }

        if ( context.returnStatement() != null )
        {
            return VisitReturnStatement( context.returnStatement() );
        }

        if ( context.block() != null )
        {
            return VisitBlock( context.block() );
        }

        return null;
    }

    public override HeteroAstNode VisitStructDeclaration( BITEParser.StructDeclarationContext context )
    {
        StructDeclarationNode structDeclarationNode = new StructDeclarationNode();
        structDeclarationNode.StructId = new Identifier( context.Identifier().Symbol.Text );
        structDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        structDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        structDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        structDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        structDeclarationNode.Block = ( BlockStatementNode ) VisitBlock( context.block() );

        string accessToken;
        string abstractStaticMod = null;

        if ( context.publicModifier() != null )
        {
            accessToken = "public";
        }
        else
        {
            accessToken = "private";
        }

        ModifiersNode Modifiers = new ModifiersNode( accessToken, abstractStaticMod );
        structDeclarationNode.Modifiers = Modifiers;

        return structDeclarationNode;
    }

    public override HeteroAstNode VisitTernary( BITEParser.TernaryContext context )
    {
        if ( context.logicOr().Length > 1 )
        {
            int counter = 0;
            TernaryOperationNode ternary = new TernaryOperationNode();
            ternary.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            ternary.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            ternary.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            ternary.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            ternary.LeftOperand = ( ExpressionNode ) VisitLogicOr( context.logicOr( 0 ) );
            ternary.MidOperand = ( ExpressionNode ) VisitLogicOr( context.logicOr( 1 ) );
            ternary.RightOperand = ( ExpressionNode ) VisitLogicOr( context.logicOr( 2 ) );

            return ternary;
        }

        return VisitLogicOr( context.logicOr( 0 ) );
    }

    public override HeteroAstNode VisitUnary( BITEParser.UnaryContext context )
    {
        if ( context.unary() != null )
        {
            int counter = 0;

            if ( context.GetChild( counter ) is BITEParser.UnaryContext lhsContext )
            {
                UnaryPostfixOperation unaryOperationNode = new UnaryPostfixOperation();
                unaryOperationNode.Primary = ( ExpressionNode ) VisitUnary( lhsContext );

                unaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
                unaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

                unaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
                unaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

                if ( context.PlusPlusOperator() != null )
                {
                    unaryOperationNode.Operator = UnaryPostfixOperation.UnaryPostfixOperatorType.PlusPlus;
                }

                if ( context.MinusMinusOperator() != null )
                {
                    unaryOperationNode.Operator = UnaryPostfixOperation.UnaryPostfixOperatorType.MinusMinus;
                }

                return unaryOperationNode;
            }
            else
            {
                UnaryPrefixOperation unaryOperationNode = new UnaryPrefixOperation();
                unaryOperationNode.Primary = ( ExpressionNode ) VisitUnary( context.unary() );

                unaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
                unaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

                unaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
                unaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

                if ( context.MinusOperator() != null )
                {
                    unaryOperationNode.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.Negate;
                }

                if ( context.LogicalNegationOperator() != null )
                {
                    unaryOperationNode.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.LogicalNot;
                }

                if ( context.PlusPlusOperator() != null )
                {
                    unaryOperationNode.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.PlusPlus;
                }

                if ( context.MinusMinusOperator() != null )
                {
                    unaryOperationNode.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.MinusMinus;
                }

                if ( context.ComplimentOperator() != null )
                {
                    unaryOperationNode.Operator = UnaryPrefixOperation.UnaryPrefixOperatorType.Compliment;
                }
                
                return unaryOperationNode;
            }
        }

        return VisitCall( context.call() );
    }

    public override HeteroAstNode VisitUsingStatement( BITEParser.UsingStatementContext context )
    {
        UsingStatementNode usingStatementNode = new UsingStatementNode();
        usingStatementNode.UsingNode = VisitExpression( context.expression() );
        usingStatementNode.UsingBlock = ( BlockStatementNode ) VisitBlock( context.block() );

        return usingStatementNode;
    }

    public override HeteroAstNode VisitVariableDeclaration( BITEParser.VariableDeclarationContext context )
    {
        VariableDeclarationNode variableDeclarationNode = new VariableDeclarationNode();
        variableDeclarationNode.VarId = new Identifier( context.Identifier().Symbol.Text );
        variableDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        variableDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        variableDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        variableDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.exprStatement() != null )
        {
            variableDeclarationNode.Initializer = new InitializerNode();

            variableDeclarationNode.Initializer.Expression =
                ( ExpressionStatementNode ) VisitExprStatement( context.exprStatement() );
        }

        string accessToken;
        string abstractStaticMod = null;

        if ( context.publicModifier() != null )
        {
            accessToken = "public";
        }
        else
        {
            accessToken = "private";
        }

        ModifiersNode Modifiers = new ModifiersNode( accessToken, abstractStaticMod );
        variableDeclarationNode.Modifiers = Modifiers;

        return variableDeclarationNode;
    }

    public override HeteroAstNode VisitWhileStatement( BITEParser.WhileStatementContext context )
    {
        WhileStatementNode whileStatementNode = new WhileStatementNode();
        whileStatementNode.Expression = ( ExpressionNode ) VisitExpression( context.expression() );
        whileStatementNode.WhileBlock = ( BlockStatementNode ) VisitBlock( context.block() );

        whileStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        whileStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        whileStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        whileStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        return whileStatementNode;
    }

    #endregion
}

}
