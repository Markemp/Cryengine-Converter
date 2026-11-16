using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.Models;
using CgfConverter.Services;

namespace CgfConverter.CryEngineCore;

public class Model
{
    private bool fileTypeRead;
    private bool fileVersionRead;
    private bool declaredChunkCountRead;
    private bool chunkTableOffsetRead;
    private IModelReadObserver? activeObserver;
    private ChunkOccurrence? activeOccurrence;

    /// <summary> The Root of the loaded object </summary>
    public ChunkNode? RootNode { get; internal set; }

    /// <summary> Gets the root nodes. </summary>
    public IEnumerable<ChunkNode> RootNodes => NodeMap.Values.Where(x => x.ParentNodeID == ~0);

    /// <summary> Lookup Table for Chunks, indexed by ChunkID </summary>
    public Dictionary<int, Chunk> ChunkMap { get; internal set; } = new();

    /// <summary> The name of the currently processed file </summary>
    public string? FileName { get; internal set; }

    /// <summary> The File Signature - CryTek for 3.5 and lower. CrCh for 3.6 and higher. #ivo for some SC files. </summary>
    public string? FileSignature { get; internal set; }

    /// <summary> The type of file (geometry or animation) </summary>
    public FileType FileType { get; internal set; }

    public FileVersion FileVersion { get; internal set; }

    /// <summary> Position of the Chunk Header table </summary>
    public int ChunkTableOffset { get; internal set; }

    /// <summary>Contains all the information about bones and skinning them.  This a reference to the Cryengine object, since multiple Models can exist for a single object).</summary>
    public SkinningInfo SkinningInfo { get; set; } = new();

    /// <summary> The Bones in the model.  The CompiledBones chunk will have a unique RootBone. </summary>
    public ChunkCompiledBones? Bones { get; internal set; }

    public uint NumChunks { get; internal set; }

    public List<ChunkHeader> chunkHeaders = new();

