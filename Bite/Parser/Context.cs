using System;
using Bite.Ast;

namespace Bite.Parser
{
    public class Context<TNode> : IContext<TNode> where TNode : HeteroAstNode
    {
        public TNode Result { get; }
        public bool Failed { get; private set; }
        public Exception Exception { get; private set; }

        private Context()
        {
        }

        public Context(TNode result)
        {
            Result = result;
        }

        //public static Context<TNode> AsFailed()
        //{
        //    return new Context<TNode>() { Failed = true };
        //}

        public static Context<TNode> AsFailed(Exception ex)
        {
            return new Context<TNode>() { Failed = true, Exception = ex };
        }
    }
}