using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using AntlrBiteParser;
using Bite.Ast;
using Bite.Parser;

namespace MemoizeSharp
{

public class HeteroAstGenerator : BITEBaseVisitor < HeteroAstNode >
{
    private ProgramNode ProgramNode = new ProgramNode("MainModule");

    private bool m_IsLookingForAssingPart = false;
    #region Public

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

                counter++;

                if ( counter < context.ChildCount - 1 )
                {
                    BinaryOperationNode newBinaryOperationNode = new BinaryOperationNode();
                    currentBinaryOperationNode.RightOperand = newBinaryOperationNode;
                    currentBinaryOperationNode = newBinaryOperationNode;

                    if ( context.GetChild( counter ) is BITEParser.MultiplicativeContext rhsContext )
                    {
                        currentBinaryOperationNode.LeftOperand = ( ExpressionNode ) VisitMultiplicative( rhsContext );
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.MultiplicativeContext rhsContext )
                    {
                        currentBinaryOperationNode.RightOperand = ( ExpressionNode ) VisitMultiplicative( rhsContext );
                    }
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }
        else
        {
            return VisitMultiplicative( context.multiplicative( 0 ) );
        }
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

        if ( context.BitwiseLeftShiftAssignOperator() != null )
        {
            AssignmentNode assignmentNode = new AssignmentNode();
            HeteroAstNode node = VisitB( context.logicOr() );
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

            return assignmentNode;
        }

        return null;
    }

    public override HeteroAstNode VisitBlock( BITEParser.BlockContext context )
    {
        BlockStatementNode blockStatementNode = new BlockStatementNode();
        blockStatementNode.DeclarationsNode = new DeclarationsNode();
        blockStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        blockStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
        blockStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        blockStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        
        foreach ( BITEParser.DeclarationContext declarationContext in context.declaration() )
        {
            HeteroAstNode decl = VisitDeclaration( declarationContext );

            if ( decl is NamespaceDeclarationNode namespaceNode )
            {
                blockStatementNode.DeclarationsNode.Namespaces.Add( namespaceNode );
            }
            else if ( decl is ClassDeclarationNode classDeclarationNode )
            {
                blockStatementNode.DeclarationsNode.Classes.Add( classDeclarationNode );
            }
            else if ( decl is FunctionDeclarationNode functionDeclarationNode )
            {
                blockStatementNode.DeclarationsNode.Functions.Add( functionDeclarationNode );
            }
            else if ( decl is StructDeclarationNode structDeclaration )
            {
                blockStatementNode.DeclarationsNode.Structs.Add( structDeclaration );
            }
            else if ( decl is VariableDeclarationNode variable )
            {
                blockStatementNode.DeclarationsNode.Variables.Add( variable );
            }
            else if ( decl is ClassInstanceDeclarationNode classInstance )
            {
                blockStatementNode.DeclarationsNode.ClassInstances.Add( classInstance );
            }
            else if ( decl is StatementNode statement )
            {
                blockStatementNode.DeclarationsNode.Statements.Add( statement );
            }
        }

        return blockStatementNode;
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
                CallNode.CallEntry currentIdentifier = null;
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
                            callNode.CallEntries = new List < CallNode.CallEntry >();
                        }

                        PrimaryNode primaryNode = new PrimaryNode();
                        primaryNode.Identifier = new Identifier();
                        primaryNode.Identifier.Id = indentifier.Symbol.Text;
                        CallNode.CallEntry callEntry = new CallNode.CallEntry();
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

