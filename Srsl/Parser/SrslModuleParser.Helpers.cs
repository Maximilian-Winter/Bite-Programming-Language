using System;
using System.Collections.Generic;
using System.Globalization;
using Srsl.Ast;

namespace Srsl.Parser
{
    public partial class SrslModuleParser 
    {
        #region Private

        private SpeculateResult<TNode> SpeculateRule<TNode>(string ruleName) where TNode : HeteroAstNode
        {
            if (!Speculating) return new SpeculateResult<TNode>(false, null);

            var alreadyParsed = alreadyParsedRule(MemoizingDictionary, "assignment");

            if (alreadyParsed.Failed)
            {
                return new SpeculateResult<TNode>(true, Context<TNode>.AsFailed());
            }
            if (alreadyParsed.Result)
            {
                return new SpeculateResult<TNode>(true, null);
            }

            return new SpeculateResult<TNode>(false, null);


        }

        private IContext<TNode> ProcessRule<TNode>(string ruleName, Func<IContext<TNode>> ruleImpl) where TNode : HeteroAstNode
        {
            int startTokenIndex = index();

            if (Speculating)
            {
                var alreadyParsed = alreadyParsedRule(MemoizingDictionary, ruleName);

                if (alreadyParsed.Failed)
                {
                    return Context<TNode>.AsFailed();
                }

                if (alreadyParsed.Result)
                {
                    return new Context<TNode>(null);
                }
            }

            var context = ruleImpl();

            if (Speculating)
            {
                memoize(MemoizingDictionary, ruleName, startTokenIndex, context.Failed);
            }

            return context;
        }

        #endregion
    }

}
