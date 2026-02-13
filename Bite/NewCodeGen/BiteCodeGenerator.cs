#region

using System.Collections.Generic;
using Bite.Ast;
using Bite.Runtime;

#endregion

namespace Bite.NewCodeGen
{

public class BiteCodeGenerator : AstVisitor < object >, IAstVisitor
{
    private BiteChunkBuilder m_Builder;
    private List < LocalScope > m_Scopes = new List < LocalScope >();
    private int m_NextGlobalSlot = 0;
    private Dictionary < string, int > m_GlobalSlots = new Dictionary < string, int >();
    private List < BiteChunk > m_FunctionChunks = new List < BiteChunk >();
    private string m_CurrentFunctionName = "";

    private class LocalScope
    {
        public Dictionary < string, int > Locals = new Dictionary < string, int >();
        public int NextSlot = 0;

        public void Define( string name, int slot )
        {
            Locals[name] = slot;
        }
    }

    private LocalScope CurrentScope => m_Scopes.Count > 0 ? m_Scopes[m_Scopes.Count - 1] : null;

    #region Public

    public BiteProgram Compile( ModuleBaseNode module )
    {
        m_Builder = new BiteChunkBuilder( "main" );
        
        Compile( (AstBaseNode)module );
        
        m_Builder.RetVal();
        
        BiteChunk mainChunk = m_Builder.Build();
        
        return new BiteProgram( mainChunk, m_FunctionChunks );
    }

    #endregion

    #region Compile Dispatch Methods

    /// <summary>
    /// Main dispatch method for all AstBaseNode types.
    /// Cases are ordered from most derived to least derived (bottom-up in hierarchy)
    /// to ensure correct pattern matching.
    /// </summary>
    private void Compile( AstBaseNode node )
    {
        switch ( node )
        {
            // Program/Module (root level)
            case ProgramBaseNode program:
                CompileProgram( program );
                break;
            case ModuleBaseNode module:
                CompileModule( module );
                break;

            // Declarations (most derived first)
            case DeclarationsBaseNode declarations:
                CompileDeclarations( declarations );
                break;
            case FunctionDeclarationBaseNode functionDecl:
                CompileFunctionDeclaration( functionDecl );
                break;
            case VariableDeclarationBaseNode varDecl:
                CompileVariableDeclaration( varDecl );
                break;
            case ClassDeclarationBaseNode classDecl:
                CompileClassDeclaration( classDecl );
                break;
            case ClassInstanceDeclarationBaseNode classInstance:
                CompileClassInstanceDeclaration( classInstance );
                break;
            case StructDeclarationBaseNode structDecl:
                CompileStructDeclaration( structDecl );
                break;
            // DeclarationBaseNode is abstract, all derived types are above

            // Statements (most derived first)
            case BlockStatementBaseNode block:
                CompileBlockStatement( block );
                break;
            case ExpressionStatementBaseNode exprStmt:
                CompileExpressionStatement( exprStmt );
                break;
            case IfStatementBaseNode ifStmt:
                CompileIfStatement( ifStmt );
                break;
            case WhileStatementBaseNode whileStmt:
                CompileWhileStatement( whileStmt );
                break;
            case ForStatementBaseNode forStmt:
                CompileForStatement( forStmt );
                break;
            case ReturnStatementBaseNode returnStmt:
                CompileReturnStatement( returnStmt );
                break;
            case BreakStatementBaseNode breakStmt:
                CompileBreakStatement( breakStmt );
                break;
            case LocalVariableDeclarationBaseNode localVar:
                CompileLocalVariableDeclaration( localVar );
                break;
            case LocalVariableInitializerBaseNode localVarInit:
                CompileLocalVariableInitializer( localVarInit );
                break;
            case UsingStatementBaseNode usingStmt:
                CompileUsingStatement( usingStmt );
                break;
            case SyncBlockNode syncBlock:
                CompileSyncBlock( syncBlock );
                break;
            // StatementBaseNode is abstract, all derived types are above

            // Expressions (most derived first)
            case AssignmentBaseNode assignment:
                CompileAssignment( assignment );
                break;
            case BinaryOperationBaseNode binary:
                CompileBinaryOperation( binary );
                break;
            case TernaryOperationBaseNode ternary:
                CompileTernaryOperation( ternary );
                break;
            case PrimaryBaseNode primary:
                CompilePrimary( primary );
                break;
            case CallBaseNode call:
                CompileCall( call );
                break;
            case UnaryPrefixOperation unaryPrefix:
                CompileUnaryPrefixOperation( unaryPrefix );
                break;
            case UnaryPostfixOperation unaryPostfix:
                CompileUnaryPostfixOperation( unaryPostfix );
                break;
            case ExpressionBaseNode expression:
                CompileExpression( expression );
                break;

            // Other AST nodes
            case InitializerBaseNode initializer:
                CompileInitializer( initializer );
                break;
            case ArgumentsBaseNode arguments:
                CompileArguments( arguments );
                break;
            case ParametersBaseNode parameters:
                CompileParameters( parameters );
                break;
            case ModifiersBaseNode modifiers:
                CompileModifiers( modifiers );
                break;
        }
    }

