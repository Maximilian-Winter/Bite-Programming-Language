using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bite.Parser;
using Bite.Runtime.CodeGen;

namespace Bite.Runtime
{
    public class Compiler
    {
        private readonly bool m_ThrowOnRecognitionException;
        private BiteParser m_Parser;

        public Exception Exception => m_Parser.Exception;
        public bool Failed => m_Parser.Failed;

        public Compiler(bool throwOnRecognitionException)
        {
            m_ThrowOnRecognitionException = throwOnRecognitionException;
        }

        public BiteProgram Compile(string mainModule, IEnumerable<string> modules)
        {
            m_Parser = new BiteParser
            {
                ThrowOnRecognitionException = m_ThrowOnRecognitionException
            };
            var program = m_Parser.ParseModules(mainModule, modules);

            CodeGenerator generator = new CodeGenerator();
            return generator.CompileProgram(program);
        }

        public BiteProgram CompileExpression(string expression)
        {
            m_Parser = new BiteParser
            {
                ThrowOnRecognitionException = m_ThrowOnRecognitionException
            };

            var expressionNode = m_Parser.ParseExpression(expression);

            CodeGenerator generator = new CodeGenerator();

            return generator.CompileExpression(expressionNode);
        }

        public BiteProgram Compile(IReadOnlyCollection<Module> modules)
        {
            m_Parser = new BiteParser
            {
                ThrowOnRecognitionException = m_ThrowOnRecognitionException
            };

            var moduleStrings = new List<string>();

            foreach (var module in modules)
            {
                var moduleBuilder = new StringBuilder();
                moduleBuilder.AppendLine($"module {module.Name};\r\n");
                foreach (var import in module.Imports)
                {
                    moduleBuilder.AppendLine($"import {import};");
                    moduleBuilder.AppendLine($"using {import};");
                }
                moduleBuilder.AppendLine();
                moduleBuilder.AppendLine(module.Code);

                moduleStrings.Add(moduleBuilder.ToString());
            }

            var program = m_Parser.ParseModules(modules.Single(m => m.MainModule).Name, moduleStrings);

            CodeGenerator generator = new CodeGenerator();

            return generator.CompileProgram(program);
        }
    }
}