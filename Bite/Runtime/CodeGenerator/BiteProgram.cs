using System.Collections.Generic;
using Bite.Ast;
using Bite.Runtime.Bytecode;
using Bite.Runtime.SymbolTable;
using Bite.SymbolTable;

namespace Bite.Runtime.CodeGen
{

public class BiteProgram
{
    private readonly Dictionary < string, Chunk > m_CompilingChunks;
    private readonly Stack < Chunk > _savedChunks = new Stack < Chunk >();

    public SymbolTableBuilder SymbolTableBuilder;
    internal BaseScope BaseScope { get; }

    internal Dictionary < string, BinaryChunk > CompiledChunks { get; }

    internal Chunk MainChunk { get; }

    internal BinaryChunk CompiledMainChunk { get; private set; }

    internal Chunk CurrentChunk { get; private set; }

    #region Public

    public BiteProgram( ModuleNode module, BiteProgram previousProgram = null )
    {
        if ( previousProgram == null )
        {
            SymbolTableBuilder = new SymbolTableBuilder();
        }
        else
        {
            SymbolTableBuilder = previousProgram.SymbolTableBuilder;
        }
        SymbolTableBuilder.BuildModuleSymbolTable( module );
        BaseScope = SymbolTableBuilder.CurrentScope as BaseScope;
        m_CompilingChunks = new Dictionary < string, Chunk >();
        MainChunk = new Chunk();
        CurrentChunk = MainChunk;
        CompiledChunks = new Dictionary < string, BinaryChunk >();
    }

    public BiteProgram( ProgramNode programNode )
    {
        SymbolTableBuilder = new SymbolTableBuilder();
        SymbolTableBuilder.BuildProgramSymbolTable( programNode );
        BaseScope = SymbolTableBuilder.CurrentScope as BaseScope;
        m_CompilingChunks = new Dictionary < string, Chunk >();
        MainChunk = new Chunk();
        CurrentChunk = MainChunk;
        CompiledChunks = new Dictionary < string, BinaryChunk >();
    }

    /// <summary>
    ///     Creates a new <see cref="BiteVm" /> and executes the <see cref="BiteProgram" />
    /// </summary>
    /// <returns></returns>
    public BiteResult Run()
    {
        BiteVm biteVm = new BiteVm();
        biteVm.InitVm();
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
