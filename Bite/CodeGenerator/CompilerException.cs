using System;
using Bite.Ast;

namespace Bite.Runtime.CodeGen
{

public class CompilerException : Exception
{
    public AstBaseNode BaseNode { get; }

    #region Public
    
    public CompilerException( string message, AstBaseNode baseNode ) : base( message )
    {
        BaseNode = baseNode;
    }

    public override string Message
    {
        get
        {
            return base.Message +
                   $" in column {BaseNode.DebugInfoAstNode.ColumnNumber} on line {BaseNode.DebugInfoAstNode.LineNumber}";
        }
    }

    #endregion
}

}
