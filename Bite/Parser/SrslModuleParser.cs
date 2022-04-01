﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Bite.Ast;

namespace Bite.Parser
{
    public partial class SrslModuleParser : Parser
    {
        public readonly IDictionary<int, IDictionary<string, int>> MemoizingDictionary =
            new Dictionary<int, IDictionary<string, int>>();

        private bool MatchSemicolonAtTheEndOfVariableAndClassInstanceDeclaration = true;
        #region Public

        public SrslModuleParser(Lexer input) : base(input)
        {
        }

        public override void clearMemo()
        {
            MemoizingDictionary.Clear();
        }
        #endregion
    }

}