                                childCounter++;
                            }
                        }
                    }
                    else if ( tree is BITEParser.ElementAccessContext elementAccess )
                    {
                        if ( currentIdentifier != null )
                        {
                            currentIdentifier.ElementAccess = new List < CallNode.CallElementEntry >();
                            foreach ( BITEParser.ElementIdentifierContext elementIdentifierContext in elementAccess.elementIdentifier() )
                            {
                                CallNode.CallElementEntry callElementEntry = new CallNode.CallElementEntry();
                                
                                if ( elementIdentifierContext.call() != null )
                                {
                                    callElementEntry.IdentifierNode = (CallNode) VisitCall( elementIdentifierContext.call() );

                                    callElementEntry.CallElementType =
                                        CallNode.CallElementEntry.CallElementTypes.Call;
                                }
                                if ( elementIdentifierContext.StringLiteral() != null )
                                {
                                    callElementEntry.Identifier = elementIdentifierContext.StringLiteral().Symbol.Text;

                                    callElementEntry.CallElementType =
                                        CallNode.CallElementEntry.CallElementTypes.StringLiteral;
                                }
                                if ( elementIdentifierContext.IntegerLiteral() != null )
                                {
                                    callElementEntry.Identifier = elementIdentifierContext.IntegerLiteral().Symbol.Text;

                                    callElementEntry.CallElementType =
                                        CallNode.CallElementEntry.CallElementTypes.IntegerLiteral;
                                }
                                
                                currentIdentifier.ElementAccess.Add( callElementEntry );
                            }
                        }
                        else
                        {
                            callNode.ElementAccess = new List < CallNode.CallElementEntry>();
                            foreach ( BITEParser.ElementIdentifierContext elementIdentifierContext in elementAccess.elementIdentifier() )
                            {
                                CallNode.CallElementEntry callElementEntry = new CallNode.CallElementEntry();
                                
                                if ( elementIdentifierContext.call() != null )
                                {
                                    callElementEntry.IdentifierNode = (CallNode) VisitCall( elementIdentifierContext.call() );

                                    callElementEntry.CallElementType =
                                        CallNode.CallElementEntry.CallElementTypes.Call;
                                }
                                if ( elementIdentifierContext.StringLiteral() != null )
                                {
                                    callElementEntry.Identifier = elementIdentifierContext.StringLiteral().Symbol.Text;

                                    callElementEntry.CallElementType =
                                        CallNode.CallElementEntry.CallElementTypes.StringLiteral;
                                }
                                if ( elementIdentifierContext.IntegerLiteral() != null )
                                {
                                    callElementEntry.Identifier = elementIdentifierContext.IntegerLiteral().Symbol.Text;

                                    callElementEntry.CallElementType =
                                        CallNode.CallElementEntry.CallElementTypes.IntegerLiteral;
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
                callNode.CallType = CallNode.CallTypes.PrimaryCall;
            }
            else if ( callNode.CallEntries != null )
            {
                callNode.CallType = CallNode.CallTypes.PrimaryCall;
            }
            else if ( callNode.Arguments != null )
            {
                callNode.CallType = CallNode.CallTypes.PrimaryCall;
            }
            else if ( callNode.IsFunctionCall )
            {
                callNode.CallType = CallNode.CallTypes.PrimaryCall;
            }
            else if ( callNode.ElementAccess != null )
            {
                callNode.CallType = CallNode.CallTypes.PrimaryCall;
            }
            else
            {
                callNode.CallType = CallNode.CallTypes.Primary;
            }

            return callNode;
        }

        return null;
    }

    public override HeteroAstNode VisitClassDeclaration( BITEParser.ClassDeclarationContext context )
    {
        ClassDeclarationNode classDeclarationNode = new ClassDeclarationNode();
        classDeclarationNode.Identifier = new Identifier();
        classDeclarationNode.Identifier.Id = context.Identifier().Symbol.Text;
        classDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        classDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
        classDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        classDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        if ( context.block() != null )
        {
            classDeclarationNode.BlockStatement = ( BlockStatementNode ) VisitBlock( context.block() );
        }

        Token accessToken = new Token();
        Token abstractStaticMod = new Token();

        if ( context.publicModifier() != null )
        {
            accessToken.text = "public";
            accessToken.type = BiteLexer.DeclarePublic;
        }
        else
        {
            accessToken.text = "private";
            accessToken.type = BiteLexer.DeclarePrivate;
        }

        if ( context.abstractModifier() != null )
        {
            abstractStaticMod.text = "abstract";
            abstractStaticMod.type = BiteLexer.DeclareAbstract;
        }
        else if ( context.staticModifier() != null )
        {
            abstractStaticMod.text = "static";
            abstractStaticMod.type = BiteLexer.DeclareStatic;
        }

        Modifiers Modifiers = new Modifiers( accessToken, abstractStaticMod );
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
        classInstanceDeclarationNode.Identifier = new Identifier();
        classInstanceDeclarationNode.Identifier.Id = context.Identifier( 0 ).Symbol.Text;
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

        Token accessToken = new Token();
        Token abstractStaticMod = new Token();

        if ( context.publicModifier() != null )
        {
            accessToken.text = "public";
            accessToken.type = BiteLexer.DeclarePublic;
        }
        else
        {
            accessToken.text = "private";
            accessToken.type = BiteLexer.DeclarePrivate;
        }

        Modifiers Modifiers = new Modifiers( accessToken, abstractStaticMod );
        classInstanceDeclarationNode.Modifiers = Modifiers;

        return classInstanceDeclarationNode;
    }

    public override HeteroAstNode VisitComparison( BITEParser.ComparisonContext context )
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
                firstBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitAdditive( lhsContext );
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

                counter++;

                if ( counter < context.ChildCount - 1 )
                {
                    BinaryOperationNode newBinaryOperationNode = new BinaryOperationNode();
                    currentBinaryOperationNode.RhsPrimary = newBinaryOperationNode;
                    currentBinaryOperationNode = newBinaryOperationNode;

                    if ( context.GetChild( counter ) is BITEParser.AdditiveContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;
            
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitAdditive( rhsContext );
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
                        currentBinaryOperationNode.RhsPrimary = ( ExpressionNode ) VisitAdditive( rhsContext );
                    }
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }
        else
        {
            return VisitAdditive( context.additive( 0 ) );
        }
    }

    public override HeteroAstNode VisitDeclaration( BITEParser.DeclarationContext context )
    {
        if ( context.namespaceDeclaration() != null )
        {
            return VisitNamespaceDeclaration( context.namespaceDeclaration() );
        }

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

    public override HeteroAstNode VisitEquality( BITEParser.EqualityContext context )
    {
        if ( context.comparison().Length > 1 )
        {
            int counter = 0;
            BinaryOperationNode firstBinaryOperationNode = new BinaryOperationNode();
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
            firstBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
            firstBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
            if ( context.GetChild( counter ) is BITEParser.ComparisonContext lhsContext )
            {
                firstBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitComparison( lhsContext );
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

                counter++;

                if ( counter < context.ChildCount - 1 )
                {
                    BinaryOperationNode newBinaryOperationNode = new BinaryOperationNode();
                    currentBinaryOperationNode.RhsPrimary = newBinaryOperationNode;
                    currentBinaryOperationNode = newBinaryOperationNode;

                    if ( context.GetChild( counter ) is BITEParser.ComparisonContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;
            
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitComparison( rhsContext );
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.ComparisonContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;
            
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.RhsPrimary = ( ExpressionNode ) VisitComparison( rhsContext );
                    }
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }
        else
        {
            return VisitComparison( context.comparison( 0 ) );
        }
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

    public override HeteroAstNode VisitForStatement( BITEParser.ForStatementContext context )
    {
        ForStatementNode forStatementNode = new ForStatementNode();
        forStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        forStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
        forStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        forStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        if ( context.variableDeclaration() != null )
        {
            forStatementNode.VariableDeclaration =
                ( VariableDeclarationNode ) VisitVariableDeclaration( context.variableDeclaration() );
        }

        if ( context.exprStatement() != null )
        {
            forStatementNode.ExpressionStatement =
                ( ExpressionStatementNode ) VisitExprStatement( context.exprStatement() );
        }

        if ( context.expression().Length > 0 )
        {
            forStatementNode.Expression1 = ( ExpressionNode ) VisitExpression( context.expression()[0] );
        }

        if ( context.expression().Length > 1 )
        {
            forStatementNode.Expression2 = ( ExpressionNode ) VisitExpression( context.expression()[1] );
        }

        forStatementNode.Block = ( BlockStatementNode ) VisitBlock( context.block() );

        return forStatementNode;
    }

    public override HeteroAstNode VisitFunctionDeclaration( BITEParser.FunctionDeclarationContext context )
    {
        FunctionDeclarationNode functionDeclarationNode = new FunctionDeclarationNode();
        functionDeclarationNode.Identifier = new Identifier();
        functionDeclarationNode.Identifier.Id = context.Identifier().Symbol.Text;
        functionDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        functionDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
        functionDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        functionDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        if ( context.block() != null )
        {
            functionDeclarationNode.BlockStatementNode = ( BlockStatementNode ) VisitBlock( context.block() );
        }

        Token accessToken = new Token();
        Token abstractStaticMod = new Token();

        if ( context.publicModifier() != null )
        {
            accessToken.text = "public";
            accessToken.type = BiteLexer.DeclarePublic;
        }
        else
        {
            accessToken.text = "private";
            accessToken.type = BiteLexer.DeclarePrivate;
        }

        if ( context.abstractModifier() != null )
        {
            abstractStaticMod.text = "abstract";
            abstractStaticMod.type = BiteLexer.DeclareAbstract;
        }
        else if ( context.staticModifier() != null )
        {
            abstractStaticMod.text = "static";
            abstractStaticMod.type = BiteLexer.DeclareStatic;
        }

        Modifiers Modifiers = new Modifiers( accessToken, abstractStaticMod );
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
        if ( context.block() != null )
        {
            ifStatementNode.ThenBlock = ( BlockStatementNode ) VisitBlock( context.block( 0 ) );
        }

        if ( context.block().Length > 1 )
        {
            ifStatementNode.ElseBlock = ( BlockStatementNode ) VisitBlock( context.block( 1 ) );
        }

        return ifStatementNode;
    }

    public override HeteroAstNode VisitLogicAnd( BITEParser.LogicAndContext context )
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
                firstBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitEquality( lhsContext );
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

                counter++;

                if ( counter < context.ChildCount - 1 )
                {
                    BinaryOperationNode newBinaryOperationNode = new BinaryOperationNode();
                    currentBinaryOperationNode.RhsPrimary = newBinaryOperationNode;
                    currentBinaryOperationNode = newBinaryOperationNode;

                    if ( context.GetChild( counter ) is BITEParser.EqualityContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;
            
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitEquality( rhsContext );
                    }
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.EqualityContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;
            
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.RhsPrimary = ( ExpressionNode ) VisitEquality( rhsContext );
                    }
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }
        else
        {
            return VisitEquality( context.equality( 0 ) );
        }
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
                firstBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitLogicAnd( lhsContext );
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

                counter++;

                if ( counter < context.ChildCount - 1 )
                {
                    BinaryOperationNode newBinaryOperationNode = new BinaryOperationNode();
                    currentBinaryOperationNode.RhsPrimary = newBinaryOperationNode;
                    currentBinaryOperationNode = newBinaryOperationNode;

                    if ( context.GetChild( counter ) is BITEParser.LogicAndContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;
            
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitLogicAnd( rhsContext );
                    }

                    counter++;
                }
                else
                {
                    if ( context.GetChild( counter ) is BITEParser.LogicAndContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;
            
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.RhsPrimary = ( ExpressionNode ) VisitLogicAnd( rhsContext );
                    }

                    counter++;
                }
            }

            return firstBinaryOperationNode;
        }
        else
        {
            return VisitLogicAnd( context.logicAnd( 0 ) );
        }
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
                firstBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitUnary( lhsContext );
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

                counter++;

                if ( counter < context.ChildCount - 1 )
                {
                    BinaryOperationNode newBinaryOperationNode = new BinaryOperationNode();
                    currentBinaryOperationNode.RhsPrimary = newBinaryOperationNode;
                    currentBinaryOperationNode = newBinaryOperationNode;

                    if ( context.GetChild( counter ) is BITEParser.UnaryContext rhsContext )
                    {
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberStart = rhsContext.Start.Line;
                        currentBinaryOperationNode.DebugInfoAstNode.LineNumberEnd = rhsContext.Stop.Line;
            
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberStart = rhsContext.Start.Column;
                        currentBinaryOperationNode.DebugInfoAstNode.ColumnNumberEnd = rhsContext.Stop.Column;
                        currentBinaryOperationNode.LhsPrimary = ( ExpressionNode ) VisitUnary( rhsContext );
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
                        currentBinaryOperationNode.RhsPrimary = ( ExpressionNode ) VisitUnary( rhsContext );
                    }
                }

                counter++;
            }

            return firstBinaryOperationNode;
        }
        else
        {
            return VisitUnary( context.unary( 0 ) );
        }
    }

    public override HeteroAstNode VisitNamespaceDeclaration( BITEParser.NamespaceDeclarationContext context )
    {
        NamespaceDeclarationNode namespaceDeclarationNode = new NamespaceDeclarationNode();
        namespaceDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        namespaceDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
        namespaceDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        namespaceDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        if ( context.Identifier().Length > 1 )
        {
            for ( int i = 0; i < context.Identifier().Length - 1; i++ )
            {
                namespaceDeclarationNode.ParentNamespacesIdentifiers.Add(
                    new Identifier( context.Identifier( i ).Symbol.Text ) );
            }
        }

        namespaceDeclarationNode.Identifier = new Identifier();
        namespaceDeclarationNode.Identifier.Id = context.Identifier( context.Identifier().Length - 1 ).Symbol.Text;
        namespaceDeclarationNode.DeclarationsNode = ( BlockStatementNode ) VisitBlock( context.block() );

        return namespaceDeclarationNode;
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
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Identifier;
            primaryNode.Identifier = new Identifier( context.NullReference().Symbol.Text );
        }

        if ( context.ThisReference() != null )
        {
            primaryNode.PrimaryType = PrimaryNode.PrimaryTypes.Identifier;
            primaryNode.Identifier = new Identifier( context.ThisReference().Symbol.Text );
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
            primaryNode.Identifier = new Identifier( context.Identifier().Symbol.Text );
        }

        return primaryNode;
    }

    public override HeteroAstNode VisitProgram( BITEParser.ProgramContext context )
    {
        ProgramNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        ProgramNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
        ProgramNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        ProgramNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        foreach ( BITEParser.DeclarationContext declarationContext in context.declaration() )
        {
            HeteroAstNode decl = VisitDeclaration( declarationContext );

            if ( decl is NamespaceDeclarationNode namespaceNode )
            {
                ProgramNode.Statements.Add( namespaceNode );
            }

            else if ( decl is ClassDeclarationNode classDeclarationNode )
            {
                ProgramNode.Statements.Add( classDeclarationNode );
            }

            else if ( decl is StructDeclarationNode structDeclaration )
            {
                ProgramNode.Statements.Add( structDeclaration );
            }

            else if ( decl is FunctionDeclarationNode functionDeclarationNode )
            {
                ProgramNode.Statements.Add( functionDeclarationNode );
            }

            else if ( decl is VariableDeclarationNode variable )
            {
                ProgramNode.Statements.Add( variable );
            }

            else if ( decl is ClassInstanceDeclarationNode classInstance )
            {
                ProgramNode.Statements.Add( classInstance );
            }

            else if ( decl is StatementNode statement )
            {
                ProgramNode.Statements.Add( statement );
            }
        }

        return ProgramNode;
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
        structDeclarationNode.Identifier = new Identifier( context.Identifier().Symbol.Text );
        structDeclarationNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        structDeclarationNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
        structDeclarationNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        structDeclarationNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        structDeclarationNode.Block = ( BlockStatementNode ) VisitBlock( context.block() );

        Token accessToken = new Token();
        Token abstractStaticMod = new Token();

        if ( context.publicModifier() != null )
        {
            accessToken.text = "public";
            accessToken.type = BiteLexer.DeclarePublic;
        }
        else
        {
            accessToken.text = "private";
            accessToken.type = BiteLexer.DeclarePrivate;
        }

        Modifiers Modifiers = new Modifiers( accessToken, abstractStaticMod );
        structDeclarationNode.Modifiers = Modifiers;

        return structDeclarationNode;
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

                return unaryOperationNode;
            }
        }

        return VisitCall( context.call() );
    }

    public override HeteroAstNode VisitVariableDeclaration( BITEParser.VariableDeclarationContext context )
    {
        VariableDeclarationNode variableDeclarationNode = new VariableDeclarationNode();
        variableDeclarationNode.Identifier = new Identifier( context.Identifier().Symbol.Text );
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

        Token accessToken = new Token();
        Token abstractStaticMod = new Token();

        if ( context.publicModifier() != null )
        {
            accessToken.text = "public";
            accessToken.type = BiteLexer.DeclarePublic;
        }
        else
        {
            accessToken.text = "private";
            accessToken.type = BiteLexer.DeclarePrivate;
        }

        Modifiers Modifiers = new Modifiers( accessToken, abstractStaticMod );
        variableDeclarationNode.Modifiers = Modifiers;

        return variableDeclarationNode;
    }

    public override HeteroAstNode VisitWhileStatement( BITEParser.WhileStatementContext context )
    {
        WhileStatementNode whileStatementNode = new WhileStatementNode();
        whileStatementNode.Expression = ( ExpressionNode ) VisitExpression( context.expression() );
        whileStatementNode.Block = ( BlockStatementNode ) VisitBlock( context.block() );

        whileStatementNode.DebugInfoAstNode.LineNumberStart = context.Start.Line;
        whileStatementNode.DebugInfoAstNode.LineNumberEnd = context.Stop.Line;
            
        whileStatementNode.DebugInfoAstNode.ColumnNumberStart = context.Start.Column;
        whileStatementNode.DebugInfoAstNode.ColumnNumberEnd = context.Stop.Column;
        
        return whileStatementNode;
    }

    #endregion
}

}
