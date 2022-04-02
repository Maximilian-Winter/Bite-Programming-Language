using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Bite.Ast;

namespace Bite.Parser
{

    public class BiteParser
    {
        public bool ThrowOnRecognitionException { get; set; }
        public Exception Exception { get; private set; }
        public bool Failed { get; private set; }

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
            BiteLexer lexer = new BiteLexer(srslModule);
            BiteModuleParser parser = new BiteModuleParser(lexer);
            var context = parser.module();
            Exception = context.Exception;
            Failed = context.Failed;
            if (context.Failed && ThrowOnRecognitionException)
            {
                throw context.Exception;
            }
            return context.Result;
        }

        public IReadOnlyCollection<StatementNode> ParseStatements(string statements)
        {
            BiteLexer lexer = new BiteLexer(statements);
            BiteModuleParser parser = new BiteModuleParser(lexer);
            var contexts = parser.statements();
            Exception = contexts[0].Exception;
            Failed = contexts[0].Failed;
            if (contexts[0].Failed && ThrowOnRecognitionException)
            {
                throw contexts[0].Exception;
            }
            return contexts.Select(c => c.Result).ToList();
        }

        public ExpressionNode ParseExpression(string expression)
        {
            BiteLexer lexer = new BiteLexer(expression);
            BiteModuleParser parser = new BiteModuleParser(lexer);
            var context = parser.expression();
            Exception = context.Exception;
            Failed = context.Failed;
            if (context.Failed && ThrowOnRecognitionException)
            {
                throw context.Exception;
            }
            return context.Result;
        }
    }

}
