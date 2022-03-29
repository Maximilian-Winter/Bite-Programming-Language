using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Srsl.Parser;
using Srsl.Runtime.Bytecode;
using Srsl.Runtime.CodeGen;

namespace Srsl.Runtime
{
    internal class Class1
    {
        public void Compile()
        {

            var files = Directory.EnumerateFiles(".\\TestProgram", "*.srsl", SearchOption.AllDirectories);

            SrslParser parser = new SrslParser();

            var program = parser.ParseModules("MainModule", files.Select(File.ReadAllText));
            
            CodeGenerator generator = new CodeGenerator();

            var context = generator.CompileProgram(program);

            SrslVm srslVm = new SrslVm();

            srslVm.Interpret(context);


        }
    }
}
