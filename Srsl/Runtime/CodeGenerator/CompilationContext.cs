using System.Collections.Generic;
using Srsl.Ast;
using Srsl.Runtime.Bytecode;
using Srsl.Runtime.SymbolTable;

namespace Srsl.Runtime.CodeGen
{
    public class CompilationContext
    {
        private Dictionary<string, Chunk> m_CompilingChunks;
        private Chunk m_CompilingChunk;
        private Stack<Chunk> _savedChunks = new Stack<Chunk>();

        public SymbolTableBuilder SymbolTableBuilder { get; }
        public IEnumerable<KeyValuePair<string, Chunk>> CompilingChunks => m_CompilingChunks;
        public Dictionary<string, BinaryChunk> CompiledChunks { get; }
        public Chunk MainChunk { get;  }
        public BinaryChunk CompiledMainChunk { get; private set; }
        public Chunk CurrentChunk => m_CompilingChunk;

        public CompilationContext(ProgramNode programNode)
        {
            SymbolTableBuilder = new SymbolTableBuilder();
            SymbolTableBuilder.BuildProgramSymbolTable(programNode);
            m_CompilingChunks = new Dictionary<string, Chunk>();
            MainChunk = new Chunk();
            m_CompilingChunk = MainChunk;
            CompiledChunks = new Dictionary<string, BinaryChunk>();
        }

        public bool HasChunk(string moduleName)
        {
            return m_CompilingChunks.ContainsKey(moduleName);
        }

        public void PushChunk()
        {
            _savedChunks.Push(m_CompilingChunk);
        }

        public void PopChunk()
        {
            m_CompilingChunk = _savedChunks.Pop();
        }

        public void NewChunk()
        {
            m_CompilingChunk = new Chunk();
        }
        
        public void SaveCurrentChunk(string moduleName)
        {
            m_CompilingChunks.Add(moduleName, CurrentChunk);
        }

        public void RestoreChunk(string moduleName)
        {
            m_CompilingChunk = m_CompilingChunks[moduleName];
        }

        public void Build()
        {

            foreach (KeyValuePair<string, Chunk> compilingChunk in CompilingChunks)
            {
                CompiledChunks.Add(compilingChunk.Key, new BinaryChunk(compilingChunk.Value.SerializeToBytes(), compilingChunk.Value.Constants, compilingChunk.Value.Lines));
            }

            CompiledMainChunk = new BinaryChunk(MainChunk.SerializeToBytes(), MainChunk.Constants, MainChunk.Lines);
        }
    }
}