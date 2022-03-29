﻿using System;
using System.Collections.Generic;
using System.Linq;
using Srsl.Ast;

namespace Srsl.Parser
{

    public class SrslParser
    {

        /// <summary>
        /// Parses a set of modules and returns a <see cref="ProgramNode"/>
        /// </summary>
        /// <param name="mainModule">The name of the Module containing the entry point</param>
        /// <param name="modules">A set of delegates that return the modules contents</param>
        /// <returns></returns>
        public ProgramNode ParseModules(string mainModule, IEnumerable<Func<string>> modules)
        {
            ProgramNode program = new ProgramNode(mainModule);

            foreach (Func<string> srslModule in modules)
            {
                ModuleNode module = ParseModule(srslModule());

                program.AddModule(module);
            }

            return program;
        }

        /// <summary>
        /// Parses a set of modules and returns a <see cref="ProgramNode"/>
        /// </summary>
        /// <param name="mainModule">The name of the Module containing the entry point</param>
        /// <param name="modules">A set of module contents for parsing, each item containing the code of a single module</param>
        /// <returns></returns>
        public ProgramNode ParseModules(string mainModule, IEnumerable<string> modules)
        {
            ProgramNode program = new ProgramNode(mainModule);

            foreach (string srslModule in modules)
            {
                ModuleNode module = ParseModule(srslModule);

                program.AddModule(module);
            }

            return program;
        }

        public ModuleNode ParseModule(string srslModule)
        {
            SrslLexer lexer = new SrslLexer(srslModule);
            SrslModuleParser parser = new SrslModuleParser(lexer);
            var context = parser.module();
            return context.Result;
        }

        public List<StatementNode> ParseStatements(string statements)
        {
            SrslLexer lexer = new SrslLexer(statements);
            SrslModuleParser parser = new SrslModuleParser(lexer);
            var contexts = parser.statements();
            return contexts.Select(c => c.Result).ToList();
        }

        public ExpressionNode ParseExpression(string expression)
        {
            SrslLexer lexer = new SrslLexer(expression);
            SrslModuleParser parser = new SrslModuleParser(lexer);
            return parser.expression().Result;
        }
    }

}
