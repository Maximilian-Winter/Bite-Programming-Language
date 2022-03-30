using System.Collections.Generic;
using Srsl.Ast;
using Srsl.Runtime.Bytecode;
using Srsl.Runtime.SymbolTable;

namespace Srsl.Runtime.CodeGen
{
    public class SrslProgram
    {
        private Dictionary<string, Chunk> m_CompilingChunks;
        private Chunk m_CompilingChunk;
        private Stack<Chunk> _savedChunks = new Stack<Chunk>();

        internal SymbolTableBuilder SymbolTableBuilder { get; }
        internal IEnumerable<KeyValuePair<string, Chunk>> CompilingChunks => m_CompilingChunks;
        internal Dictionary<string, BinaryChunk> CompiledChunks { get; }
        internal Chunk MainChunk { get;  }
        internal BinaryChunk CompiledMainChunk { get; private set; }
        internal Chunk CurrentChunk => m_CompilingChunk;

        public SrslProgram(ModuleNode module)
        {
            SymbolTableBuilder = new SymbolTableBuilder();
            SymbolTableBuilder.BuildModuleSymbolTable(module);
            //SymbolTableBuilder.BuildStatementsSymbolTable(statements);
            m_CompilingChunks = new Dictionary<string, Chunk>();
            MainChunk = new Chunk();
            m_CompilingChunk = MainChunk;
            CompiledChunks = new Dictionary<string, BinaryChunk>();
        }

        public SrslProgram(ProgramNode programNode)
        {
            SymbolTableBuilder = new SymbolTableBuilder();
            SymbolTableBuilder.BuildProgramSymbolTable(programNode);
            m_CompilingChunks = new Dictionary<string, Chunk>();
            MainChunk = new Chunk();
            m_CompilingChunk = MainChunk;
            CompiledChunks = new Dictionary<string, BinaryChunk>();
        }

        internal bool HasChunk(string moduleName)
        {
            return m_CompilingChunks.ContainsKey(moduleName);
        }

        internal void PushChunk()
        {
            _savedChunks.Push(m_CompilingChunk);
        }

        internal void PopChunk()
        {
            m_CompilingChunk = _savedChunks.Pop();
        }

        internal void NewChunk()
        {
            m_CompilingChunk = new Chunk();
        }

        internal void SaveCurrentChunk(string moduleName)
        {
            m_CompilingChunks.Add(moduleName, CurrentChunk);
        }

        internal void RestoreChunk(string moduleName)
        {
            m_CompilingChunk = m_CompilingChunks[moduleName];
        }

        internal void Build()
        {

            foreach (KeyValuePair<string, Chunk> compilingChunk in CompilingChunks)
            {
                CompiledChunks.Add(compilingChunk.Key, new BinaryChunk(compilingChunk.Value.SerializeToBytes(), compilingChunk.Value.Constants, compilingChunk.Value.Lines));
            }

            CompiledMainChunk = new BinaryChunk(MainChunk.SerializeToBytes(), MainChunk.Constants, MainChunk.Lines);
        }

        public SrslVm Run()
        {
            var srslVm = new SrslVm();
            srslVm.Interpret(this);
            return srslVm;
        }
    }
}