using System.IO;
using Srsl.Parser;
using Srsl.Runtime;
using Srsl.Runtime.CodeGen;
using Xunit;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void AddNumbers()
        {
            SrslParser parser = new SrslParser();

            var statements = parser.ParseStatements("1 + 1");

            CodeGenerator generator = new CodeGenerator();

            var context = generator.CompileStatements(statements);

            SrslVm srslVm = new SrslVm();

            srslVm.Interpret(context);
        }
    }
}