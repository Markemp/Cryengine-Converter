using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CgfConverter.Models;
using CgfConverter.Services;
using CgfConverter.Utilities;

namespace CgfConverter.CryEngineCore;

public abstract class Chunk : IBinaryChunk
{
    protected static Random rnd = new();
    protected static HashSet<int> alreadyPickedRandoms = new();

    private readonly static Dictionary<Type, Dictionary<uint, Func<Chunk>>> _chunkFactoryCache = new() { };

    internal ChunkHeader _header;
    internal Model _model;

    internal uint VersionRaw;
    internal bool IsBigEndian => (VersionRaw & 0x80000000u) != 0;
    internal uint Version => VersionRaw & 0x7fffffffu;
    internal int ID;
    internal uint Size;

    public uint Offset { get; internal set; }
    public ChunkType ChunkType { get; internal set; }
    public uint DataSize { get; set; }

    public static Chunk New(ChunkType chunkType, uint version) =>
        CreateForRead(chunkType, version).Chunk;

    internal static ChunkCreationResult CreateForRead(ChunkType chunkType, uint version)
    {
        return chunkType switch
        {
            ChunkType.SourceInfo => CreateImplemented<ChunkSourceInfo>(version),
            ChunkType.Timing => CreateImplemented<ChunkTimingFormat>(version),
            ChunkType.ExportFlags => CreateImplemented<ChunkExportFlags>(version),
            ChunkType.MtlName => CreateImplemented<ChunkMtlName>(version),
            ChunkType.DataStream => CreateImplemented<ChunkDataStream>(version),
            ChunkType.Mesh => CreateImplemented<ChunkMesh>(version),
            ChunkType.MeshSubsets => CreateImplemented<ChunkMeshSubsets>(version),
            ChunkType.Node => CreateImplemented<ChunkNode>(version),
            ChunkType.Helper => CreateImplemented<ChunkHelper>(version),
            ChunkType.Controller => CreateImplemented<ChunkController>(version),
            ChunkType.SceneProps => CreateImplemented<ChunkSceneProp>(version),
            ChunkType.MeshPhysicsData => CreateImplemented<ChunkMeshPhysicsData>(version),
            ChunkType.BoneAnim => CreateImplemented<ChunkBoneAnim>(version),
            ChunkType.MotionParams => CreateImplemented<ChunkMotionParameters>(version),
            // Compiled chunks
            ChunkType.CompiledBones => CreateImplemented<ChunkCompiledBones>(version),
            ChunkType.CompiledPhysicalProxies => CreateImplemented<ChunkCompiledPhysicalProxies>(version),
            ChunkType.CompiledPhysicalBones => CreateImplemented<ChunkCompiledPhysicalBones>(version),
            ChunkType.CompiledIntSkinVertices => CreateImplemented<ChunkCompiledIntSkinVertices>(version),
            ChunkType.CompiledMorphTargets => CreateImplemented<ChunkCompiledMorphTargets>(version),
            ChunkType.CompiledExt2IntMap => CreateImplemented<ChunkCompiledExtToIntMap>(version),
            ChunkType.CompiledIntFaces => CreateImplemented<ChunkCompiledIntFaces>(version),
            // Star Citizen equivalents
            ChunkType.CompiledBonesSC => CreateImplemented<ChunkCompiledBones>(version),
            ChunkType.CompiledPhysicalBonesSC => CreateImplemented<ChunkCompiledPhysicalBones>(version),
            ChunkType.CompiledExt2IntMapSC => CreateImplemented<ChunkCompiledExtToIntMap>(version),
            ChunkType.CompiledIntFacesSC => CreateImplemented<ChunkCompiledIntFaces>(version),
            ChunkType.CompiledIntSkinVerticesSC => CreateImplemented<ChunkCompiledIntSkinVertices>(version),
            ChunkType.CompiledMorphTargetsSC => CreateImplemented<ChunkCompiledMorphTargets>(version),
            ChunkType.CompiledPhysicalProxiesSC => CreateImplemented<ChunkCompiledPhysicalProxies>(version),
            // SC IVO chunks
            ChunkType.MtlNameIvo => CreateImplemented<ChunkMtlName>(version),
            ChunkType.MtlNameIvo320 => CreateImplemented<ChunkMtlName>(version),
            ChunkType.CompiledBones_Ivo => CreateImplemented<ChunkCompiledBones>(version),
            ChunkType.CompiledBones_Ivo2 => CreateImplemented<ChunkCompiledBones>(version),
            ChunkType.MeshIvo => CreateImplemented<ChunkMesh>(version),
            ChunkType.MeshInfo => CreateImplemented<ChunkMesh>(version),
            ChunkType.IvoSkin2 => CreateImplemented<ChunkIvoSkinMesh>(version),
            ChunkType.IvoSkin => CreateImplemented<ChunkIvoSkinMesh>(version),
            ChunkType.NodeMeshCombo => CreateImplemented<ChunkNodeMeshCombo>(version),
            // Old chunks
            ChunkType.BoneNameList => CreateImplemented<ChunkBoneNameList>(version),
            ChunkType.MeshMorphTarget => CreateImplemented<ChunkMeshMorphTargets>(version),
            ChunkType.BinaryXmlDataSC => CreateImplemented<ChunkBinaryXmlData>(version),
            // Star Citizen #ivo animation chunks
            ChunkType.IvoCAFData => CreateImplemented<ChunkIvoCAF>(version),
            ChunkType.IvoAnimInfo => CreateImplemented<ChunkIvoAnimInfo>(version),
            ChunkType.IvoDBAData => CreateImplemented<ChunkIvoDBAData>(version),
            ChunkType.IvoDBAMetadata => CreateImplemented<ChunkIvoDBAMetadata>(version),
            // Star Citizen 4.5+ new chunk types - safely skipped (metadata/LOD data not needed for rendering)
            ChunkType.IvoAssetMetadata => CreateIntentionallyIgnored(),
            ChunkType.IvoLodDistances => CreateIntentionallyIgnored(),
            ChunkType.IvoLodMeshData => CreateIntentionallyIgnored(),
            ChunkType.IvoBoundingData => CreateIntentionallyIgnored(),
            ChunkType.IvoChunkTerminator => CreateIntentionallyIgnored(),
            ChunkType.IvoMtlNameVariant => CreateIntentionallyIgnored(),
            _ => CreateUnknown(chunkType),
        };
    }

