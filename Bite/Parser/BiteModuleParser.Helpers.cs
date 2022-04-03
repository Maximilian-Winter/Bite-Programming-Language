using System;
using Bite.Ast;

namespace Bite.Parser
{

public partial class BiteModuleParser
{
    #region Private

    private IContext < TNode > ProcessRule < TNode >( string ruleName, Func < IContext < TNode > > ruleImpl )
        where TNode : HeteroAstNode
    {
        int startTokenIndex = index();

        if ( Speculating )
        {
            AlreadyParsedRuleResult alreadyParsed = alreadyParsedRule( MemoizingDictionary, ruleName );

            if ( alreadyParsed.Failed )
            {
                return Context < TNode >.AsFailed( new AlreadyParsedFailedException() );
            }

            if ( alreadyParsed.Result )
            {
                return new Context < TNode >( null );
            }
        }

        IContext < TNode > context = ruleImpl();

        if ( Speculating )
        {
            memoize( MemoizingDictionary, ruleName, startTokenIndex, context.Failed );
        }

        return context;
    }

    #endregion
}

}
