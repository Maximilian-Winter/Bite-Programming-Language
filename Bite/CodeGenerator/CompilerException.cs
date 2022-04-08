using System;
using Bite.Ast;

namespace Bite.Runtime.CodeGen
{

public class CompilerException : Exception
{
    public HeteroAstNode Node { get; }

    #region Public
    
    public CompilerException( string message, HeteroAstNode node ) : base( message )
    {
        Node = node;
    }

    public override string Message
    {
        get
        {
            return base.Message +
                   $" in column {Node.DebugInfoAstNode.ColumnNumber} on line {Node.DebugInfoAstNode.LineNumber}";
        }
    }

    #endregion
}

}
