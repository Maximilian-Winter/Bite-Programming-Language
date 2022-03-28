using System;
using System.Collections.Generic;
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
            SrslModuleParser moduleParser = new SrslModuleParser(lexer);
            ModuleNode module = moduleParser.module();
            return module;
        }

        public List<StatementNode> ParseStatements(string srslStatments)
        {
            SrslLexer lexer = new SrslLexer(srslStatments);
            SrslModuleParser statmentParser = new SrslModuleParser(lexer);
            List<StatementNode> statementNodes = statmentParser.statements();

            return statementNodes;
        }
    }

}