    /// <summary>
    /// Dispatch method for StatementBaseNode types.
    /// Since DeclarationBaseNode : StatementBaseNode, declarations are also handled here.
    /// </summary>
    private void CompileStatement( StatementBaseNode statement )
    {
        switch ( statement )
        {
            // Declarations (most derived first) - DeclarationBaseNode : StatementBaseNode
            case FunctionDeclarationBaseNode functionDecl:
                CompileFunctionDeclaration( functionDecl );
                break;
            case VariableDeclarationBaseNode varDecl:
                CompileVariableDeclaration( varDecl );
                break;
            case ClassDeclarationBaseNode classDecl:
                CompileClassDeclaration( classDecl );
                break;
            case ClassInstanceDeclarationBaseNode classInstance:
                CompileClassInstanceDeclaration( classInstance );
                break;
            case StructDeclarationBaseNode structDecl:
                CompileStructDeclaration( structDecl );
                break;
            case DeclarationsBaseNode declarations:
                CompileDeclarations( declarations );
                break;
            // DeclarationBaseNode is abstract

            // Statements
            case BlockStatementBaseNode blockStmt:
                CompileBlockStatement( blockStmt );
                break;
            case ExpressionStatementBaseNode exprStmt:
                CompileExpressionStatement( exprStmt );
                break;
            case IfStatementBaseNode ifStmt:
                CompileIfStatement( ifStmt );
                break;
            case WhileStatementBaseNode whileStmt:
                CompileWhileStatement( whileStmt );
                break;
            case ForStatementBaseNode forStmt:
                CompileForStatement( forStmt );
                break;
            case ReturnStatementBaseNode returnStmt:
                CompileReturnStatement( returnStmt );
                break;
            case BreakStatementBaseNode breakStmt:
                CompileBreakStatement( breakStmt );
                break;
            case LocalVariableDeclarationBaseNode localVarDecl:
                CompileLocalVariableDeclaration( localVarDecl );
                break;
            case LocalVariableInitializerBaseNode localVarInit:
                CompileLocalVariableInitializer( localVarInit );
                break;
            case UsingStatementBaseNode usingStmt:
                CompileUsingStatement( usingStmt );
                break;
            case SyncBlockNode syncBlock:
                CompileSyncBlock( syncBlock );
                break;
            // StatementBaseNode is abstract
        }
    }
    
    /// <summary>
    /// Dispatch method for DeclarationBaseNode types.
    /// </summary>
    private void CompileDeclaration( DeclarationBaseNode declaration )
    {
        switch ( declaration )
        {
            case FunctionDeclarationBaseNode functionDecl:
                CompileFunctionDeclaration( functionDecl );
                break;
            case VariableDeclarationBaseNode varDecl:
                CompileVariableDeclaration( varDecl );
                break;
            case ClassDeclarationBaseNode classDecl:
                CompileClassDeclaration( classDecl );
                break;
            case ClassInstanceDeclarationBaseNode classInstance:
                CompileClassInstanceDeclaration( classInstance );
                break;
            case StructDeclarationBaseNode structDecl:
                CompileStructDeclaration( structDecl );
                break;
            case DeclarationsBaseNode declarations:
                CompileDeclarations( declarations );
                break;
            // DeclarationBaseNode is abstract
        }
    }

