using System;
using System.Collections.Generic;
using System.Globalization;
using Srsl.Ast;

namespace Srsl.Parser
{


    public partial class SrslModuleParser 
    {
        #region Private

        private IContext<TNode> ProcessRule<TNode>(string ruleName, Func<IContext<TNode>> ruleImpl) where TNode : HeteroAstNode
        {
            int startTokenIndex = index();

            if (Speculating)
            {
                var alreadyParsed = alreadyParsedRule(MemoizingDictionary, ruleName);

                if (alreadyParsed.Failed)
                {
                    return Context<TNode>.AsFailed(new AlreadyParsedFailedException());
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
