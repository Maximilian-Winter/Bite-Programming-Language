#region

using System.Collections.Generic;
using Bite.Ast;
using Bite.Runtime;

#endregion

namespace Bite.NewCodeGen
{

public class BiteCodeGenerator : AstVisitor < object >
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
        
        Visit( module );
        
        m_Builder.Ret();
        
        BiteChunk mainChunk = m_Builder.Build();
        
        return new BiteProgram( mainChunk, m_FunctionChunks );
    }

    #endregion

    #region Visitor Methods

    public override object Visit( ProgramBaseNode node )
    {
        foreach ( var module in node.ModuleNodes )
        {
            Visit( module );
        }

        return null;
    }

    public override object Visit( ModuleBaseNode node )
    {
        if ( node.Statements != null )
        {
            foreach ( var statement in node.Statements )
            {
                Visit( statement );
            }
        }

        return null;
    }

    public override object Visit( DeclarationsBaseNode node )
    {
        // Visit functions
        foreach ( var func in node.Functions )
        {
            Visit( func );
        }

        // Visit variables
        foreach ( var var in node.Variables )
        {
            Visit( var );
        }

        // Visit classes
        foreach ( var cls in node.Classes )
        {
            Visit( cls );
        }

        // Visit class instances
        foreach ( var inst in node.ClassInstances )
        {
            Visit( inst );
        }

        // Visit structs
        foreach ( var str in node.Structs )
        {
            Visit( str );
        }

        // Visit statements
        if ( node.Statements != null )
        {
            foreach ( var stmt in node.Statements )
            {
                Visit( stmt );
            }
        }

        return null;
    }

    public override object Visit( DeclarationBaseNode node )
    {
        // DeclarationBaseNode is abstract - this shouldn't be called directly
        // The actual types are handled by the visitor pattern
        return null;
    }

    public override object Visit( FunctionDeclarationBaseNode node )
    {
        string prevFunction = m_CurrentFunctionName;
        m_CurrentFunctionName = node.FunctionId?.Id ?? "anonymous";
        
        BiteChunkBuilder prevBuilder = m_Builder;
        m_Builder = new BiteChunkBuilder( m_CurrentFunctionName );
        
        m_Scopes.Add( new LocalScope() );
        
        // Compile parameters as locals
        if ( node.ParametersBase != null && node.ParametersBase.Identifiers != null )
        {
            foreach ( var param in node.ParametersBase.Identifiers )
            {
                int slot = m_Builder.AllocateLocal();
                CurrentScope.Define( param.Id, slot );
            }
        }
        
        // Compile function body
        if ( node.FunctionBlock != null )
        {
            Visit( node.FunctionBlock );
        }
        
        // Ensure function ends with return
        m_Builder.Ret();
        
        BiteChunk functionChunk = m_Builder.Build();
        m_FunctionChunks.Add( functionChunk );
        
        m_Scopes.RemoveAt( m_Scopes.Count - 1 );
        
        m_Builder = prevBuilder;
        m_CurrentFunctionName = prevFunction;
        
        // Store function reference in global
        int globalSlot = GetOrCreateGlobalSlot( m_CurrentFunctionName );
        m_Builder.ConstI32( m_FunctionChunks.Count - 1 );
        m_Builder.StoreGlobal( ( ushort ) globalSlot );

        return null;
    }

    public override object Visit( VariableDeclarationBaseNode node )
    {
        string name = node.VarId?.Id;
        if ( name == null ) return null;

        if ( m_Scopes.Count > 0 )
        {
            // Local variable
            int slot = m_Builder.AllocateLocal();
            CurrentScope.Define( name, slot );

            if ( node.InitializerBase != null )
            {
                Visit( node.InitializerBase );
                m_Builder.StoreLocal( ( byte ) slot );
            }
        }
        else
        {
            // Global variable
            int slot = GetOrCreateGlobalSlot( name );

            if ( node.InitializerBase != null )
            {
                Visit( node.InitializerBase );
                m_Builder.StoreGlobal( ( ushort ) slot );
            }
        }

        return null;
    }

    public override object Visit( ClassDeclarationBaseNode node )
    {
        // TODO: Implement class declaration
        return null;
    }

    public override object Visit( ClassInstanceDeclarationBaseNode node )
    {
        // TODO: Implement class instance declaration
        return null;
    }

    public override object Visit( StatementBaseNode node )
    {
        // StatementBaseNode is abstract - handle specific types
        // This shouldn't be called directly as visitors for derived types should be called
        return null;
    }

    public override object Visit( BlockStatementBaseNode node )
    {
        m_Scopes.Add( new LocalScope() );

        if ( node.DeclarationsBase != null )
        {
            Visit( node.DeclarationsBase );
        }

        m_Scopes.RemoveAt( m_Scopes.Count - 1 );

        return null;
    }

    public override object Visit( ExpressionStatementBaseNode node )
    {
        if ( node.ExpressionBase != null )
        {
            Visit( node.ExpressionBase );
            m_Builder.Pop();
        }

        return null;
    }

    public override object Visit( IfStatementBaseNode node )
    {
        // Compile condition
        if ( node.ExpressionBase != null )
        {
            Visit( node.ExpressionBase );
        }
        
        // Jump to else if condition is false
        var elseJump = m_Builder.JumpIfFalse();
        
        // Compile then branch
        if ( node.ThenStatementBase != null )
        {
            Visit( node.ThenStatementBase );
        }
        
        if ( node.ElseStatementBase != null )
        {
            // Jump over else after then
            var endJump = m_Builder.Jump();
            
            // Patch else jump target
            elseJump.Patch();
            
            // Compile else branch
            Visit( node.ElseStatementBase );
            
            // Patch end jump
            endJump.Patch();
        }
        else
        {
            // Patch else jump target (no else branch)
            elseJump.Patch();
        }

        return null;
    }

    public override object Visit( WhileStatementBaseNode node )
    {
        int loopStart = m_Builder.CurrentOffset;
        
        // Compile condition
        if ( node.ExpressionBase != null )
        {
            Visit( node.ExpressionBase );
        }
        
        // Jump out if condition is false
        var exitJump = m_Builder.JumpIfFalse();
        
        // Compile body
        if ( node.WhileBlock != null )
        {
            Visit( node.WhileBlock );
        }
        
        // Loop back
        m_Builder.Loop( loopStart );
        
        // Patch exit jump
        exitJump.Patch();

        return null;
    }

    public override object Visit( ForStatementBaseNode node )
    {
        m_Scopes.Add( new LocalScope() );
        
        // Initialize
        if ( node.InitializerBase != null )
        {
            if ( node.InitializerBase.LocalVariableInitializerBase != null )
            {
                Visit( node.InitializerBase.LocalVariableInitializerBase );
            }
            else if ( node.InitializerBase.Expressions != null )
            {
                foreach ( var expr in node.InitializerBase.Expressions )
                {
                    if ( expr != null )
                    {
                        Visit( expr );
                        m_Builder.Pop();
                    }
                }
            }
        }
        
        int loopStart = m_Builder.CurrentOffset;
        
        // Condition
        if ( node.Condition != null )
        {
            Visit( node.Condition );
        }
        else
        {
            m_Builder.ConstTrue();
        }
        
        var exitJump = m_Builder.JumpIfFalse();
        
        // Body
        if ( node.StatementBase != null )
        {
            Visit( node.StatementBase );
        }
        
        // Increment
        if ( node.Iterators != null )
        {
            foreach ( var iter in node.Iterators )
            {
                if ( iter != null )
                {
                    Visit( iter );
                    m_Builder.Pop();
                }
            }
        }
        
        // Loop back
        m_Builder.Loop( loopStart );
        
        exitJump.Patch();
        
        m_Scopes.RemoveAt( m_Scopes.Count - 1 );

        return null;
    }

    public override object Visit( ReturnStatementBaseNode node )
    {
        if ( node.ExpressionStatementBase != null && node.ExpressionStatementBase.ExpressionBase != null )
        {
            Visit( node.ExpressionStatementBase.ExpressionBase );
            m_Builder.RetVal();
        }
        else
        {
            m_Builder.Ret();
        }

        return null;
    }

    public override object Visit( BreakStatementBaseNode node )
    {
        // TODO: Implement break with proper loop tracking
        return null;
    }

    public override object Visit( LocalVariableDeclarationBaseNode node )
    {
        // Handle local variable declarations inside for loops, etc.
        if ( node.ExpressionBase != null )
        {
            Visit( node.ExpressionBase );
        }

        return null;
    }

    public override object Visit( LocalVariableInitializerBaseNode node )
    {
        // TODO: Implement local variable initializer
        return null;
    }

    public override object Visit( SyncBlockNode node )
    {
        // TODO: Implement sync block for Unity context
        return null;
    }

    public override object Visit( ExpressionBaseNode node )
    {
        if ( node.AssignmentBase != null )
        {
            Visit( node.AssignmentBase );
        }

        return null;
    }

    public override object Visit( AssignmentBaseNode node )
    {
        // Handle assignment to primary (variable)
        if ( node.PrimaryBaseNode != null && node.PrimaryBaseNode.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
        {
            string name = node.PrimaryBaseNode.PrimaryId?.Id;
            if ( name == null ) return null;
            
            // Compile the right-hand side
            if ( node.Binary != null )
            {
                Visit( node.Binary );
            }
            else if ( node.Ternary != null )
            {
                Visit( node.Ternary );
            }
            else if ( node.UnaryPrefix != null )
            {
                Visit( node.UnaryPrefix );
            }
            else if ( node.UnaryPostfix != null )
            {
                Visit( node.UnaryPostfix );
            }
            
            // Store to variable
            int slot = ResolveVariable( name );
            if ( slot >= 0 )
            {
                m_Builder.StoreLocal( ( byte ) slot );
            }
            else
            {
                int globalSlot = GetOrCreateGlobalSlot( name );
                m_Builder.StoreGlobal( ( ushort ) globalSlot );
            }
            
            // Leave value on stack
            if ( slot >= 0 )
            {
                m_Builder.LoadLocal( ( byte ) slot );
            }
            else
            {
                m_Builder.LoadGlobal( ( ushort ) GetOrCreateGlobalSlot( name ) );
            }
        }

        return null;
    }

    public override object Visit( BinaryOperationBaseNode node )
    {
        // Compile left operand
        if ( node.LeftOperand != null )
        {
            Visit( node.LeftOperand );
        }
        
        // Compile right operand
        if ( node.RightOperand != null )
        {
            Visit( node.RightOperand );
        }
        
        // Emit operator
        switch ( node.Operator )
        {
            case BinaryOperationBaseNode.BinaryOperatorType.Plus:
                m_Builder.Add();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.Minus:
                m_Builder.Sub();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.Mult:
                m_Builder.Mul();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.Div:
                m_Builder.Div();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.Modulo:
                m_Builder.Mod();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.Equal:
                m_Builder.Eq();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.NotEqual:
                m_Builder.Neq();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.Less:
                m_Builder.Lt();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.LessOrEqual:
                m_Builder.Le();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.Greater:
                m_Builder.Gt();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.GreaterOrEqual:
                m_Builder.Ge();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.And:
                m_Builder.And();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.Or:
                m_Builder.Or();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseAnd:
                m_Builder.BitAnd();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseOr:
                m_Builder.BitOr();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseXor:
                m_Builder.BitXor();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.ShiftLeft:
                m_Builder.LShift();
                break;
            case BinaryOperationBaseNode.BinaryOperatorType.ShiftRight:
                m_Builder.RShift();
                break;
        }

        return null;
    }

    public override object Visit( TernaryOperationBaseNode node )
    {
        // Compile condition (left operand)
        if ( node.LeftOperand != null )
        {
            Visit( node.LeftOperand );
        }
        
        var elseJump = m_Builder.JumpIfFalse();
        
        // Compile true expression (mid operand)
        if ( node.MidOperand != null )
        {
            Visit( node.MidOperand );
        }
        
        var endJump = m_Builder.Jump();
        
        elseJump.Patch();
        
        // Compile false expression (right operand)
        if ( node.RightOperand != null )
        {
            Visit( node.RightOperand );
        }
        
        endJump.Patch();

        return null;
    }

    public override object Visit( PrimaryBaseNode node )
    {
        switch ( node.PrimaryType )
        {
            case PrimaryBaseNode.PrimaryTypes.IntegerLiteral:
                if ( node.IntegerLiteral.HasValue )
                {
                    long val = node.IntegerLiteral.Value;
                    if ( val >= sbyte.MinValue && val <= sbyte.MaxValue )
                    {
                        m_Builder.ConstI8( ( sbyte ) val );
                    }
                    else if ( val >= int.MinValue && val <= int.MaxValue )
                    {
                        m_Builder.ConstI32( ( int ) val );
                    }
                    else
                    {
                        m_Builder.ConstF64( (double)val );
                    }
                }
                break;

            case PrimaryBaseNode.PrimaryTypes.FloatLiteral:
                if ( node.FloatLiteral.HasValue )
                {
                    m_Builder.ConstF64( node.FloatLiteral.Value );
                }
                break;

            case PrimaryBaseNode.PrimaryTypes.BooleanLiteral:
                if ( node.BooleanLiteral.HasValue )
                {
                    if ( node.BooleanLiteral.Value )
                    {
                        m_Builder.ConstTrue();
                    }
                    else
                    {
                        m_Builder.ConstFalse();
                    }
                }
                break;

            case PrimaryBaseNode.PrimaryTypes.StringLiteral:
                if ( node.StringLiteral != null )
                {
                    m_Builder.ConstStr( node.StringLiteral );
                }
                break;

            case PrimaryBaseNode.PrimaryTypes.Identifier:
                if ( node.PrimaryId != null )
                {
                    string name = node.PrimaryId.Id;
                    int slot = ResolveVariable( name );
                    if ( slot >= 0 )
                    {
                        m_Builder.LoadLocal( ( byte ) slot );
                    }
                    else
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
                {
                    Visit( node.Expression );
                }
                break;

            case PrimaryBaseNode.PrimaryTypes.ArrayExpression:
                // TODO: Implement array literal
                break;

            case PrimaryBaseNode.PrimaryTypes.DictionaryExpression:
                // TODO: Implement dictionary literal
                break;
        }

        return null;
    }

    public override object Visit( UnaryPrefixOperation node )
    {
        // Compile operand
        if ( node.Primary != null )
        {
            Visit( node.Primary );
        }
        
        switch ( node.Operator )
        {
            case UnaryPrefixOperation.UnaryPrefixOperatorType.Negate:
                m_Builder.Neg();
                break;
            case UnaryPrefixOperation.UnaryPrefixOperatorType.LogicalNot:
                m_Builder.Not();
                break;
            case UnaryPrefixOperation.UnaryPrefixOperatorType.Compliment:
                m_Builder.BitNot();
                break;
            case UnaryPrefixOperation.UnaryPrefixOperatorType.PlusPlus:
                // TODO: Implement pre-increment
                break;
            case UnaryPrefixOperation.UnaryPrefixOperatorType.MinusMinus:
                // TODO: Implement pre-decrement
                break;
        }

        return null;
    }

    public override object Visit( UnaryPostfixOperation node )
    {
        // Compile operand
        if ( node.Primary != null )
        {
            Visit( node.Primary );
        }
        
        // TODO: Implement postfix operations (post-increment, post-decrement)
        
        return null;
    }

    public override object Visit( CallBaseNode node )
    {
        // TODO: Implement function/method calls
        return null;
    }

    public override object Visit( InitializerBaseNode node )
    {
        if ( node.Expression != null )
        {
            Visit( node.Expression );
        }

        return null;
    }

    public override object Visit( ArgumentsBaseNode node )
    {
        // TODO: Implement argument compilation
        return null;
    }

    public override object Visit( ParametersBaseNode node )
    {
        // Parameters are handled in function declaration
        return null;
    }

    public override object Visit( UsingStatementBaseNode node )
    {
        // Using statements are compile-time only
        return null;
    }

    public override object Visit( ModifiersBaseNode node )
    {
        // Modifiers are handled during semantic analysis
        return null;
    }

    public override object Visit( StructDeclarationBaseNode node )
    {
        // TODO: Implement struct declaration
        return null;
    }

    public override object Visit( AstBaseNode baseNode )
    {
        // Base case - do nothing
        return null;
    }

    #endregion

    #region Private Helpers

    private int ResolveVariable( string name )
    {
        // Search from innermost scope outward
        for ( int i = m_Scopes.Count - 1; i >= 0; i-- )
        {
            if ( m_Scopes[i].Locals.TryGetValue( name, out int slot ) )
            {
                return slot;
            }
        }

        return -1; // Not found in locals - must be global
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
        Chunks = chunks;
    }
}

}