    private static ChunkCreationResult CreateImplemented<T>(uint version) where T : Chunk =>
        new(New<T>(version), ChunkFactoryClassification.ParserAvailable);

    private static ChunkCreationResult CreateIntentionallyIgnored() =>
        new(new ChunkUnknown(), ChunkFactoryClassification.IntentionallyIgnored);

    private static ChunkCreationResult CreateUnknown(ChunkType chunkType) =>
        new(
            new ChunkUnknown(),
            Enum.IsDefined(typeof(ChunkType), chunkType)
                ? ChunkFactoryClassification.EnumDefinedUnimplemented
                : ChunkFactoryClassification.RawUnknown);

    public static T New<T>(uint version) where T : Chunk
    {
        Func<Chunk> factory;

        lock (_chunkFactoryCache)
        {
            if (!_chunkFactoryCache.TryGetValue(typeof(T), out var versionMap))
            {
                versionMap = new Dictionary<uint, Func<Chunk>> { };
                _chunkFactoryCache[typeof(T)] = versionMap;
            }

            version &= 0x7fffffff;
            if (!versionMap.TryGetValue(version, out factory))
            {
                var targetType = typeof(T).Assembly.GetTypes()
                    .FirstOrDefault(type =>
                        !type.IsAbstract
                        && type.IsClass
                        && !type.IsGenericType
                        && typeof(T).IsAssignableFrom(type)
                        && type.Name == string.Format("{0}_{1:X}", typeof(T).Name, version));

                if (targetType != null)
                    factory = () => Activator.CreateInstance(targetType) as T;

                _chunkFactoryCache[typeof(T)][version] = factory;
            }
        }

        if (factory != null)
            return factory.Invoke() as T;

        throw new NotSupportedException($"Version {version:X} of {typeof(T).Name} is not supported");
    }

