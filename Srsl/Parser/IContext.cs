using System;
using Srsl.Ast;

namespace Srsl.Parser
{
    public interface IContext<out TNode> where TNode : HeteroAstNode
    {
        Exception Exception { get; }
        TNode Result { get; }
        bool Failed { get; }
    }
}