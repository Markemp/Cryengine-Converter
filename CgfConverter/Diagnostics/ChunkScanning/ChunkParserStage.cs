namespace CgfConverter.Diagnostics.ChunkScanning;

public enum ChunkParserStage
{
    FileOpen,
    FileHeader,
    ChunkTable,
    ChunkFactory,
    ChunkRead,
    ChunkSkip
}