    private Dictionary<int, ChunkNode> nodeMap { get; set; }
    /// <summary> All NodeChunks for this model in a dictionary by chunk Id. </summary>
    public Dictionary<int, ChunkNode> NodeMap
    {
        get
        {
            if (nodeMap is null)
            {
                nodeMap = new Dictionary<int, ChunkNode>() { };
                ChunkNode? rootNode = null;
                RootNode = rootNode = (rootNode ?? RootNode);  // Each model will have it's own rootnode.

                foreach (ChunkNode node in ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node).OrderBy(ob=>ob.ID))
                {
                    // Preserve existing parents
                    if (nodeMap.TryGetValue(node.ID, out ChunkNode knownNode))
                    {
                        ChunkNode parentNode = knownNode.ParentNode;

                        if (parentNode is not null)
                            parentNode = nodeMap[parentNode.ID];

                        node.ParentNode = parentNode;
                    }

                    nodeMap[node.ID] = node;
                }
            }
            return nodeMap;
        }
    }

    public bool HasBones => Bones is not null;

    public bool HasGeometry
    {
        get
        {
            var types = ChunkMap.Select(n => n.Value.ChunkType);
            if (ChunkMap.Select(n => n.Value.ChunkType).Contains(ChunkType.Mesh) || ChunkMap.Select(n => n.Value.ChunkType).Contains(ChunkType.MeshIvo))
                return true;

            return false;
        }
    }

    /// <summary> Load the specified stream as a Model</summary>
    public static Model FromStream(string fileName, Stream stream, bool closeStream = false) =>
        FromStreamCore(fileName, stream, observer: null, closeStream);

    internal static Model FromStream(
        string fileName,
        Stream stream,
        IModelReadObserver observer,
        bool closeStream = false) =>
        FromStreamCore(fileName, stream, observer, closeStream);

    private static Model FromStreamCore(
        string fileName,
        Stream stream,
        IModelReadObserver? observer,
        bool closeStream)
    {
        try
        {
            var buffer = new Model();
            buffer.Load(fileName, stream, observer);
            return buffer;
        }
        finally
        {
            if (closeStream)
                stream.Close();
        }
    }

    public bool IsIvoFile => FileSignature?.Equals("#ivo") ?? false;

    internal void ObserveUnknownDataStream(uint typeCode, long offset)
    {
        if (activeObserver is null || activeOccurrence is not ChunkOccurrence occurrence)
            return;
        if (offset < 0 || offset > uint.MaxValue)
            throw new InvalidDataException($"Datastream offset is outside the supported file range: {offset}.");
        activeObserver.OnUnknownDataStream(occurrence, typeCode, (uint)offset);
    }

    private void Load(string fileName, Stream fs, IModelReadObserver? observer)
    {
        FileName = fileName;

        using EndiannessChangeableBinaryReader reader = new(fs);
        // Get the header.  This isn't essential for .cgam files, but we need this info to find the version and offset to the chunk table
        try
        {
            ReadFileHeader(reader);
        }
        catch (Exception exception) when (observer is not null && IsObservableDiagnosticException(exception))
        {
            observer.OnFileHeader(CreateFileHeaderObservation(complete: false));
            observer.OnFileFailure(
                ModelReadStage.FileHeader,
                exception is NotSupportedException && FileSignature is not null
                    ? ReadFailureClassification.UnsupportedFileSignature
                    : ClassifyReadFailure(exception),
                exception);
            throw;
        }

        observer?.OnFileHeader(CreateFileHeaderObservation(complete: true));

        try
        {
            ReadChunkTable(reader, observer);
        }
        catch (Exception exception) when (observer is not null && IsObservableDiagnosticException(exception))
        {
            observer.OnFileFailure(
                ModelReadStage.ChunkTable,
                ClassifyReadFailure(exception),
                exception);
            throw;
        }

        ReadChunks(reader, observer);

        // Move this to the parent.  Model should just read a single file.
        //BuildIvoNodeStructure();
        
    }

    private void ReadFileHeader(BinaryReader b)
    {
        b.BaseStream.Seek(0, SeekOrigin.Begin);
        FileSignature = b.ReadFString(4);

        if (FileSignature == "CrCh")           // file signature v3.6+
        {
            FileVersion = (FileVersion)b.ReadUInt32();    // 0x746
            fileVersionRead = true;
            NumChunks = b.ReadUInt32();       // number of Chunks in the chunk table
            declaredChunkCountRead = true;
            ChunkTableOffset = b.ReadInt32(); // location of the chunk table
            chunkTableOffsetRead = true;

            return;
        }
        else if (FileSignature == "#ivo")
        {
            FileVersion = (FileVersion)b.ReadUInt32();  // 0x0900
            fileVersionRead = true;
            NumChunks = b.ReadUInt32();
            declaredChunkCountRead = true;
            ChunkTableOffset = b.ReadInt32();
            chunkTableOffsetRead = true;
            return;
        }

        b.BaseStream.Seek(0, SeekOrigin.Begin);
        FileSignature = b.ReadFString(8);

        if (FileSignature == "CryTek")         // file signature v3.5-
        {
            FileType = (FileType)b.ReadUInt32();
            fileTypeRead = true;
            FileVersion = (FileVersion)b.ReadUInt32();    // 0x744 0x745
            fileVersionRead = true;
            ChunkTableOffset = b.ReadInt32() + 4;
            chunkTableOffsetRead = true;
            NumChunks = b.ReadUInt32();       // number of Chunks in the chunk table
            declaredChunkCountRead = true;

            return;
        }

        throw new NotSupportedException($"Unsupported File Signature {FileSignature}");
    }

    private void CreateDummyRootNode()
    {
        ChunkNode rootNode = new ChunkNode_823
        {
            Name = Path.GetFileNameWithoutExtension(FileName!),
            ObjectNodeID = 2,      // No node IDs in #ivo skin files.  The actual mesh is the only node in the m file.
            ParentNodeID = ~0,     // No parent
            NumChildren = 0,     // Single object
            MaterialID = 11,
            Transform = Matrix4x4.Identity,
            ChunkType = ChunkType.Node,
            ID = 1
        };
        RootNode = rootNode;
        rootNode._model = this;
        ChunkMap.Add(1, rootNode);
    }

    private FileHeaderObservation CreateFileHeaderObservation(bool complete) =>
        new(
            FileSignature,
            fileTypeRead ? (uint)FileType : null,
            fileVersionRead ? (uint)FileVersion : null,
            declaredChunkCountRead ? NumChunks : null,
            chunkTableOffsetRead ? ChunkTableOffset : null,
            complete);

    private void ReadChunkTable(BinaryReader b, IModelReadObserver? observer)
    {
        b.BaseStream.Seek(ChunkTableOffset, SeekOrigin.Begin);

        // For 0x744-format CAF animation files the NumChunks header field is unreliable
        // (it contains frame count or other unrelated data rather than the chunk count).
        // Compute the actual count from the bytes remaining at the chunk table position,
        // since ChunkHeader_744 is exactly 16 bytes.  Other 0x744 file types (.cga, .cgf,
        // .chr, .skin) have a correct NumChunks in their header, so we leave those alone.
        if (FileVersion == FileVersion.x0744 &&
            string.Equals(Path.GetExtension(FileName), ".caf", StringComparison.OrdinalIgnoreCase))
        {
            var availableBytes = b.BaseStream.Length - b.BaseStream.Position;
            NumChunks = (uint)(availableBytes / 16);
        }

        for (int i = 0; i < NumChunks; i++)
        {
            ChunkHeader header = Chunk.New<ChunkHeader>((uint)FileVersion);
            header.Read(b);

            // Version 0x900 stores no chunk ID. Preserve the existing random surrogate during normal
            // conversion, but use a stable negative surrogate for diagnostics so large scans cannot
            // exhaust the process-global random-ID pool or accidentally match ordinary material IDs.
            if (FileVersion == FileVersion.x0900)
                header.ID = observer is null
                    ? Chunk.GetNextRandom()
                    : checked(int.MinValue + i);

            chunkHeaders.Add(header);
        }

        // Set sizes for versions that don't have sizes (x0744 and x0900 chunk headers don't include size)
        // Chunks are stored in offset order in the chunk table, so we can calculate sizes directly
        if (FileVersion == FileVersion.x0744 || FileVersion == FileVersion.x0900)
        {
            for (int i = 0; i < chunkHeaders.Count - 1; i++)
            {
                chunkHeaders[i].Size = (uint)(chunkHeaders[i + 1].Offset - chunkHeaders[i].Offset);
            }
        }
    }

    private void ReadChunks(BinaryReader reader, IModelReadObserver? observer)
    {
        var recoverStreamPosition = false;

        for (var tableIndex = 0; tableIndex < chunkHeaders.Count; tableIndex++)
        {
            var chunkHeaderItem = chunkHeaders[tableIndex];
            var sizeKnown = (FileVersion != FileVersion.x0744
                && FileVersion != FileVersion.x0900)
                || tableIndex < chunkHeaders.Count - 1;
            var occurrence = new ChunkOccurrence(
                tableIndex,
                (uint)chunkHeaderItem.ChunkType,
                chunkHeaderItem.VersionRaw,
                chunkHeaderItem.Offset,
                sizeKnown ? chunkHeaderItem.Size : null,
                FileVersion == FileVersion.x0900 ? null : chunkHeaderItem.ID);

            Utilities.HelperMethods.Log(Utilities.LogLevelEnum.Debug,
                $"[{Path.GetFileName(FileName)}] Reading chunk: Type={chunkHeaderItem.ChunkType}, Version=0x{chunkHeaderItem.Version:X}, ID={chunkHeaderItem.ID}, Offset=0x{chunkHeaderItem.Offset:X}, Size={chunkHeaderItem.Size}");

            Chunk chunk;
            try
            {
                chunk = Chunk.New(chunkHeaderItem.ChunkType, chunkHeaderItem.Version);
            }
            catch (Exception exception) when (observer is not null && IsObservableDiagnosticException(exception))
            {
                var classification = exception is NotSupportedException
                    ? ReadFailureClassification.UnsupportedVersion
                    : ClassifyReadFailure(exception);
                observer.OnChunkFailure(
                    occurrence,
                    ModelReadStage.ChunkFactory,
                    classification,
                    exception);

                if (MustAbortDiagnosticRead(classification))
                    throw;

                recoverStreamPosition = true;
                continue;
            }

            if (observer is not null)
            {
                observer.OnChunkClassified(
                    occurrence,
                    ChunkFactoryClassifier.Classify(chunkHeaderItem.ChunkType, chunk));
            }

            var stage = ModelReadStage.ChunkRead;
            var hadPreviousChunk = ChunkMap.TryGetValue(chunkHeaderItem.ID, out var previousChunk);
            try
            {
                ChunkMap[chunkHeaderItem.ID] = chunk;
                chunk.Load(this, chunkHeaderItem);

                if (observer is not null && recoverStreamPosition)
                {
                    reader.BaseStream.Seek(chunkHeaderItem.Offset, SeekOrigin.Begin);
                    ((EndiannessChangeableBinaryReader)reader).IsBigEndian =
                        (chunkHeaderItem.VersionRaw & 0x80000000u) != 0;
                    recoverStreamPosition = false;
                }
                activeObserver = observer;
                activeOccurrence = observer is null ? null : occurrence;
                try
                {
                    chunk.Read(reader);

                    // Ensure we read to end of structure
                    stage = ModelReadStage.ChunkSkip;
                    chunk.SkipBytes(reader);
                }
                finally
                {
                    activeObserver = null;
                    activeOccurrence = null;
                }
            }
            catch (Exception exception) when (observer is not null && IsObservableDiagnosticException(exception))
            {
                if (hadPreviousChunk)
                    ChunkMap[chunkHeaderItem.ID] = previousChunk!;
                else
                    ChunkMap.Remove(chunkHeaderItem.ID);

                var classification = ClassifyReadFailure(exception);
                observer.OnChunkFailure(
                    occurrence,
                    stage,
                    classification,
                    exception);

                if (MustAbortDiagnosticRead(classification))
                    throw;

                recoverStreamPosition = true;
                continue;
            }

            // Assume first node read in Model[0] is root node.  This may be bad if they aren't in order!
            if (chunkHeaderItem.ChunkType == ChunkType.Node && RootNode == null)
                RootNode = chunk as ChunkNode;

            // Add Bones to the model.  We are assuming there is only one CompiledBones chunk per file.
            if (chunkHeaderItem.ChunkType == ChunkType.CompiledBones ||
                chunkHeaderItem.ChunkType == ChunkType.CompiledBonesSC ||
                chunkHeaderItem.ChunkType == ChunkType.CompiledBones_Ivo ||
                chunkHeaderItem.ChunkType == ChunkType.CompiledBones_Ivo2)
            {
                Bones = chunk as ChunkCompiledBones;
            }
        }
    }

    private static ReadFailureClassification ClassifyReadFailure(Exception exception) =>
        exception switch
        {
            OutOfMemoryException => ReadFailureClassification.ResourceExhaustion,
            IOException when exception is not EndOfStreamException => ReadFailureClassification.IoFailure,
            _ => ReadFailureClassification.ParseFailure,
        };

    private static bool MustAbortDiagnosticRead(ReadFailureClassification classification) =>
        classification is ReadFailureClassification.IoFailure
            or ReadFailureClassification.ResourceExhaustion;

    private static bool IsObservableDiagnosticException(Exception exception) =>
        exception is not OperationCanceledException
            and not AccessViolationException
            and not StackOverflowException;
}
