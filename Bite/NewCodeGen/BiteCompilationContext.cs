using System.Collections.Generic;
using Bite.Runtime.Bytecode;
using Bite.Symbols;

namespace Bite.Runtime.CodeGen
{

public class BiteCompilationContext
{
    private readonly Dictionary < string, Chunk > m_CompilingChunks;
    private readonly Stack < Chunk > _savedChunks = new Stack < Chunk >();
    private readonly Chunk m_MainChunk;

    internal BaseScope BaseScope { get; }

    internal Chunk CurrentChunk { get; private set; }

    internal Dictionary < string, BinaryChunk > CompiledChunks { get; private set; }

    internal BinaryChunk CompiledMainChunk { get; private set; }

    #region Public

    public BiteCompilationContext( SymbolTable symbolTable )
    {
        BaseScope = symbolTable.RootScope;
        m_CompilingChunks = new Dictionary < string, Chunk >();
        m_MainChunk = new Chunk();
        CurrentChunk = m_MainChunk;
    }

    internal void Build()
    {
        CompiledChunks = new Dictionary < string, BinaryChunk >();

        foreach ( KeyValuePair < string, Chunk > compilingChunk in m_CompilingChunks )
        {
            CompiledChunks.Add(
                compilingChunk.Key,
                new BinaryChunk(
                    compilingChunk.Value.SerializeToBytes(),
                    compilingChunk.Value.Constants,
                    compilingChunk.Value.Lines ) );
        }

        CompiledMainChunk = new BinaryChunk( m_MainChunk.SerializeToBytes(), m_MainChunk.Constants, m_MainChunk.Lines );
    }

    internal bool HasChunk( string moduleName )
    {
        return m_CompilingChunks.ContainsKey( moduleName );
    }

    internal void NewChunk()
    {
        CurrentChunk = new Chunk();
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

    internal void SetCurrentChunk( Chunk chunk )
    {
        CurrentChunk = chunk;
    }

    #endregion
}

}
