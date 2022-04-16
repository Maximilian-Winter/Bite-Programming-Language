using System;

namespace Bite.Runtime
{

public class BiteVmRuntimeException : ApplicationException
{
    public string BiteVmRuntimeExceptionMessage { get; }

    #region Public

    public BiteVmRuntimeException( string message ) : base( message )
    {
        BiteVmRuntimeExceptionMessage = message;
    }

    #endregion
}

}
