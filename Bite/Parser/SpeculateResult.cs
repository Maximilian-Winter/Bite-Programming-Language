using Bite.Ast;

namespace Bite.Parser
{
    public struct SpeculateResult<TNode> where TNode : HeteroAstNode
    {
        public bool ShouldReturn { get; }
        public IContext<TNode> Context { get; }

        public SpeculateResult(bool shouldReturn, IContext<TNode> context)
        {
            ShouldReturn = shouldReturn;
            Context = context;
        }
    }
}