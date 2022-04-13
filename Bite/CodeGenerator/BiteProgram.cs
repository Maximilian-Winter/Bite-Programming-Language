using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bite.Modules.Callables;
using Bite.Runtime.Bytecode;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Symbols;

namespace Bite.Runtime.CodeGen
{

/// <summary>
/// 
/// </summary>
public class BiteProgram
{
    public TypeRegistry TypeRegistry { get; } = new TypeRegistry();

    public SymbolTable SymbolTable { get; private set; }

    internal BinaryChunk CompiledMainChunk { get; private set; }

    internal Dictionary < string, BinaryChunk > CompiledChunks { get; private set; }

    #region Public

    internal BiteProgram()
    {
    }

    internal static BiteProgram Create( SymbolTable symbolTable, BinaryChunk compiledMainChunk,
        Dictionary < string, BinaryChunk > compiledChunks )
    {
        return new BiteProgram()
        {
            SymbolTable = symbolTable,
            CompiledMainChunk = compiledMainChunk,
            CompiledChunks = compiledChunks,
        };
    }

    /// <summary>
    /// Creates a new <see cref="BiteVm" /> and executes the current <see cref="BiteProgram" />
    /// </summary>
    /// <returns></returns>
    public BiteResult Run( Dictionary < string, object > externalObjects = null )
    {
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();
        biteVm.RegisterSystemModuleCallables( TypeRegistry );
        biteVm.RegisterExternalGlobalObjects( externalObjects );

        BiteVmInterpretResult result = biteVm.Interpret( this, CancellationToken.None );

        return new BiteResult { InterpretResult = result, ReturnValue = biteVm.ReturnValue };
    }

    /// <summary>
    /// Creates a new <see cref="BiteVm" /> and executes the current <see cref="BiteProgram" />
    /// </summary>
    /// <returns></returns>
    public BiteResult Run( CancellationToken cancellationToken, Dictionary < string, object > externalObjects = null )
    {
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();
        biteVm.RegisterSystemModuleCallables( TypeRegistry );
        biteVm.RegisterExternalGlobalObjects( externalObjects );

        BiteVmInterpretResult result = biteVm.Interpret( this, cancellationToken );

        return new BiteResult { InterpretResult = result, ReturnValue = biteVm.ReturnValue };
    }

    /// <summary>
    /// Creates a new <see cref="BiteVm" /> and executes the current <see cref="BiteProgram" />
    /// </summary>
    /// <returns></returns>
    public async Task < BiteResult > RunAsync( CancellationToken cancellationToken,
        Dictionary < string, object > externalObjects = null )
    {
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();
        biteVm.RegisterSystemModuleCallables( TypeRegistry );
        biteVm.RegisterExternalGlobalObjects( externalObjects );

        BiteVmInterpretResult result = await biteVm.InterpretAsync( this, cancellationToken );

        return new BiteResult { InterpretResult = result, ReturnValue = biteVm.ReturnValue };
    }

    #endregion
}

}