    /// <summary>
    /// Dispatch method for ExpressionBaseNode types.
    /// Note: AssignmentBaseNode : ExpressionBaseNode, so assignment is the most common expression type.
    /// </summary>
    private void CompileExpression( ExpressionBaseNode expression )
    {
        if ( expression?.AssignmentBase != null )
            CompileAssignment( expression.AssignmentBase );
        else if ( expression is CallBaseNode call )
            CompileCall( call );
        else if ( expression is UnaryPrefixOperation unaryPrefix )
            CompileUnaryPrefixOperation( unaryPrefix );
        else if ( expression is UnaryPostfixOperation unaryPostfix )
            CompileUnaryPostfixOperation( unaryPostfix );
        else if ( expression is BinaryOperationBaseNode binary )
            CompileBinaryOperation( binary );
        else if ( expression is TernaryOperationBaseNode ternary )
            CompileTernaryOperation( ternary );
    }

    #endregion

    #region Compile Methods

    private void CompileProgram( ProgramBaseNode node )
    {
        foreach ( var module in node.ModuleNodes )
        {
            Compile( (AstBaseNode)module );
        }
    }

    private void CompileModule( ModuleBaseNode node )
    {
        if ( node.Statements != null )
        {
            foreach ( var statement in node.Statements )
            {
                CompileStatement( statement );
            }
        }
    }

    private void CompileDeclarations( DeclarationsBaseNode node )
    {
        foreach ( var func in node.Functions )
            Compile( (AstBaseNode)func );
        foreach ( var var in node.Variables )
            Compile( (AstBaseNode)var );
        foreach ( var cls in node.Classes )
            Compile( (AstBaseNode)cls );
        foreach ( var inst in node.ClassInstances )
            Compile( (AstBaseNode)inst );
        foreach ( var str in node.Structs )
            Compile( (AstBaseNode)str );
        if ( node.Statements != null )
            foreach ( var stmt in node.Statements )
                CompileStatement( stmt );
    }

    private void CompileFunctionDeclaration( FunctionDeclarationBaseNode node )
    {
        string prevFunction = m_CurrentFunctionName;
        m_CurrentFunctionName = node.FunctionId?.Id ?? "anonymous";
        
        BiteChunkBuilder prevBuilder = m_Builder;
        m_Builder = new BiteChunkBuilder( m_CurrentFunctionName );
        
        m_Scopes.Add( new LocalScope() );
        
        if ( node.ParametersBase != null && node.ParametersBase.Identifiers != null )
        {
            foreach ( var param in node.ParametersBase.Identifiers )
            {
                int slot = m_Builder.AllocateLocal();
                CurrentScope.Define( param.Id, slot );
            }
        }
        
        if ( node.FunctionBlock != null )
            Compile( (AstBaseNode)node.FunctionBlock );
        
        m_Builder.Ret();
        
        BiteChunk functionChunk = m_Builder.Build();
        m_FunctionChunks.Add( functionChunk );
        
        m_Scopes.RemoveAt( m_Scopes.Count - 1 );
        
        m_Builder = prevBuilder;
        
        int globalSlot = GetOrCreateGlobalSlot( m_CurrentFunctionName );
        m_Builder.ConstI32( m_FunctionChunks.Count - 1 );
        m_Builder.StoreGlobal( ( ushort ) globalSlot );
        
        m_CurrentFunctionName = prevFunction;
    }

    private void CompileVariableDeclaration( VariableDeclarationBaseNode node )
    {
        string name = node.VarId?.Id;
        if ( name == null ) return;

        if ( m_Scopes.Count > 0 )
        {
            int slot = m_Builder.AllocateLocal();
            CurrentScope.Define( name, slot );

            if ( node.InitializerBase != null )
            {
                // Compile the initializer expression
                Compile( (AstBaseNode)node.InitializerBase );
                // Store the result in the local variable
                m_Builder.StoreLocal( ( byte ) slot );
            }
        }
        else
        {
            int slot = GetOrCreateGlobalSlot( name );

            if ( node.InitializerBase != null )
            {
                // Compile the initializer expression
                Compile( (AstBaseNode)node.InitializerBase );
                // Store the result in the global variable
                m_Builder.StoreGlobal( ( ushort ) slot );
            }
        }
    }

