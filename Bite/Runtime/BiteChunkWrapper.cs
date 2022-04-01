using Srsl.Runtime.Bytecode;

namespace Srsl.Runtime
{

    public class BiteChunkWrapper
    {
        private BinaryChunk m_ChunkToWrap;

        public BiteChunkWrapper()
        {
            m_ChunkToWrap = null;
        }

        public BiteChunkWrapper(BinaryChunk chunkToWrap)
        {
            m_ChunkToWrap = chunkToWrap;
        }

        public BinaryChunk ChunkToWrap
        {
            get => m_ChunkToWrap;
            set => m_ChunkToWrap = value;
        }
    }

}
