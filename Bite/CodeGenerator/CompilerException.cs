using System;
using Bite.Ast;

namespace Bite.Runtime.CodeGen
{

public class CompilerException : Exception
{
    public AstBaseNode BaseNode { get; }

    public override string Message =>
        base.Message +
        $" in column {BaseNode.DebugInfoAstNode.ColumnNumber} on line {BaseNode.DebugInfoAstNode.LineNumber}";

    #region Public

    public CompilerException( string message, AstBaseNode baseNode ) : base( message )
    {
        BaseNode = baseNode;
    }

    #endregion
}

}
