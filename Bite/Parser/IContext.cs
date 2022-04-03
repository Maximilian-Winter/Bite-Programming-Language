using System;
using Bite.Ast;

namespace Bite.Parser
{

public interface IContext < out TNode > where TNode : HeteroAstNode
{
    Exception Exception { get; }

    bool Failed { get; }

    TNode Result { get; }
}

}
