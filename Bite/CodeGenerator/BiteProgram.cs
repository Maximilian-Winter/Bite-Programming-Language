using System.Collections.Generic;
using Bite.Ast;
using Bite.Runtime.Bytecode;
using Bite.Runtime.Functions;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Symbols;

namespace Bite.Runtime.CodeGen
{

public class BiteProgram
{
    public SymbolTable SymbolTable { get; }

    private readonly Dictionary < string, Chunk > m_CompilingChunks;
    private readonly Stack < Chunk > _savedChunks = new Stack < Chunk >();

    internal BaseScope BaseScope { get; }

    internal Dictionary < string, BinaryChunk > CompiledChunks { get; }

    internal Chunk MainChunk { get; }

    internal BinaryChunk CompiledMainChunk { get; private set; }

    internal Chunk CurrentChunk { get; private set; }

    #region Public

    public BiteProgram( SymbolTable symbolTable )
    {
        SymbolTable = symbolTable;
        BaseScope = symbolTable.RootScope;
        m_CompilingChunks = new Dictionary < string, Chunk >();
        MainChunk = new Chunk();
        CurrentChunk = MainChunk;
        CompiledChunks = new Dictionary < string, BinaryChunk >();
    }

    public Dictionary < string, BinaryChunk > GetChunks()
    {
        return CompiledChunks;
    }

    public void RestoreChunks( Dictionary<string, BinaryChunk> chunks )
    {
        foreach ( var compiledChunk in chunks)
        {
            if ( !CompiledChunks.ContainsKey( compiledChunk.Key ) )
            {
                CompiledChunks.Add( compiledChunk.Key, compiledChunk.Value );
            }
        }
    }
    
    /// <summary>
    ///     Creates a new <see cref="BiteVm" /> and executes the <see cref="BiteProgram" />
    /// </summary>
    /// <returns></returns>
    public BiteResult Run( Dictionary < string, object > externalObjects = null )
    {
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();
        // TODO: move somewhere else!
        biteVm.RegisterCallable( "CSharpInterfaceCall", new ForeignLibraryInterfaceVm() );
        biteVm.RegisterCallable( "Print", new PrintFunctionVm() );
        biteVm.RegisterCallable( "PrintLine", new PrintLineFunctionVm() );

        if ( externalObjects != null )
        {
            foreach ( KeyValuePair < string, object > externalCSharpObject in externalObjects )
            {
                biteVm.RegisterExternalGlobalObject( externalCSharpObject.Key, externalCSharpObject.Value );
            }
        }

        BiteVmInterpretResult result = biteVm.Interpret( this );

        return new BiteResult { InterpretResult = result, ReturnValue = biteVm.ReturnValue };
    }

    internal void Build()
    {
        foreach ( KeyValuePair < string, Chunk > compilingChunk in m_CompilingChunks )
        {
            CompiledChunks.Add(
                compilingChunk.Key,
                new BinaryChunk(
                    compilingChunk.Value.SerializeToBytes(),
                    compilingChunk.Value.Constants,
                    compilingChunk.Value.Lines ) );
        }

        CompiledMainChunk = new BinaryChunk( MainChunk.SerializeToBytes(), MainChunk.Constants, MainChunk.Lines );
    }

    internal bool HasChunk( string moduleName )
    {
        return m_CompilingChunks.ContainsKey( moduleName );
    }

    internal void NewChunk()
    {
        CurrentChunk = new Chunk();
    }
    internal void SetCurrentChunk( Chunk chunk )
    {
        CurrentChunk = chunk;
    }

    internal void PopChunk()
    {
        CurrentChunk = _savedChunks.Pop();
    }

    internal void PushChunk()
    {
        // TODO: this is only ever one depth pushed so maybe we don't need a stack and Push/Pop API?
        _savedChunks.Push( CurrentChunk );
    }

    internal void RestoreChunk( string moduleName )
    {
        CurrentChunk = m_CompilingChunks[moduleName];
    }

    internal void SaveCurrentChunk( string moduleName )
    {
        m_CompilingChunks.Add( moduleName, CurrentChunk );
    }

    #endregion
}

}