    private void CompileClassDeclaration( ClassDeclarationBaseNode node )
    {
        // Not implemented
    }

    private void CompileClassInstanceDeclaration( ClassInstanceDeclarationBaseNode node )
    {
        // Not implemented
    }

    private void CompileBlockStatement( BlockStatementBaseNode node )
    {
        m_Scopes.Add( new LocalScope() );

        if ( node.DeclarationsBase != null )
            Compile( (AstBaseNode)node.DeclarationsBase );

        m_Scopes.RemoveAt( m_Scopes.Count - 1 );
    }

    private void CompileExpressionStatement( ExpressionStatementBaseNode node )
    {
        if ( node.ExpressionBase != null )
            CompileExpression( node.ExpressionBase );
    }

    private void CompileIfStatement( IfStatementBaseNode node )
    {
        if ( node.ExpressionBase != null )
            CompileExpression( node.ExpressionBase );
        
        var elseJump = m_Builder.JumpIfFalse();
        
        if ( node.ThenStatementBase != null )
            CompileStatement( node.ThenStatementBase );
        
        if ( node.ElseStatementBase != null )
        {
            var endJump = m_Builder.Jump();
            elseJump.Patch();
            CompileStatement( node.ElseStatementBase );
            endJump.Patch();
        }
        else
        {
            elseJump.Patch();
        }
    }

    private void CompileWhileStatement( WhileStatementBaseNode node )
    {
        int loopStart = m_Builder.CurrentOffset;
        
        if ( node.ExpressionBase != null )
            CompileExpression( node.ExpressionBase );
        
        var exitJump = m_Builder.JumpIfFalse();
        
        if ( node.WhileBlock != null )
            Compile( (AstBaseNode)node.WhileBlock );
        
        m_Builder.Loop( loopStart );
        exitJump.Patch();
    }

    private void CompileForStatement( ForStatementBaseNode node )
    {
        m_Scopes.Add( new LocalScope() );
        
        if ( node.InitializerBase != null )
        {
            if ( node.InitializerBase.LocalVariableInitializerBase != null )
                Compile( (AstBaseNode)node.InitializerBase.LocalVariableInitializerBase );
            else if ( node.InitializerBase.Expressions != null )
            {
                foreach ( var expr in node.InitializerBase.Expressions )
                {
                    if ( expr != null )
                    {
                        CompileExpression( expr );
                        m_Builder.Pop();
                    }
                }
            }
        }
        
        int loopStart = m_Builder.CurrentOffset;
        
        if ( node.Condition != null )
            CompileExpression( node.Condition );
        else
            m_Builder.ConstTrue();
        
        var exitJump = m_Builder.JumpIfFalse();
        
        if ( node.StatementBase != null )
            CompileStatement( node.StatementBase );
        
        if ( node.Iterators != null )
        {
            foreach ( var iter in node.Iterators )
            {
                if ( iter != null )
                {
                    CompileExpression( iter );
                    m_Builder.Pop();
                }
            }
        }
        
        m_Builder.Loop( loopStart );
        exitJump.Patch();
        
        m_Scopes.RemoveAt( m_Scopes.Count - 1 );
    }

    private void CompileReturnStatement( ReturnStatementBaseNode node )
    {
        if ( node.ExpressionStatementBase != null && node.ExpressionStatementBase.ExpressionBase != null )
        {
            CompileExpression( node.ExpressionStatementBase.ExpressionBase );
            m_Builder.RetVal();
        }
        else
        {
            m_Builder.Ret();
        }
    }

    private void CompileBreakStatement( BreakStatementBaseNode node )
    {
        // Not implemented
    }

    private void CompileLocalVariableDeclaration( LocalVariableDeclarationBaseNode node )
    {
        string name = node.VarId?.Id;
        if ( name == null ) return;
        
        int slot = m_Builder.AllocateLocal();
        CurrentScope.Define( name, slot );
        
        if ( node.ExpressionBase != null )
        {
            CompileExpression( node.ExpressionBase );
            m_Builder.StoreLocal( ( byte ) slot );
        }
    }

