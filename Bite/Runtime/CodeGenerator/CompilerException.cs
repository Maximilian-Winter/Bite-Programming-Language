using System;

namespace Bite.Runtime.CodeGen
{

public class CompilerException : Exception
{
    #region Public

    public CompilerException( string message ) : base( message )
    {
    }

    #endregion
}

}