    public void Load(Model model, ChunkHeader header)
    {
        _model = model;
        _header = header;
    }

    public void SkipBytes(BinaryReader reader, long? bytesToSkip = null)
    {
        if (reader is null)
            return;

        if ((reader.BaseStream.Position > Offset + Size) && (Size > 0))
            HelperMethods.Log(LogLevelEnum.Debug, "Buffer Overflow in {2} 0x{0:X} ({1} bytes)", ID, reader.BaseStream.Position - Offset - Size, GetType().Name);

        if (reader.BaseStream.Length < Offset + Size)
            HelperMethods.Log(LogLevelEnum.Debug, "Corrupt Headers in {1} 0x{0:X}", ID, GetType().Name);

        if (!bytesToSkip.HasValue)
            bytesToSkip = (long)(Size - Math.Max(reader.BaseStream.Position - Offset, 0));

        reader.BaseStream.Seek((long)(bytesToSkip), SeekOrigin.Current);
    }

    public virtual void Read(BinaryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        ChunkType = _header.ChunkType;
        VersionRaw = _header.VersionRaw;
        Offset = _header.Offset;
        ID = _header.ID;
        Size = _header.Size;
        DataSize = Size;    // For SC files, there is no header in chunks.  But need Datasize to calculate things.

        reader.BaseStream.Seek(_header.Offset, SeekOrigin.Begin);

        var swappableReader = (EndiannessChangeableBinaryReader)reader;

        // Star Citizen files don't have the type, version, offset and ID at the start of a chunk, so don't read them.
        if (_model.FileVersion == FileVersion.x0744 || _model.FileVersion == FileVersion.x0745)
        {
            // This part is always in little endian.
            swappableReader.IsBigEndian = false;

            ChunkType = (ChunkType)reader.ReadUInt32();
            VersionRaw = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            ID = reader.ReadInt32();
            DataSize = Size - 16;
        }

        if (ChunkType != _header.ChunkType ||
            VersionRaw != _header.VersionRaw ||
            Offset != _header.Offset ||
            ID != _header.ID)
        {
            HelperMethods.Log(LogLevelEnum.Debug, "Conflict in chunk definition");
            HelperMethods.Log(LogLevelEnum.Debug, "ChunkType=header({0:X}) vs header2({1:X})", (uint)_header.ChunkType, (uint)ChunkType);
            HelperMethods.Log(LogLevelEnum.Debug, "VersionRaw=VersionRaw({0:X}) vs header2({1:X})", _header.VersionRaw, VersionRaw);
            HelperMethods.Log(LogLevelEnum.Debug, "Offset=header({0:X}) vs header2({1:X})", _header.Offset, Offset);
            HelperMethods.Log(LogLevelEnum.Debug, "ID=header({0:X}) vs header2({1:X})", _header.ID, ID);
            HelperMethods.Log(LogLevelEnum.Debug, "ChunkType=header({0:X}) vs header2({1:X})", _header.ChunkType, ChunkType);
        }

        // Remainder of the chunk is in whichever endianness specified from version.
        swappableReader.IsBigEndian = IsBigEndian;
    }

    public virtual void Write(BinaryWriter writer) { throw new NotImplementedException(); }

    public override string ToString() => $@"Chunk Type: {ChunkType}, Ver: {Version:X}, Offset: {Offset:X}, ID: {ID:X}, Size: {Size}";

    public static int GetNextRandom()
    {
        bool available = false;
        int rand = 0;
        while (!available) {
            rand = rnd.Next(100000);
            if (!alreadyPickedRandoms.Contains(rand))
            {
                alreadyPickedRandoms.Add(rand);
                available = true;
            }
        }
        return rand;
    }
}
