using System;

namespace CgfConverter.CryEngineCore;

internal enum ChunkFactoryClassification
{
    ParserAvailable,
    IntentionallyIgnored,
    EnumDefinedUnimplemented,
    RawUnknown,
}

internal static class ChunkFactoryClassifier
{
    internal static ChunkFactoryClassification Classify(ChunkType chunkType, Chunk chunk)
    {
        if (chunk is not ChunkUnknown)
            return ChunkFactoryClassification.ParserAvailable;

        if (chunkType is ChunkType.IvoAssetMetadata
            or ChunkType.IvoLodDistances
            or ChunkType.IvoLodMeshData
            or ChunkType.IvoBoundingData
            or ChunkType.IvoChunkTerminator
            or ChunkType.IvoMtlNameVariant)
        {
            return ChunkFactoryClassification.IntentionallyIgnored;
        }

        return Enum.IsDefined(typeof(ChunkType), chunkType)
            ? ChunkFactoryClassification.EnumDefinedUnimplemented
            : ChunkFactoryClassification.RawUnknown;
    }
}

internal enum ModelReadStage
{
    FileOpen,
    FileHeader,
    ChunkTable,
    FileRead,
    ChunkFactory,
    ChunkRead,
    DataStreamRead,
    ChunkSkip,
}

internal enum ReadFailureClassification
{
    UnsupportedFileSignature,
    UnsupportedVersion,
    ParseFailure,
    IoFailure,
    ResourceExhaustion,
    UnexpectedFailure,
}

internal readonly record struct FileHeaderObservation(
    string? Signature,
    uint? FileTypeRaw,
    uint? FileVersionRaw,
    uint? DeclaredChunkCount,
    int? ChunkTableOffset,
    bool Complete);

internal readonly record struct ChunkOccurrence(
    int TableIndex,
    uint TypeCode,
    uint VersionRaw,
    uint Offset,
    uint? Size,
    int? StoredId)
{
    internal int Number => TableIndex + 1;
    internal uint Version => VersionRaw & 0x7fffffffu;
}

internal interface IModelReadObserver
{
    void OnFileHeader(in FileHeaderObservation header);

    void OnFileFailure(
        ModelReadStage stage,
        ReadFailureClassification classification,
        Exception exception);

    void OnChunkClassified(
        in ChunkOccurrence occurrence,
        ChunkFactoryClassification classification);

    void OnChunkFailure(
        in ChunkOccurrence occurrence,
        ModelReadStage stage,
        ReadFailureClassification classification,
        Exception exception);

    void OnUnknownDataStream(
        in ChunkOccurrence occurrence,
        uint typeCode,
        uint offset);
}
