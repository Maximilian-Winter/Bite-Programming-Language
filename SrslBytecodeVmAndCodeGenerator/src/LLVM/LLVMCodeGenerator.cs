using System;
using System.Collections.Generic;
using LLVMSharp;
using MemoizeSharp;

namespace SrslInterpreterBacktrackingMemoizingParserBased.LLVMCompiler
{

public class LLVMCodeGenerator: HeteroAstVisitor < object >, IAstVisitor
{
    LLVMModuleRef module = LLVM.ModuleCreateWithName("MyProg");
    LLVMBuilderRef builder = LLVM.CreateBuilder();
    
    private static readonly LLVMBool LLVMBoolFalse = new LLVMBool(0);
    private static readonly LLVMBool LLVMBoolTrue = new LLVMBool(1);

    private static readonly LLVMValueRef NullValue = new LLVMValueRef(IntPtr.Zero);

    private readonly Dictionary<string, LLVMValueRef> NamedValues = new Dictionary<string, LLVMValueRef>();

    private readonly Stack<LLVMValueRef> ValueStack = new Stack<LLVMValueRef>();
    
    public Stack<LLVMValueRef> ResultStack { get { return ValueStack; } }
    
    public void ClearResultStack()
    {
        ValueStack.Clear();
    }

    private object Compile( HeteroAstNode astNode )
    {
        return astNode.Accept( this );
    }
    
    public override object Visit( ProgramNode node )
    {
        foreach ( KeyValuePair < string, ModuleNode > module in node.ModuleNodes )
        {
            //ModuleSymbol moduleSymbol = m_SymbolTableBuilder.CurrentScope.resolve( module.Key ) as ModuleSymbol;
            
            if ( module.Key != node.MainModule )
            {
                Compile( module.Value );
            }
        }

        if ( node.ModuleNodes.ContainsKey( node.MainModule ) )
        {
            Compile( node.ModuleNodes[node.MainModule]);
        }
        else
        {
            throw new Exception( "Main Module: " + node.MainModule + " not found!" );
        }
        
        return null;
    }

    public override object Visit( ModuleNode node )
    {
        return null;
    }

    public override object Visit( ModifiersNode node )
    {
        return null;
    }

    public override object Visit( DeclarationNode node )
    {
        return null;
    }

    public override object Visit( UsingStatementNode node )
    {
        return null;
    }

    public override object Visit( DeclarationsNode node )
    {
        return null;
    }

    public override object Visit( ClassDeclarationNode node )
    {
        return null;
    }

    public override object Visit( FunctionDeclarationNode node )
    {
        return null;
    }

    public override object Visit( VariableDeclarationNode node )
    {
        return null;
    }

    public override object Visit( ClassInstanceDeclarationNode node )
    {
        return null;
    }

    public override object Visit( CallNode node )
    {
        return null;
    }

    public override object Visit( ArgumentsNode node )
    {
        return null;
    }

    public override object Visit( ParametersNode node )
    {
        return null;
    }

    public override object Visit( AssignmentNode node )
    {
        return null;
    }

    public override object Visit( ExpressionNode node )
    {
        return null;
    }

    public override object Visit( BlockStatementNode node )
    {
        return null;
    }

    public override object Visit( StatementNode node )
    {
        return null;
    }

    public override object Visit( ExpressionStatementNode node )
    {
        return null;
    }

    public override object Visit( IfStatementNode node )
    {
        return null;
    }

    public override object Visit( ForStatementNode node )
    {
        return null;
    }

    public override object Visit( WhileStatementNode node )
    {
        return null;
    }

    public override object Visit( ReturnStatementNode node )
    {
        return null;
    }

    public override object Visit( InitializerNode node )
    {
        return null;
    }

    public override object Visit( BinaryOperationNode node )
    {
        return null;
    }

    public override object Visit( TernaryOperationNode node )
    {
        return null;
    }

    public override object Visit( PrimaryNode node )
    {
        switch ( node.PrimaryType )
        {
            case PrimaryNode.PrimaryTypes.Identifier:
            {
                // Look this variable up in the function.
                if (NamedValues.TryGetValue(node.PrimaryId.Id, out LLVMValueRef value))
                {
                    ValueStack.Push(value);
                }
                else
                {
                    throw new Exception("Unknown variable name");
                }
                return null; 
            }
            
            case PrimaryNode.PrimaryTypes.ThisReference:
                
                return null;

            case PrimaryNode.PrimaryTypes.BooleanLiteral:
               // LLVM.ConstInt( LLVMTypeRef.Int1Type(), , true );
                return null;

            case PrimaryNode.PrimaryTypes.IntegerLiteral:
                return null;

            case PrimaryNode.PrimaryTypes.FloatLiteral:
                return null;

            case PrimaryNode.PrimaryTypes.StringLiteral:
                return null;

            case PrimaryNode.PrimaryTypes.Expression:
                node.Expression.Accept( this );
                return null;
            
            case PrimaryNode.PrimaryTypes.NullReference:
                return null;

            case PrimaryNode.PrimaryTypes.Default:
            default:
                throw new ArgumentOutOfRangeException(
                    nameof( node.PrimaryType ),
                    node.PrimaryType,
                    null );
        }
        
        return null;
    }

    public override object Visit( StructDeclarationNode node )
    {
        return null;
    }

    public override object Visit( UnaryPostfixOperation node )
    {
        return null;
    }

    public override object Visit( UnaryPrefixOperation node )
    {
        return null;
    }

    public override object Visit( HeteroAstNode node )
    {
        return null;
    }
}

}
