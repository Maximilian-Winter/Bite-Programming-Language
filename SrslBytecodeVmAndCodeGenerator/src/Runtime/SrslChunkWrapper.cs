using Srsl.Runtime.Bytecode;

namespace Srsl.Runtime
{

    public class SrslChunkWrapper
    {
        private BinaryChunk m_ChunkToWrap;

        public SrslChunkWrapper()
        {
            m_ChunkToWrap = null;
        }

        public SrslChunkWrapper(BinaryChunk chunkToWrap)
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