    private void CompileLocalVariableInitializer( LocalVariableInitializerBaseNode node )
    {
        // Not implemented
    }

    private void CompileSyncBlock( SyncBlockNode node )
    {
        // Not implemented
    }

    /// <summary>
    /// Compile an assignment node.
    /// This handles assignment operators (+=, -=, etc.), and also delegates to
    /// Binary, Ternary, Primary, Call, and Unary operations based on the assignment type.
    /// </summary>
    private void CompileAssignment( AssignmentBaseNode node )
    {
        // The assignment target can be in PrimaryBaseNode (direct identifier)
        // or in CallBase.PrimaryBase (identifier wrapped in call node)
        string targetName = null;
        if ( node.PrimaryBaseNode != null && node.PrimaryBaseNode.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
        {
            targetName = node.PrimaryBaseNode.PrimaryId?.Id;
        }
        else if ( node.CallBase != null && node.CallBase.PrimaryBase != null && 
                  node.CallBase.PrimaryBase.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
        {
            targetName = node.CallBase.PrimaryBase.PrimaryId?.Id;
        }
        
        // If we have a target name, this is an assignment to a variable
        if ( targetName != null )
        {
            // Check if this is actually an assignment (has a value to assign)
            // or just an identifier expression like "x;" which has AssignmentBase
            // but no actual value to assign
            bool hasValue = node.AssignmentBase != null || 
                            node.Binary != null || 
                            node.Ternary != null || 
                            node.UnaryPrefix != null || 
                            node.UnaryPostfix != null;
            
            if ( !hasValue )
            {
                // This is just an identifier expression, not an assignment
                // Load the value of the variable
                if ( node.CallBase != null )
                    CompileCall( node.CallBase );
                return;
            }
            
            // Compile the right-hand side value
            // The value can be in: AssignmentBase (for x = y), Binary (for x = y + z),
            // Ternary, UnaryPrefix, UnaryPostfix, or just a CallBase (for simple value)
            if ( node.AssignmentBase != null )
            {
                // For x = y, the RHS is in AssignmentBase and we need to load its value.
                // The RHS AssignmentBase has CallBase containing 'y', but AssignmentBase itself
                // is not a complete assignment expression - we should just compile the CallBase
                // to load the value of 'y'.
                // 
                // Check if the RHS is a simple value (identifier, literal) or a complex expression.
                // A simple value has only CallBase set and no other expression fields.
                // We also need to check that the CallBase doesn't have arguments (not a function call)
                // and the PrimaryBase is a simple identifier or literal.
                bool isSimpleValue = node.AssignmentBase.CallBase != null && 
                     node.AssignmentBase.AssignmentBase == null &&
                     node.AssignmentBase.Binary == null &&
                     node.AssignmentBase.Ternary == null &&
                     node.AssignmentBase.UnaryPrefix == null &&
                     node.AssignmentBase.UnaryPostfix == null &&
                     node.AssignmentBase.CallBase.ArgumentsBase == null &&
                     node.AssignmentBase.CallBase.CallEntries == null &&
                     node.AssignmentBase.CallBase.ElementAccess == null;
                
                if ( isSimpleValue )
                {
                    // Simple case: x = y or x = 10, just load the value
                    CompileCall( node.AssignmentBase.CallBase );
                }
                else
                {
                    // Complex case: x = (some expression), compile the RHS assignment
                    CompileAssignment( node.AssignmentBase );
                }
            }
            else if ( node.Binary != null )
                CompileExpression( node.Binary );
            else if ( node.Ternary != null )
                CompileExpression( node.Ternary );
            else if ( node.UnaryPrefix != null )
                CompileExpression( node.UnaryPrefix );
            else if ( node.UnaryPostfix != null )
                CompileExpression( node.UnaryPostfix );
            
            // Store the value and load it back (for expression value)
            int slot = ResolveVariable( targetName );
            if ( slot >= 0 )  // Local
            {
                m_Builder.StoreLocal( ( byte ) slot );
                m_Builder.LoadLocal( ( byte ) slot );
            }
            else  // Global (-2) or not found (-1)
            {
                int globalSlot = GetOrCreateGlobalSlot( targetName );
                m_Builder.StoreGlobal( ( ushort ) globalSlot );
                m_Builder.LoadGlobal( ( ushort ) globalSlot );
            }
        }
        else if ( node.PrimaryBaseNode != null )
        {
            // Non-identifier primary (literal, expression, etc.)
            CompilePrimary( node.PrimaryBaseNode );
        }
        else if ( node.CallBase != null )
        {
            // Call expression without simple identifier target (e.g., array[index], method call)
            CompileCall( node.CallBase );
        }
        else if ( node.Binary != null )
        {
            CompileExpression( node.Binary );
        }
        else if ( node.Ternary != null )
        {
            CompileExpression( node.Ternary );
        }
        else if ( node.UnaryPrefix != null )
        {
            CompileExpression( node.UnaryPrefix );
        }
        else if ( node.UnaryPostfix != null )
        {
            CompileExpression( node.UnaryPostfix );
        }
    }

    /// <summary>
    /// Compile a binary operation (e.g., a + b, a == b).
    /// </summary>
    private void CompileBinaryOperation( BinaryOperationBaseNode node )
    {
        if ( node.LeftOperand != null )
            CompileExpression( node.LeftOperand );
        
        if ( node.RightOperand != null )
            CompileExpression( node.RightOperand );
        
        switch ( node.Operator )
        {
            case BinaryOperationBaseNode.BinaryOperatorType.Plus: m_Builder.Add(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.Minus: m_Builder.Sub(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.Mult: m_Builder.Mul(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.Div: m_Builder.Div(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.Modulo: m_Builder.Mod(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.Equal: m_Builder.Eq(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.NotEqual: m_Builder.Neq(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.Less: m_Builder.Lt(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.LessOrEqual: m_Builder.Le(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.Greater: m_Builder.Gt(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.GreaterOrEqual: m_Builder.Ge(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.And: m_Builder.And(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.Or: m_Builder.Or(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseAnd: m_Builder.BitAnd(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseOr: m_Builder.BitOr(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseXor: m_Builder.BitXor(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.ShiftLeft: m_Builder.LShift(); break;
            case BinaryOperationBaseNode.BinaryOperatorType.ShiftRight: m_Builder.RShift(); break;
        }
    }

    /// <summary>
    /// Compile a ternary operation (condition ? trueExpr : falseExpr).
    /// </summary>
    private void CompileTernaryOperation( TernaryOperationBaseNode node )
    {
        if ( node.LeftOperand != null )
            CompileExpression( node.LeftOperand );
        
        var elseJump = m_Builder.JumpIfFalse();
        
        if ( node.MidOperand != null )
            CompileExpression( node.MidOperand );
        
        var endJump = m_Builder.Jump();
        
        elseJump.Patch();
        
        if ( node.RightOperand != null )
            CompileExpression( node.RightOperand );
        
        endJump.Patch();
    }

    /// <summary>
    /// Compile a primary expression (literals, identifiers, parenthesized expressions).
    /// </summary>
    private void CompilePrimary( PrimaryBaseNode node )
    {
        switch ( node.PrimaryType )
        {
            case PrimaryBaseNode.PrimaryTypes.IntegerLiteral:
                if ( node.IntegerLiteral.HasValue )
                {
                    long val = node.IntegerLiteral.Value;
                    if ( val >= sbyte.MinValue && val <= sbyte.MaxValue )
                        m_Builder.ConstI8( ( sbyte ) val );
                    else if ( val >= int.MinValue && val <= int.MaxValue )
                        m_Builder.ConstI32( ( int ) val );
                    else
                        m_Builder.ConstF64( (double)val );
                }
                break;

            case PrimaryBaseNode.PrimaryTypes.FloatLiteral:
                if ( node.FloatLiteral.HasValue )
                    m_Builder.ConstF64( node.FloatLiteral.Value );
                break;

            case PrimaryBaseNode.PrimaryTypes.BooleanLiteral:
                if ( node.BooleanLiteral.HasValue )
                {
                    if ( node.BooleanLiteral.Value )
                        m_Builder.ConstTrue();
                    else
                        m_Builder.ConstFalse();
                }
                break;

            case PrimaryBaseNode.PrimaryTypes.StringLiteral:
                if ( node.StringLiteral != null )
                    m_Builder.ConstStr( node.StringLiteral );
                break;

            case PrimaryBaseNode.PrimaryTypes.Identifier:
                if ( node.PrimaryId != null )
                {
                    string name = node.PrimaryId.Id;
                    int slot = ResolveVariable( name );
                    if ( slot >= 0 )  // Local
                        m_Builder.LoadLocal( ( byte ) slot );
                    else  // Global (-2) or not found (-1)
                    {
                        int globalSlot = GetOrCreateGlobalSlot( name );
                        m_Builder.LoadGlobal( ( ushort ) globalSlot );
                    }
                }
                break;

            case PrimaryBaseNode.PrimaryTypes.NullReference:
                m_Builder.ConstNull();
                break;

            case PrimaryBaseNode.PrimaryTypes.Expression:
                if ( node.Expression != null )
                    Compile( node.Expression );
                break;
        }
    }

    /// <summary>
    /// Compile a unary prefix operation (e.g., -x, !x, ~x).
    /// </summary>
    private void CompileUnaryPrefixOperation( UnaryPrefixOperation node )
    {
        if ( node.Primary != null )
            CompileExpression( node.Primary );
        
        switch ( node.Operator )
        {
            case UnaryPrefixOperation.UnaryPrefixOperatorType.Negate: m_Builder.Neg(); break;
            case UnaryPrefixOperation.UnaryPrefixOperatorType.LogicalNot: m_Builder.Not(); break;
            case UnaryPrefixOperation.UnaryPrefixOperatorType.Compliment: m_Builder.BitNot(); break;
        }
    }

    /// <summary>
    /// Compile a unary postfix operation (e.g., x++, x--).
    /// </summary>
    private void CompileUnaryPostfixOperation( UnaryPostfixOperation node )
    {
        if ( node.Primary != null )
            CompileExpression( node.Primary );
    }

    private void CompileCall( CallBaseNode node )
    {
        // Handle simple primary expressions (literals, identifiers) wrapped in CallBase
        if ( node.CallType == CallTypes.Primary && node.PrimaryBase != null )
        {
            CompilePrimary( node.PrimaryBase );
        }
        // TODO: Handle function calls (CallTypes.PrimaryCall)
    }

    private void CompileInitializer( InitializerBaseNode node )
    {
        if ( node.Expression != null )
            CompileStatement( node.Expression );
    }

    private void CompileArguments( ArgumentsBaseNode node )
    {
        // Not implemented
    }

    private void CompileParameters( ParametersBaseNode node )
    {
        // Not implemented
    }

    private void CompileUsingStatement( UsingStatementBaseNode node )
    {
        // Not implemented
    }

    private void CompileModifiers( ModifiersBaseNode node )
    {
        // Not implemented
    }

    private void CompileStructDeclaration( StructDeclarationBaseNode node )
    {
        // Not implemented
    }

    #endregion

    #region Visitor Methods (Interface Implementation)

    public override object Visit( ProgramBaseNode node )
    {
        CompileProgram( node );
        return null;
    }

    public override object Visit( ModuleBaseNode node )
    {
        CompileModule( node );
        return null;
    }

    public override object Visit( DeclarationsBaseNode node )
    {
        CompileDeclarations( node );
        return null;
    }

    public override object Visit( DeclarationBaseNode node )
    {
        CompileDeclaration( node );
        return null;
    }

    public override object Visit( FunctionDeclarationBaseNode node )
    {
        CompileFunctionDeclaration( node );
        return null;
    }

    public override object Visit( VariableDeclarationBaseNode node )
    {
        CompileVariableDeclaration( node );
        return null;
    }

    public override object Visit( ClassDeclarationBaseNode node )
    {
        CompileClassDeclaration( node );
        return null;
    }

    public override object Visit( ClassInstanceDeclarationBaseNode node )
    {
        CompileClassInstanceDeclaration( node );
        return null;
    }

    public override object Visit( StatementBaseNode node )
    {
        CompileStatement( node );
        return null;
    }

    public override object Visit( BlockStatementBaseNode node )
    {
        CompileBlockStatement( node );
        return null;
    }

    public override object Visit( ExpressionStatementBaseNode node )
    {
        CompileExpressionStatement( node );
        return null;
    }

    public override object Visit( IfStatementBaseNode node )
    {
        CompileIfStatement( node );
        return null;
    }

    public override object Visit( WhileStatementBaseNode node )
    {
        CompileWhileStatement( node );
        return null;
    }

    public override object Visit( ForStatementBaseNode node )
    {
        CompileForStatement( node );
        return null;
    }

    public override object Visit( ReturnStatementBaseNode node )
    {
        CompileReturnStatement( node );
        return null;
    }

    public override object Visit( BreakStatementBaseNode node )
    {
        CompileBreakStatement( node );
        return null;
    }

    public override object Visit( LocalVariableDeclarationBaseNode node )
    {
        CompileLocalVariableDeclaration( node );
        return null;
    }

    public override object Visit( LocalVariableInitializerBaseNode node )
    {
        CompileLocalVariableInitializer( node );
        return null;
    }

    public override object Visit( SyncBlockNode node )
    {
        CompileSyncBlock( node );
        return null;
    }

    public override object Visit( ExpressionBaseNode node )
    {
        CompileExpression( node );
        return null;
    }

    public override object Visit( AssignmentBaseNode node )
    {
        CompileAssignment( node );
        return null;
    }

    public override object Visit( BinaryOperationBaseNode node )
    {
        CompileBinaryOperation( node );
        return null;
    }

    public override object Visit( TernaryOperationBaseNode node )
    {
        CompileTernaryOperation( node );
        return null;
    }

    public override object Visit( PrimaryBaseNode node )
    {
        CompilePrimary( node );
        return null;
    }

    public override object Visit( UnaryPrefixOperation node )
    {
        CompileUnaryPrefixOperation( node );
        return null;
    }

    public override object Visit( UnaryPostfixOperation node )
    {
        CompileUnaryPostfixOperation( node );
        return null;
    }

    public override object Visit( CallBaseNode node )
    {
        CompileCall( node );
        return null;
    }

    public override object Visit( InitializerBaseNode node )
    {
        CompileInitializer( node );
        return null;
    }

    public override object Visit( ArgumentsBaseNode node )
    {
        CompileArguments( node );
        return null;
    }

    public override object Visit( ParametersBaseNode node )
    {
        CompileParameters( node );
        return null;
    }

    public override object Visit( UsingStatementBaseNode node )
    {
        CompileUsingStatement( node );
        return null;
    }

    public override object Visit( ModifiersBaseNode node )
    {
        CompileModifiers( node );
        return null;
    }

    public override object Visit( StructDeclarationBaseNode node )
    {
        CompileStructDeclaration( node );
        return null;
    }

    public override object Visit( AstBaseNode baseNode )
    {
        Compile( baseNode );
        return null;
    }

    #endregion

    #region Private Helpers

    private int ResolveVariable( string name )
    {
        // Search local scopes first (innermost to outermost)
        for ( int i = m_Scopes.Count - 1; i >= 0; i-- )
        {
            if ( m_Scopes[i].Locals.TryGetValue( name, out int slot ) )
            {
                return slot;
            }
        }
        
        // If not found in locals, check if it's a global
        if ( m_GlobalSlots.TryGetValue( name, out int globalSlot ) )
        {
            return -2; // Special marker for global
        }
        
        return -1; // Not found
    }

    private int GetOrCreateGlobalSlot( string name )
    {
        if ( !m_GlobalSlots.TryGetValue( name, out int slot ) )
        {
            slot = m_NextGlobalSlot++;
            m_GlobalSlots[name] = slot;
        }

        return slot;
    }

    #endregion
}

public class BiteProgram
{
    public BiteChunk MainChunk { get; }
    public List < BiteChunk > Chunks { get; }

    public BiteProgram( BiteChunk mainChunk, List < BiteChunk > chunks )
    {
        MainChunk = mainChunk;
        Chunks = new List<BiteChunk>(chunks);
        // Always include main chunk as first chunk
        if (MainChunk != null && (Chunks.Count == 0 || Chunks[0] != MainChunk))
            Chunks.Insert(0, MainChunk);
    }
}

}
