using System;
using System.Collections.Generic;
using System.Globalization;
using Antlr4.Runtime.Tree;
using AntlrBiteParser;
using Bite.Ast;

namespace MemoizeSharp
{

public class BiteAstGeneratorException : ApplicationException
{
    public BiteAstGeneratorException( string message ) : base( message )
    {
        AstGeneratorExceptionMessage = message;
    }
    public string AstGeneratorExceptionMessage { get; }
}

public class BiteAstGenerator : BITEParserBaseVisitor < AstBaseNode >
{
    private ProgramBaseNode m_ProgramBaseNode = new ProgramBaseNode();

    private bool m_IsLookingForAssingPart = false;

    #region Public

    public ProgramBaseNode CreateAst( BITEParser.ProgramContext programContext )
    {
        m_ProgramBaseNode = new ProgramBaseNode();

        return ( ProgramBaseNode ) VisitProgram( programContext );
    }

    public override AstBaseNode VisitAbstractModifier( BITEParser.AbstractModifierContext context )
    {
        return base.VisitAbstractModifier( context );
    }

    public override AstBaseNode VisitAdditive( BITEParser.AdditiveContext context )
    {
        if ( context.multiplicative().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.MultiplicativeContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitMultiplicative( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "-" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Minus;
                    }

                    if ( terminalNode.GetText() == "+" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Plus;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.MultiplicativeContext rhsContext )
                    {
                        currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitMultiplicative( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitMultiplicative( context.multiplicative( 0 ) );
    }

    public override AstBaseNode VisitArgumentExpression( BITEParser.ArgumentExpressionContext context )
    {
        return base.VisitArgumentExpression( context );
    }

    public override AstBaseNode VisitArguments( BITEParser.ArgumentsContext context )
    {
        ArgumentsBaseNode argumentsBase = new ArgumentsBaseNode();
        argumentsBase.Expressions = new List < ExpressionBaseNode >();
        argumentsBase.IsReference = new List < bool >();
        argumentsBase.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        argumentsBase.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        argumentsBase.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        argumentsBase.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        foreach ( BITEParser.ArgumentExpressionContext expressionContext in context.argumentExpression() )
        {
            if ( expressionContext.ReferenceOperator() != null )
            {
                argumentsBase.IsReference.Add( true );
            }
            else
            {
                argumentsBase.IsReference.Add( false );
            }

            argumentsBase.Expressions.Add( ( ExpressionBaseNode ) VisitExpression( expressionContext.expression() ) );
        }

        return argumentsBase;
    }

    public override AstBaseNode VisitAssignment( BITEParser.AssignmentContext context )
    {
        if ( context.call() != null && context.assignment() != null )
        {
            AssignmentBaseNode assignmentBaseNode = new AssignmentBaseNode();
            assignmentBaseNode.Type = AssignmentTypes.Assignment;
            assignmentBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            assignmentBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            assignmentBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            assignmentBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
            m_IsLookingForAssingPart = true;
            assignmentBaseNode.CallBase = ( CallBaseNode ) VisitCall( context.call() );
            m_IsLookingForAssingPart = false;

            if ( context.AssignOperator() != null )
            {
                assignmentBaseNode.OperatorType = AssignmentOperatorTypes.Assign;
            }

            if ( context.DivideAssignOperator() != null )
            {
                assignmentBaseNode.OperatorType = AssignmentOperatorTypes.DivAssign;
            }

            if ( context.MultiplyAssignOperator() != null )
            {
                assignmentBaseNode.OperatorType = AssignmentOperatorTypes.MultAssign;
            }

            if ( context.PlusAssignOperator() != null )
            {
                assignmentBaseNode.OperatorType = AssignmentOperatorTypes.PlusAssign;
            }

            if ( context.MinusAssignOperator() != null )
            {
                assignmentBaseNode.OperatorType = AssignmentOperatorTypes.MinusAssign;
            }

            assignmentBaseNode.AssignmentBase = ( AssignmentBaseNode ) VisitAssignment( context.assignment() );

            return assignmentBaseNode;
        }

        if ( context.ternary() != null )
        {
            AssignmentBaseNode assignmentBaseNode = new AssignmentBaseNode();
            AstBaseNode baseNode = VisitTernary( context.ternary() );
            assignmentBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            assignmentBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            assignmentBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            assignmentBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( baseNode is AssignmentBaseNode assignment )
            {
                assignmentBaseNode.Type = AssignmentTypes.Assignment;
                assignmentBaseNode.CallBase = assignment.CallBase;
                assignmentBaseNode.OperatorType = assignment.OperatorType;
                assignmentBaseNode.AssignmentBase = assignment;
            }

            if ( baseNode is BinaryOperationBaseNode binaryOperationNode )
            {
                assignmentBaseNode.Type = AssignmentTypes.Binary;
                assignmentBaseNode.Binary = binaryOperationNode;
            }

            if ( baseNode is UnaryPrefixOperation unaryPrefixOperation )
            {
                assignmentBaseNode.Type = AssignmentTypes.UnaryPrefix;
                assignmentBaseNode.UnaryPrefix = unaryPrefixOperation;
            }

            if ( baseNode is UnaryPostfixOperation unaryPostfixOperation )
            {
                assignmentBaseNode.Type = AssignmentTypes.UnaryPostfix;
                assignmentBaseNode.UnaryPostfix = unaryPostfixOperation;
            }

            if ( baseNode is CallBaseNode callNode )
            {
                assignmentBaseNode.Type = AssignmentTypes.Call;
                assignmentBaseNode.CallBase = callNode;
            }

            if ( baseNode is PrimaryBaseNode primaryNode )
            {
                assignmentBaseNode.Type = AssignmentTypes.Primary;
                assignmentBaseNode.PrimaryBaseNode = primaryNode;
            }

            if (baseNode is TernaryOperationBaseNode ternaryOperationNode)
            {
                assignmentBaseNode.Type = AssignmentTypes.Ternary;
                assignmentBaseNode.Ternary = ternaryOperationNode;
            }

            return assignmentBaseNode;
        }

        return null;
    }

    public override AstBaseNode VisitBitwiseAnd( BITEParser.BitwiseAndContext context )
    {
        if ( context.equality().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.EqualityContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitEquality( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "&" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.BitwiseAnd;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.EqualityContext rhsContext )
                    {
                        currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitEquality( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( "&" ) )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitEquality( context.equality( 0 ) );
    }

    public override AstBaseNode VisitBitwiseOr( BITEParser.BitwiseOrContext context )
    {
        if ( context.bitwiseXor().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.BitwiseXorContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitBitwiseXor( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "|" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.BitwiseOr;
                    }
                }
                else if ( context.GetChild( counter ) is BITEParser.BitwiseXorContext rhsContext )
                {
                    currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                    currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                    currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                    currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                    currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitBitwiseXor( rhsContext );
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( " |" ) )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitBitwiseXor( context.bitwiseXor( 0 ) );
    }

    public override AstBaseNode VisitBitwiseXor( BITEParser.BitwiseXorContext context )
    {
        if ( context.bitwiseAnd().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.BitwiseAndContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitBitwiseAnd( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "^" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.BitwiseXor;
                    }
                }
                else if ( context.GetChild( counter ) is BITEParser.BitwiseAndContext rhsContext )
                {
                    currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                    currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                    currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                    currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                    currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitBitwiseAnd( rhsContext );
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( "^" ) )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitBitwiseAnd( context.bitwiseAnd( 0 ) );
    }

    public override AstBaseNode VisitBlock( BITEParser.BlockContext context )
    {
        BlockStatementBaseNode blockStatementBaseNode = new BlockStatementBaseNode();
        blockStatementBaseNode.DeclarationsBase = new DeclarationsBaseNode();
        blockStatementBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        blockStatementBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        blockStatementBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        blockStatementBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        foreach ( BITEParser.DeclarationContext declarationContext in context.declaration() )
        {
            AstBaseNode decl = VisitDeclaration( declarationContext );

            if ( decl is ClassDeclarationBaseNode classDeclarationNode )
            {
                blockStatementBaseNode.DeclarationsBase.Classes.Add( classDeclarationNode );
            }
            else if ( decl is FunctionDeclarationBaseNode functionDeclarationNode )
            {
                blockStatementBaseNode.DeclarationsBase.Functions.Add( functionDeclarationNode );
            }
            else if ( decl is StructDeclarationBaseNode structDeclaration )
            {
                blockStatementBaseNode.DeclarationsBase.Structs.Add( structDeclaration );
            }
            else if ( decl is VariableDeclarationBaseNode variable )
            {
                blockStatementBaseNode.DeclarationsBase.Variables.Add( variable );
            }
            else if ( decl is ClassInstanceDeclarationBaseNode classInstance )
            {
                blockStatementBaseNode.DeclarationsBase.ClassInstances.Add( classInstance );
            }
            else if ( decl is StatementBaseNode statement )
            {
                blockStatementBaseNode.DeclarationsBase.Statements.Add( statement );
            }
        }

        return blockStatementBaseNode;
    }

    public override AstBaseNode VisitSyncBlock( BITEParser.SyncBlockContext context )
    {
        SyncBlockNode syncBlockNode = new SyncBlockNode();
        syncBlockNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        syncBlockNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        if ( context.block() != null )
        {
            syncBlockNode.Block = ( BlockStatementBaseNode ) VisitBlock( context.block() );
        }

        syncBlockNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        syncBlockNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        return syncBlockNode;
    }

    public override AstBaseNode VisitBreakStatement( BITEParser.BreakStatementContext context )
    {
        if ( context.Break() != null )
        {
            BreakStatementBaseNode breakStatementBaseNode = new BreakStatementBaseNode();
            breakStatementBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            breakStatementBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            breakStatementBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            breakStatementBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            return breakStatementBaseNode;
        }

        return base.VisitBreakStatement( context );
    }

    public override AstBaseNode VisitCall( BITEParser.CallContext context )
    {
        if ( context.ChildCount > 0 )
        {
            int childCounter = 0;
            CallBaseNode callBaseNode = new CallBaseNode();
            callBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            callBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            callBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            callBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
            IParseTree tree = context.GetChild( childCounter );

            if ( tree is BITEParser.PrimaryContext )
            {
                CallEntry currentIdentifier = null;
                callBaseNode.PrimaryBase = ( PrimaryBaseNode ) VisitPrimary( context.primary() );
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
                        if ( callBaseNode.CallEntries == null )
                        {
                            callBaseNode.CallEntries = new List < CallEntry >();
                        }

                        PrimaryBaseNode primaryBaseNode = new PrimaryBaseNode();
                        primaryBaseNode.PrimaryId = new Identifier();
                        primaryBaseNode.PrimaryId.Id = indentifier.Symbol.Text;
                        primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.Identifier;
                        CallEntry callEntry = new CallEntry();
                        callEntry.PrimaryBase = primaryBaseNode;
                        callBaseNode.CallEntries.Add( callEntry );
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
                            callBaseNode.IsFunctionCall = true;
                        }

                        if ( callArgumentsContext.arguments() != null )
                        {
                            if ( currentIdentifier != null &&
                                 callArgumentsContext.
                                     arguments() !=
                                 null )
                            {
                                currentIdentifier.ArgumentsBase = new ArgumentsBaseNode();
                                currentIdentifier.ArgumentsBase.Expressions = new List < ExpressionBaseNode >();
                                currentIdentifier.ArgumentsBase.IsReference = new List < bool >();

                                foreach ( BITEParser.ArgumentExpressionContext expressionContext in
                                         callArgumentsContext.
                                             arguments().
                                             argumentExpression() )
                                {
                                    if ( expressionContext.ReferenceOperator() != null )
                                    {
                                        currentIdentifier.ArgumentsBase.IsReference.Add( true );
                                    }
                                    else
                                    {
                                        currentIdentifier.ArgumentsBase.IsReference.Add( false );
                                    }

                                    currentIdentifier.ArgumentsBase.
                                                      Expressions.Add(
                                                          ( ExpressionBaseNode ) VisitExpression(
                                                              expressionContext.expression() ) );
                                }

                                childCounter++;
                            }
                            else
                            {
                                callBaseNode.ArgumentsBase = new ArgumentsBaseNode();
                                callBaseNode.ArgumentsBase.Expressions = new List < ExpressionBaseNode >();
                                callBaseNode.ArgumentsBase.IsReference = new List < bool >();

                                foreach ( BITEParser.ArgumentExpressionContext expressionContext in
                                         callArgumentsContext.
                                             arguments().
                                             argumentExpression() )
                                {
                                    if ( expressionContext.ReferenceOperator() != null )
                                    {
                                        callBaseNode.ArgumentsBase.IsReference.Add( true );
                                    }
                                    else
                                    {
                                        callBaseNode.ArgumentsBase.IsReference.Add( false );
                                    }

                                    callBaseNode.ArgumentsBase.Expressions.Add(
                                        ( ExpressionBaseNode ) VisitExpression( expressionContext.expression() ) );
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
                                    callElementEntry.CallBase = ( CallBaseNode ) VisitCall( elementIdentifierContext.call() );

                                    callElementEntry.CallElementType =
                                        CallElementTypes.Call;
                                }

                                if ( elementIdentifierContext.@string() != null && elementIdentifierContext.@string().stringPart(0).TEXT() != null )
                                {
                                    string literal = elementIdentifierContext.@string().stringPart(0).TEXT().Symbol.Text;

                                    callElementEntry.Identifier = literal;

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
                            callBaseNode.ElementAccess = new List < CallElementEntry >();

                            foreach ( BITEParser.ElementIdentifierContext elementIdentifierContext in elementAccess.
                                         elementIdentifier() )
                            {
                                CallElementEntry callElementEntry = new CallElementEntry();

                                if ( elementIdentifierContext.call() != null )
                                {
                                    callElementEntry.CallBase = ( CallBaseNode ) VisitCall( elementIdentifierContext.call() );

                                    callElementEntry.CallElementType =
                                        CallElementTypes.Call;
                                }

                                if ( elementIdentifierContext.@string() != null )
                                {
                                    string literal = elementIdentifierContext.@string().stringPart(0).TEXT().Symbol.Text;

                                    callElementEntry.Identifier = literal;

                                    callElementEntry.CallElementType =
                                        CallElementTypes.StringLiteral;
                                }

                                if ( elementIdentifierContext.IntegerLiteral() != null )
                                {
                                    callElementEntry.Identifier = elementIdentifierContext.IntegerLiteral().Symbol.Text;

                                    callElementEntry.CallElementType =
                                        CallElementTypes.IntegerLiteral;
                                }

                                callBaseNode.ElementAccess.Add( callElementEntry );
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

            if ( callBaseNode.CallEntries != null && callBaseNode.ArgumentsBase != null )
            {
                callBaseNode.CallType = CallTypes.PrimaryCall;
            }
            else if ( callBaseNode.CallEntries != null )
            {
                callBaseNode.CallType = CallTypes.PrimaryCall;
            }
            else if ( callBaseNode.ArgumentsBase != null )
            {
                callBaseNode.CallType = CallTypes.PrimaryCall;
            }
            else if ( callBaseNode.IsFunctionCall )
            {
                callBaseNode.CallType = CallTypes.PrimaryCall;
            }
            else if ( callBaseNode.ElementAccess != null )
            {
                callBaseNode.CallType = CallTypes.PrimaryCall;
            }
            else
            {
                callBaseNode.CallType = CallTypes.Primary;
            }

            return callBaseNode;
        }

        return null;
    }

    public override AstBaseNode VisitCallArguments( BITEParser.CallArgumentsContext context )
    {
        return base.VisitCallArguments( context );
    }

    public override AstBaseNode VisitClassDeclaration( BITEParser.ClassDeclarationContext context )
    {
        ClassDeclarationBaseNode classDeclarationBaseNode = new ClassDeclarationBaseNode();
        classDeclarationBaseNode.ClassId = new Identifier();
        classDeclarationBaseNode.ClassId.Id = context.Identifier().Symbol.Text;
        classDeclarationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        classDeclarationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        classDeclarationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        classDeclarationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.block() != null )
        {
            classDeclarationBaseNode.BlockStatementBase = ( BlockStatementBaseNode ) VisitBlock( context.block() );
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

        ModifiersBaseNode modifiersBase = new ModifiersBaseNode( accessToken, abstractStaticMod );
        classDeclarationBaseNode.ModifiersBase = modifiersBase;

        if ( context.inheritance() != null )
        {
            classDeclarationBaseNode.Inheritance = new List < Identifier >();

            foreach ( ITerminalNode terminalNode in context.inheritance().Identifier() )
            {
                Identifier id = new Identifier();
                id.Id = terminalNode.Symbol.Text;
                classDeclarationBaseNode.Inheritance.Add( id );
            }
        }

        return classDeclarationBaseNode;
    }


    public override AstBaseNode VisitMemberInitialization( BITEParser.MemberInitializationContext context )
    {
        return new MemberInitializationNode()
        {
            Identifier = new Identifier( context.Identifier().Symbol.Text ),
            Expression = (ExpressionBaseNode) VisitExpression( context.expression() )
        };
    }


    public override AstBaseNode VisitClassInstanceDeclaration( BITEParser.ClassInstanceDeclarationContext context )
    {
        ClassInstanceDeclarationBaseNode classInstanceDeclarationBaseNode = new ClassInstanceDeclarationBaseNode();
        classInstanceDeclarationBaseNode.InstanceId = new Identifier();
        classInstanceDeclarationBaseNode.InstanceId.Id = context.Identifier( 0 ).Symbol.Text;
        classInstanceDeclarationBaseNode.ClassName = new Identifier();
        classInstanceDeclarationBaseNode.ClassName.Id = context.Identifier( 1 ).Symbol.Text;
        classInstanceDeclarationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        classInstanceDeclarationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        classInstanceDeclarationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        classInstanceDeclarationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.DeclareVariable() != null )
        {
            classInstanceDeclarationBaseNode.IsVariableRedeclaration = false;
        }
        else
        {
            classInstanceDeclarationBaseNode.IsVariableRedeclaration = true;
        }

        if ( context.arguments() != null )
        {
            classInstanceDeclarationBaseNode.ArgumentsBase = ( ArgumentsBaseNode ) VisitArguments( context.arguments() );
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


        ModifiersBaseNode modifiersBase = new ModifiersBaseNode( accessToken, abstractStaticMod );
        classInstanceDeclarationBaseNode.ModifiersBase = modifiersBase;

        BITEParser.InitializerExpressionContext initializerExpression = context.initializerExpression();

        if ( initializerExpression != null )
        {
            classInstanceDeclarationBaseNode.Initializers = new List < MemberInitializationNode >();
            foreach ( var initialization in initializerExpression.memberInitialization() )
            {
                classInstanceDeclarationBaseNode.Initializers.Add(
                    ( MemberInitializationNode ) VisitMemberInitialization( initialization ) );
            }
        }


        return classInstanceDeclarationBaseNode;
    }

    public override AstBaseNode VisitDeclaration( BITEParser.DeclarationContext context )
    {
        if ( context.classDeclaration() != null )
        {
            return VisitClassDeclaration( context.classDeclaration() );
        }

        if ( context.structDeclaration() != null )
        {
            return VisitStructDeclaration( context.structDeclaration() );
        }

        if (context.externalFunctionDeclaration() != null)
        {
            return VisitExternalFunctionDeclaration( context.externalFunctionDeclaration() );
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
            StatementBaseNode statementBase = VisitStatement( context.statement() ) as StatementBaseNode;
            statementBase.DebugInfoAstNode.LineNumberStart = context.statement().Start.Line;
            statementBase.DebugInfoAstNode.LineNumberEnd = context.statement().Stop.Line;

            statementBase.DebugInfoAstNode.ColumnNumberStart = context.statement().Start.Column;
            statementBase.DebugInfoAstNode.ColumnNumberEnd = context.statement().Stop.Column;

            return statementBase;
        }

        return null;
    }

    public override AstBaseNode VisitElementAccess( BITEParser.ElementAccessContext context )
    {
        return base.VisitElementAccess( context );
    }

    public override AstBaseNode VisitElementIdentifier( BITEParser.ElementIdentifierContext context )
    {
        return base.VisitElementIdentifier( context );
    }

    public override AstBaseNode VisitEquality( BITEParser.EqualityContext context )
    {
        if ( context.relational().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.RelationalContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitRelational( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "==" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Equal;
                    }

                    if ( terminalNode.GetText() == "!=" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.NotEqual;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.RelationalContext rhsContext )
                    {
                        currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                        currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitRelational( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitRelational( context.relational( 0 ) );
    }

    public override AstBaseNode VisitExpression( BITEParser.ExpressionContext context )
    {
        ExpressionBaseNode expressionBaseNode = new ExpressionBaseNode();
        expressionBaseNode.AssignmentBase = ( AssignmentBaseNode ) VisitAssignment( context.assignment() );
        expressionBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        expressionBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        expressionBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        expressionBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        return expressionBaseNode;
    }

    public override AstBaseNode VisitExprStatement( BITEParser.ExprStatementContext context )
    {
        ExpressionStatementBaseNode expressionStatementBaseNode = new ExpressionStatementBaseNode();
        expressionStatementBaseNode.ExpressionBase = ( ExpressionBaseNode ) VisitExpression( context.expression() );
        expressionStatementBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        expressionStatementBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        expressionStatementBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        expressionStatementBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        return expressionStatementBaseNode;
    }

    public override AstBaseNode VisitLocalVarDeclaration( BITEParser.LocalVarDeclarationContext context )
    {
        LocalVariableDeclarationBaseNode variableDeclarationBaseNode = new LocalVariableDeclarationBaseNode();
        variableDeclarationBaseNode.VarId = new Identifier( context.Identifier().Symbol.Text );
        variableDeclarationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        variableDeclarationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        variableDeclarationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        variableDeclarationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.expression() != null )
        {
            variableDeclarationBaseNode.ExpressionBase = ( ExpressionBaseNode ) VisitExpression( context.expression() );
        }

        string accessToken = "private";

        ModifiersBaseNode modifiersBase = new ModifiersBaseNode( accessToken, null );
        variableDeclarationBaseNode.ModifiersBase = modifiersBase;

        return variableDeclarationBaseNode;
    }

    public override AstBaseNode VisitLocalVarInitializer( BITEParser.LocalVarInitializerContext context )
    {
        LocalVariableInitializerBaseNode localVariableInitializerBaseNode = new LocalVariableInitializerBaseNode();

        var variableDeclarationNodes = new List < LocalVariableDeclarationBaseNode >();

        var localVarDeclarations = context.localVarDeclaration();

        if ( localVarDeclarations != null )
        {
            foreach ( var localVarDeclarationContext in context.localVarDeclaration() )
            {
                variableDeclarationNodes.Add(
                    ( LocalVariableDeclarationBaseNode ) VisitLocalVarDeclaration( localVarDeclarationContext ) );
            }
        }

        localVariableInitializerBaseNode.VariableDeclarations = variableDeclarationNodes;

        return localVariableInitializerBaseNode;
    }

    public override AstBaseNode VisitForInitializer( BITEParser.ForInitializerContext context )
    {
        ForInitializerBaseNode forInitializerBaseNode = new ForInitializerBaseNode();

        var expressions = context.expression();

        if ( expressions != null && expressions.Length > 0 )
        {
            forInitializerBaseNode.Expressions = new ExpressionBaseNode[expressions.Length];
            int i = 0;

            foreach ( var expression in expressions )
            {
                forInitializerBaseNode.Expressions[i++] = ( ExpressionBaseNode ) VisitExpression( expression );
            }
        }

        var localVarInitializer = context.localVarInitializer();

        if ( localVarInitializer != null )
        {
            forInitializerBaseNode.LocalVariableInitializerBase =
                ( LocalVariableInitializerBaseNode ) VisitLocalVarInitializer( localVarInitializer );
        }

        return forInitializerBaseNode;
    }

    public override AstBaseNode VisitForStatement( BITEParser.ForStatementContext context )
    {
        ForStatementBaseNode forStatementBaseNode = new ForStatementBaseNode();
        forStatementBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        forStatementBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        forStatementBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        forStatementBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        var initializer = context.forInitializer();

        if ( initializer != null )
        {
            forStatementBaseNode.InitializerBase = ( ForInitializerBaseNode ) VisitForInitializer( initializer );
        }

        if ( context.condition != null )
        {
            forStatementBaseNode.Condition = ( ExpressionBaseNode ) VisitExpression( context.condition );
        }

        var iterators = context.forIterator();

        if ( iterators != null )
        {
            var expressions = iterators.expression();

            forStatementBaseNode.Iterators = new ExpressionBaseNode[expressions.Length];
            var i = 0;

            foreach ( var iterator in expressions )
            {
                forStatementBaseNode.Iterators[i++] = ( ExpressionBaseNode ) VisitExpression( iterator );
            }
        }

        forStatementBaseNode.StatementBase = ( StatementBaseNode ) VisitStatement( context.statement() );

        return forStatementBaseNode;
    }

    public override AstBaseNode VisitExternalFunctionDeclaration( BITEParser.ExternalFunctionDeclarationContext context )
    {
        FunctionDeclarationBaseNode functionDeclarationBaseNode = new FunctionDeclarationBaseNode();
        functionDeclarationBaseNode.FunctionId = new Identifier();
        functionDeclarationBaseNode.FunctionId.Id = context.Identifier()[0].Symbol.Text;
        functionDeclarationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        functionDeclarationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        functionDeclarationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        functionDeclarationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        string accessToken;
        string abstractStaticMod = null;
        bool isExtern = false;
        bool isCallable = false;

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

        if ( context.ExternModifier() != null )
        {
            isExtern = true;
        }

        if ( context.CallableModifier() != null )
        {
            isCallable = true;
            // TODO: define callable link name semantics
            string linkName = context.Identifier()[0].GetText();
            if ( context.Identifier().Length > 1 )
            {
                linkName = context.Identifier()[0].GetText();
            }
            functionDeclarationBaseNode.LinkFunctionId = new Identifier( linkName );
        }

        ModifiersBaseNode modifiersBase = new ModifiersBaseNode( accessToken, abstractStaticMod, isExtern, isCallable );

        functionDeclarationBaseNode.ModifiersBase = modifiersBase;

        if ( context.parameters() != null )
        {
            functionDeclarationBaseNode.ParametersBase = ( ParametersBaseNode ) VisitParameters( context.parameters() );
        }

        return functionDeclarationBaseNode;
    }

    public override AstBaseNode VisitFunctionDeclaration( BITEParser.FunctionDeclarationContext context )
    {
        FunctionDeclarationBaseNode functionDeclarationBaseNode = new FunctionDeclarationBaseNode();
        functionDeclarationBaseNode.FunctionId = new Identifier();
        functionDeclarationBaseNode.FunctionId.Id = context.Identifier().Symbol.Text;
        functionDeclarationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        functionDeclarationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        functionDeclarationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        functionDeclarationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.block() != null )
        {
            functionDeclarationBaseNode.FunctionBlock = ( BlockStatementBaseNode ) VisitBlock( context.block() );
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

        ModifiersBaseNode modifiersBase = new ModifiersBaseNode( accessToken, abstractStaticMod, false, false );

        functionDeclarationBaseNode.ModifiersBase = modifiersBase;

        if ( context.parameters() != null )
        {
            functionDeclarationBaseNode.ParametersBase = ( ParametersBaseNode ) VisitParameters( context.parameters() );
        }

        return functionDeclarationBaseNode;
    }

    public override AstBaseNode VisitIfStatement( BITEParser.IfStatementContext context )
    {
        IfStatementBaseNode ifStatementBaseNode = new IfStatementBaseNode();
        ifStatementBaseNode.ExpressionBase = ( ExpressionBaseNode ) VisitExpression( context.expression() );
        ifStatementBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        ifStatementBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        ifStatementBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        ifStatementBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.trueStatement != null )
        {
            ifStatementBaseNode.ThenStatementBase = ( StatementBaseNode ) VisitStatement( context.trueStatement );
        }

        if ( context.falseStatement != null )
        {
            ifStatementBaseNode.ElseStatementBase = ( StatementBaseNode ) VisitStatement( context.falseStatement );
        }

        return ifStatementBaseNode;
    }

    public override AstBaseNode VisitImportDirective( BITEParser.ImportDirectiveContext context )
    {
        return base.VisitImportDirective( context );
    }

    public override AstBaseNode VisitInheritance( BITEParser.InheritanceContext context )
    {
        return base.VisitInheritance( context );
    }

    public override AstBaseNode VisitLogicAnd( BITEParser.LogicAndContext context )
    {
        if ( context.bitwiseOr().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.BitwiseOrContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitBitwiseOr( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "&&" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.And;
                    }
                }
                else if ( context.GetChild( counter ) is BITEParser.BitwiseOrContext rhsContext )
                {
                    currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                    currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                    currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                    currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                    currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitBitwiseOr( rhsContext );
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( " &&" ) )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitBitwiseOr( context.bitwiseOr( 0 ) );
    }

    public override AstBaseNode VisitLogicOr( BITEParser.LogicOrContext context )
    {
        if ( context.logicAnd().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.LogicAndContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitLogicAnd( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "||" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Or;
                    }
                }
                else if ( context.GetChild( counter ) is BITEParser.LogicAndContext rhsContext )
                {
                    currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                    currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                    currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                    currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                    currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitLogicAnd( rhsContext );
                }

                if ( counter < context.ChildCount - 2 &&
                     context.GetChild( counter + 1 ) is TerminalNodeImpl termina &&
                     termina.Symbol.Text.Equals( " ||" ) )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitLogicAnd( context.logicAnd( 0 ) );
    }

    public override AstBaseNode VisitModule( BITEParser.ModuleContext context )
    {
        ModuleBaseNode moduleBaseNode = new ModuleBaseNode();
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

        moduleBaseNode.ModuleIdent = new ModuleIdentifier( moduleName, parentModules );

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

            if ( moduleBaseNode.ModuleIdent.ToString().Equals( importedModules[importedModules.Count - 1].ToString() ) )
            {
                throw new BiteAstGeneratorException(
                    $"Ast Generation Error: Imported module with name: '{importedModules[importedModules.Count - 1]}' has the same name as currently compiled Module! " );
            }
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

        moduleBaseNode.ImportedModules = importedModules;
        moduleBaseNode.UsedModules = usedModules;

        List < StatementBaseNode > _statements = new List < StatementBaseNode >();

        foreach ( BITEParser.DeclarationContext declarationContext in context.declaration() )
        {
            _statements.Add( ( StatementBaseNode ) VisitDeclaration( declarationContext ) );
        }

        moduleBaseNode.Statements = _statements;

        return moduleBaseNode;
    }

    public override AstBaseNode VisitMultiplicative( BITEParser.MultiplicativeContext context )
    {
        if ( context.unary().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.UnaryContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitUnary( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "/" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Div;
                    }

                    if ( terminalNode.GetText() == "*" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Mult;
                    }

                    if ( terminalNode.GetText() == "%" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Modulo;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.UnaryContext rhsContext )
                    {
                        currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                        currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitUnary( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitUnary( context.unary( 0 ) );
    }

    public override AstBaseNode VisitParameters( BITEParser.ParametersContext context )
    {
        ParametersBaseNode parametersBaseNode = new ParametersBaseNode();
        parametersBaseNode.Identifiers = new List < Identifier >();
        parametersBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        parametersBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        parametersBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        parametersBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        foreach ( BITEParser.ParametersIdentifierContext parametersIdentifier in context.parametersIdentifier() )
        {
            parametersBaseNode.Identifiers.Add( new Identifier( parametersIdentifier.Identifier().Symbol.Text ) );
        }

        return parametersBaseNode;
    }

    public override AstBaseNode VisitPrimary( BITEParser.PrimaryContext context )
    {
        PrimaryBaseNode primaryBaseNode = new PrimaryBaseNode();
        primaryBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        primaryBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        primaryBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        primaryBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.expression() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.Expression;
            primaryBaseNode.Expression = ( ExpressionBaseNode ) VisitExpression( context.expression() );
        }

        if ( context.BooleanLiteral() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.BooleanLiteral;
            primaryBaseNode.BooleanLiteral = bool.Parse( context.BooleanLiteral().Symbol.Text );
        }

        if ( context.NullReference() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.NullReference;
            primaryBaseNode.PrimaryId = new Identifier( context.NullReference().Symbol.Text );
        }

        if ( context.ThisReference() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.ThisReference;
            primaryBaseNode.PrimaryId = new Identifier( context.ThisReference().Symbol.Text );
        }

        if ( context.IntegerLiteral() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.IntegerLiteral;
            primaryBaseNode.IntegerLiteral = int.Parse( context.IntegerLiteral().Symbol.Text, NumberFormatInfo.InvariantInfo );
        }

        if ( context.FloatingLiteral() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.FloatLiteral;
            primaryBaseNode.FloatLiteral = double.Parse( context.FloatingLiteral().Symbol.Text, NumberFormatInfo.InvariantInfo );
        }

        if ( context.@string() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.StringLiteral;

            string lastText = "";
            string interPolated = "";
            foreach ( BITEParser.StringPartContext stringContent in context.@string().stringPart() )
            {
                if ( stringContent.expression() != null )
                {
                    if ( primaryBaseNode.InterpolatedString == null )
                    {
                        primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.InterpolatedString;
                        primaryBaseNode.InterpolatedString = new InterpolatedString();
                        primaryBaseNode.InterpolatedString.StringParts = new List < InterpolatedStringPart >();
                    }
                    ExpressionBaseNode expressionBaseNode = ( ExpressionBaseNode ) VisitExpression( stringContent.expression() );
                    if ( expressionBaseNode != null )
                    {
                        primaryBaseNode.InterpolatedString.StringParts.Add( new InterpolatedStringPart( interPolated, expressionBaseNode) );
                        interPolated = "";
                    }
                }

                if ( stringContent.TEXT() != null )
                {
                    lastText = stringContent.TEXT().Symbol.Text;
                    interPolated += lastText;
                    primaryBaseNode.StringLiteral += lastText;
                }

                if ( stringContent.ESCAPE_SEQUENCE() != null )
                {
                    lastText = stringContent.ESCAPE_SEQUENCE().Symbol.Text;
                    lastText = lastText.Substring( 1, lastText.Length - 1 );
                    interPolated += lastText;
                    primaryBaseNode.StringLiteral += lastText;
                }

            }

            if ( primaryBaseNode.PrimaryType == PrimaryBaseNode.PrimaryTypes.InterpolatedString )
            {
                primaryBaseNode.InterpolatedString.TextAfterLastExpression = interPolated;
            }

        }

        /**/


        if ( context.Identifier() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.Identifier;
            primaryBaseNode.PrimaryId = new Identifier( context.Identifier().Symbol.Text );
        }

        if ( context.arrayExpression() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.ArrayExpression;
            primaryBaseNode.Expression = ( ArrayExpressionNode ) VisitArrayExpression( context.arrayExpression() );
        }

        if ( context.dictionaryExpression() != null )
        {
            primaryBaseNode.PrimaryType = PrimaryBaseNode.PrimaryTypes.DictionaryExpression;
            primaryBaseNode.Expression = ( DictionaryInitializerNode ) VisitDictionaryExpression( context.dictionaryExpression() );
        }

        return primaryBaseNode;
    }

    public override AstBaseNode VisitArrayExpression( BITEParser.ArrayExpressionContext context )
    {
        var initializerNode = new ArrayExpressionNode();

        if ( context.expression() != null )
        {
            initializerNode.Expressions = new List < ExpressionBaseNode >();
            foreach ( var expression in context.expression() )
            {
                initializerNode.Expressions.Add( (ExpressionBaseNode) VisitExpression( expression ) );
            }
        }

        return initializerNode;
    }

    public override AstBaseNode VisitDictionaryExpression( BITEParser.DictionaryExpressionContext context )
    {
        var initializerNode = new DictionaryInitializerNode();

        if (context.elementInitialization() != null)
        {
            initializerNode.ElementInitializers = new Dictionary < Identifier, ExpressionBaseNode >();

            foreach ( var initialization in context.elementInitialization() )
            {
                initializerNode.ElementInitializers.Add(
                    new Identifier( initialization.Identifier().Symbol.Text ),
                    ( ExpressionBaseNode ) VisitExpression( initialization.expression() )
                );
            }
        }

        return initializerNode;
    }



        public override AstBaseNode VisitProgram( BITEParser.ProgramContext context )
    {
        m_ProgramBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        m_ProgramBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        m_ProgramBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        m_ProgramBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        foreach ( BITEParser.ModuleContext declarationContext in context.module() )
        {
            AstBaseNode decl = VisitModule( declarationContext );

            m_ProgramBaseNode.AddModule( decl as ModuleBaseNode );
        }

        return m_ProgramBaseNode;
    }

    public override AstBaseNode VisitRelational( BITEParser.RelationalContext context )
    {
        if ( context.shift().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.ShiftContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitShift( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "<" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Less;
                    }

                    if ( terminalNode.GetText() == ">" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.Greater;
                    }

                    if ( terminalNode.GetText() == ">=" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.GreaterOrEqual;
                    }

                    if ( terminalNode.GetText() == "<=" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.LessOrEqual;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.ShiftContext rhsContext )
                    {
                        currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                        currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitShift( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitShift( context.shift( 0 ) );
    }

    public override AstBaseNode VisitReturnStatement( BITEParser.ReturnStatementContext context )
    {
        ReturnStatementBaseNode returnStatementBaseNode = new ReturnStatementBaseNode();
        returnStatementBaseNode.ExpressionStatementBase = new ExpressionStatementBaseNode();
        returnStatementBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        returnStatementBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        returnStatementBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        returnStatementBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        returnStatementBaseNode.ExpressionStatementBase.ExpressionBase = ( ExpressionBaseNode ) VisitExpression( context.expression() );

        return returnStatementBaseNode;
    }

    public override AstBaseNode VisitShift( BITEParser.ShiftContext context )
    {
        if ( context.additive().Length > 1 )
        {
            int counter = 0;
            BinaryOperationBaseNode firstBinaryOperationBaseNode = new BinaryOperationBaseNode();
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            if ( context.GetChild( counter ) is BITEParser.AdditiveContext lhsContext )
            {
                firstBinaryOperationBaseNode.LeftOperand = ( ExpressionBaseNode ) VisitAdditive( lhsContext );
            }

            counter++;

            BinaryOperationBaseNode currentBinaryOperationBaseNode = firstBinaryOperationBaseNode;

            while ( counter < context.ChildCount )
            {
                if ( context.GetChild( counter ) is TerminalNodeImpl terminalNode )
                {
                    if ( terminalNode.GetText() == "<<" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.ShiftLeft;
                    }

                    if ( terminalNode.GetText() == ">>" )
                    {
                        currentBinaryOperationBaseNode.Operator = BinaryOperationBaseNode.BinaryOperatorType.ShiftRight;
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.AdditiveContext rhsContext )
                    {
                        currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationBaseNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;

                        currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationBaseNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationBaseNode.RightOperand = ( ExpressionBaseNode ) VisitAdditive( rhsContext );
                    }
                }

                if ( counter < context.ChildCount - 2 && context.GetChild( counter + 1 ) is TerminalNodeImpl termina )
                {
                    BinaryOperationBaseNode binaryOperationBaseNode = new BinaryOperationBaseNode();
                    binaryOperationBaseNode.LeftOperand = currentBinaryOperationBaseNode;
                    currentBinaryOperationBaseNode = binaryOperationBaseNode;
                }
                else
                {
                    firstBinaryOperationBaseNode = currentBinaryOperationBaseNode;
                }

                counter++;
            }

            return firstBinaryOperationBaseNode;
        }

        return VisitAdditive( context.additive( 0 ) );
    }

    /// <summary>
    /// This is primarily for unit testing
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override AstBaseNode VisitStatements( BITEParser.StatementsContext context )
    {
        DeclarationsBaseNode declarationsBase = new DeclarationsBaseNode()
        {
            Statements = new List < StatementBaseNode >()
        };

        foreach ( var declaration in context.declaration() )
        {
            declarationsBase.Statements.Add( ( StatementBaseNode ) VisitDeclaration( declaration ) );
        }

        return declarationsBase;
    }

    public override AstBaseNode VisitStatement( BITEParser.StatementContext context )
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

        if (context.syncBlock() != null)
        {
            return VisitSyncBlock( context.syncBlock() );
        }

        if ( context.block() != null )
        {
            return VisitBlock( context.block() );
        }

        return null;
    }

    public override AstBaseNode VisitStructDeclaration( BITEParser.StructDeclarationContext context )
    {
        StructDeclarationBaseNode structDeclarationBaseNode = new StructDeclarationBaseNode();
        structDeclarationBaseNode.StructId = new Identifier( context.Identifier().Symbol.Text );
        structDeclarationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        structDeclarationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        structDeclarationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        structDeclarationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        structDeclarationBaseNode.Block = ( BlockStatementBaseNode ) VisitBlock( context.block() );

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

        ModifiersBaseNode modifiersBase = new ModifiersBaseNode( accessToken, abstractStaticMod );
        structDeclarationBaseNode.ModifiersBase = modifiersBase;

        return structDeclarationBaseNode;
    }

    public override AstBaseNode VisitTernary( BITEParser.TernaryContext context )
    {
        if ( context.logicOr().Length > 1 )
        {
            int counter = 0;
            TernaryOperationBaseNode ternary = new TernaryOperationBaseNode();
            ternary.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            ternary.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

            ternary.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            ternary.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

            ternary.LeftOperand = ( ExpressionBaseNode ) VisitLogicOr( context.logicOr( 0 ) );
            ternary.MidOperand = ( ExpressionBaseNode ) VisitLogicOr( context.logicOr( 1 ) );
            ternary.RightOperand = ( ExpressionBaseNode ) VisitLogicOr( context.logicOr( 2 ) );

            return ternary;
        }

        return VisitLogicOr( context.logicOr( 0 ) );
    }

    public override AstBaseNode VisitUnary( BITEParser.UnaryContext context )
    {
        if ( context.unary() != null )
        {
            int counter = 0;

            if ( context.GetChild( counter ) is BITEParser.UnaryContext lhsContext )
            {
                UnaryPostfixOperation unaryOperationNode = new UnaryPostfixOperation();
                unaryOperationNode.Primary = ( ExpressionBaseNode ) VisitUnary( lhsContext );

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
                unaryOperationNode.Primary = ( ExpressionBaseNode ) VisitUnary( context.unary() );

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

    public override AstBaseNode VisitUsingStatement( BITEParser.UsingStatementContext context )
    {
        UsingStatementBaseNode usingStatementBaseNode = new UsingStatementBaseNode();
        usingStatementBaseNode.UsingBaseNode = VisitExpression( context.expression() );
        usingStatementBaseNode.UsingBlock = ( BlockStatementBaseNode ) VisitBlock( context.block() );

        return usingStatementBaseNode;
    }

    public override AstBaseNode VisitVariableDeclaration( BITEParser.VariableDeclarationContext context )
    {
        VariableDeclarationBaseNode variableDeclarationBaseNode = new VariableDeclarationBaseNode();
        variableDeclarationBaseNode.VarId = new Identifier( context.Identifier().Symbol.Text );
        variableDeclarationBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        variableDeclarationBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        variableDeclarationBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        variableDeclarationBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        if ( context.exprStatement() != null )
        {
            variableDeclarationBaseNode.InitializerBase = new InitializerBaseNode();

            variableDeclarationBaseNode.InitializerBase.Expression =
                ( ExpressionStatementBaseNode ) VisitExprStatement( context.exprStatement() );
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

        ModifiersBaseNode modifiersBase = new ModifiersBaseNode( accessToken, abstractStaticMod );
        variableDeclarationBaseNode.ModifiersBase = modifiersBase;

        return variableDeclarationBaseNode;
    }

    public override AstBaseNode VisitWhileStatement( BITEParser.WhileStatementContext context )
    {
        WhileStatementBaseNode whileStatementBaseNode = new WhileStatementBaseNode();
        whileStatementBaseNode.ExpressionBase = ( ExpressionBaseNode ) VisitExpression( context.expression() );
        whileStatementBaseNode.WhileBlock = ( BlockStatementBaseNode ) VisitBlock( context.block() );

        whileStatementBaseNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        whileStatementBaseNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;

        whileStatementBaseNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        whileStatementBaseNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;

        return whileStatementBaseNode;
    }

    #endregion
}

}
