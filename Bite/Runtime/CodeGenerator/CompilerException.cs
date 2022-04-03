using System;

namespace Bite.Runtime.CodeGen
{
    public class CompilerException : Exception
    {
        public CompilerException(string message) : base(message)
        {

        }
    }
}