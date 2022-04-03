using Bite.Runtime.Bytecode;

namespace Bite.Runtime
{

public class BiteChunkWrapper
{
    public BinaryChunk ChunkToWrap { get; set; }

    #region Public

    public BiteChunkWrapper()
    {
        ChunkToWrap = null;
    }

    public BiteChunkWrapper( BinaryChunk chunkToWrap )
    {
        ChunkToWrap = chunkToWrap;
    }

    #endregion
}

}
