using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter.CryEngineCore;

public abstract class Chunk : IBinaryChunk
{
    protected static Random rnd = new();
    protected static HashSet<int> alreadyPickedRandoms = new();

    private readonly static Dictionary<Type, Dictionary<uint, Func<dynamic>>> _chunkFactoryCache = new() { };

    internal ChunkHeader _header;
    internal Model _model;

    public uint Offset { get; internal set; }
    public ChunkType ChunkType { get; internal set; }
    internal uint Version;
    internal int ID;
    internal uint Size;
    public uint DataSize { get; set; }

    internal Dictionary<long, byte> SkippedBytes = new Dictionary<long, byte> { };

    public static Chunk New(ChunkType chunkType, uint version)
    {
        return chunkType switch
        {
            ChunkType.SourceInfo => Chunk.New<ChunkSourceInfo>(version),
            ChunkType.Timing => Chunk.New<ChunkTimingFormat>(version),
            ChunkType.ExportFlags => Chunk.New<ChunkExportFlags>(version),
            ChunkType.MtlName => Chunk.New<ChunkMtlName>(version),
            ChunkType.DataStream => Chunk.New<ChunkDataStream>(version),
            ChunkType.Mesh => Chunk.New<ChunkMesh>(version),
            ChunkType.MeshSubsets => Chunk.New<ChunkMeshSubsets>(version),
            ChunkType.Node => Chunk.New<ChunkNode>(version),
            ChunkType.Helper => Chunk.New<ChunkHelper>(version),
            ChunkType.Controller => Chunk.New<ChunkController>(version),
            ChunkType.SceneProps => Chunk.New<ChunkSceneProp>(version),
            ChunkType.MeshPhysicsData => Chunk.New<ChunkMeshPhysicsData>(version),
            ChunkType.BoneAnim => Chunk.New<ChunkBoneAnim>(version),
            // Compiled chunks
            ChunkType.CompiledBones => Chunk.New<ChunkCompiledBones>(version),
            ChunkType.CompiledPhysicalProxies => Chunk.New<ChunkCompiledPhysicalProxies>(version),
            ChunkType.CompiledPhysicalBones => Chunk.New<ChunkCompiledPhysicalBones>(version),
            ChunkType.CompiledIntSkinVertices => Chunk.New<ChunkCompiledIntSkinVertices>(version),
            ChunkType.CompiledMorphTargets => Chunk.New<ChunkCompiledMorphTargets>(version),
            ChunkType.CompiledExt2IntMap => Chunk.New<ChunkCompiledExtToIntMap>(version),
            ChunkType.CompiledIntFaces => Chunk.New<ChunkCompiledIntFaces>(version),
            // Star Citizen equivalents
            ChunkType.CompiledBonesSC => Chunk.New<ChunkCompiledBones>(version),
            ChunkType.CompiledPhysicalBonesSC => Chunk.New<ChunkCompiledPhysicalBones>(version),
            ChunkType.CompiledExt2IntMapSC => Chunk.New<ChunkCompiledExtToIntMap>(version),
            ChunkType.CompiledIntFacesSC => Chunk.New<ChunkCompiledIntFaces>(version),
            ChunkType.CompiledIntSkinVerticesSC => Chunk.New<ChunkCompiledIntSkinVertices>(version),
            ChunkType.CompiledMorphTargetsSC => Chunk.New<ChunkCompiledMorphTargets>(version),
            ChunkType.CompiledPhysicalProxiesSC => Chunk.New<ChunkCompiledPhysicalProxies>(version),
            // SC IVO chunks
            ChunkType.MtlNameIvo => Chunk.New<ChunkMtlName>(version),
            ChunkType.CompiledBonesIvo => Chunk.New<ChunkCompiledBones>(version),
            ChunkType.MeshIvo => Chunk.New<ChunkMesh>(version),
            ChunkType.IvoSkin => Chunk.New<ChunkIvoSkin>(version),
            // Old chunks
            ChunkType.BoneNameList => Chunk.New<ChunkBoneNameList>(version),
            ChunkType.MeshMorphTarget => Chunk.New<ChunkMeshMorphTargets>(version),
            ChunkType.BinaryXmlDataSC => Chunk.New<ChunkBinaryXmlData>(version),
            _ => new ChunkUnknown(),
        };
    }

    public static T New<T>(uint version) where T : Chunk
    {
        Dictionary<uint, Func<dynamic>> versionMap = null;
        Func<dynamic> factory = null;

        if (!_chunkFactoryCache.TryGetValue(typeof(T), out versionMap))
        {
            versionMap = new Dictionary<uint, Func<dynamic>> { };
            _chunkFactoryCache[typeof(T)] = versionMap;
        }

        if (!versionMap.TryGetValue(version, out factory))
        {
            var targetType = typeof(T).Assembly.GetTypes()
                .FirstOrDefault(type =>
                              !type.IsAbstract
                              && type.IsClass
                              && !type.IsGenericType
                              && typeof(T).IsAssignableFrom(type)
                              && type.Name == String.Format("{0}_{1:X}", typeof(T).Name, version));

            if (targetType != null)
                factory = () => { return Activator.CreateInstance(targetType) as T; };

            _chunkFactoryCache[typeof(T)][version] = factory;
        }

        if (factory != null)
            return factory.Invoke() as T;

        throw new NotSupportedException(string.Format("Version {0:X} of {1} is not supported", version, typeof(T).Name));
    }

    public void Load(Model model, ChunkHeader header)
    {
        _model = model;
        _header = header;
    }

    public void SkipBytes(BinaryReader reader, long? bytesToSkip = null)
    {
        if (reader == null)
            return;

        if ((reader.BaseStream.Position > Offset + Size) && (Size > 0))
            Utils.Log(LogLevelEnum.Debug, "Buffer Overflow in {2} 0x{0:X} ({1} bytes)", ID, reader.BaseStream.Position - Offset - Size, GetType().Name);

        if (reader.BaseStream.Length < Offset + Size)
            Utils.Log(LogLevelEnum.Debug, "Corrupt Headers in {1} 0x{0:X}", ID, GetType().Name);

        if (!bytesToSkip.HasValue)
            bytesToSkip = (long)(Size - Math.Max(reader.BaseStream.Position - Offset, 0));

        for (long i = 0; i < bytesToSkip; i++)
        {
            SkippedBytes[reader.BaseStream.Position - Offset] = reader.ReadByte();
        }
    }

    public virtual void Read(BinaryReader reader)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        ChunkType = _header.ChunkType;
        Version = _header.Version;
        Offset = _header.Offset;
        ID = _header.ID;
        Size = _header.Size;
        DataSize = Size;          // For SC files, there is no header in chunks.  But need Datasize to calculate things.

        reader.BaseStream.Seek(_header.Offset, SeekOrigin.Begin);

        // Star Citizen files don't have the type, version, offset and ID at the start of a chunk, so don't read them.
        if (_model.FileVersion == FileVersion.CryTek_3_4 || _model.FileVersion == FileVersion.CryTek_3_5)
        {
            ChunkType = (ChunkType)reader.ReadUInt32();
            Version = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            ID = reader.ReadInt32();
            DataSize = Size - 16;
        }

        if (Offset != _header.Offset || Size != _header.Size)
        {
            Utils.Log(LogLevelEnum.Debug, "Conflict in chunk definition");
            Utils.Log(LogLevelEnum.Debug, "{0:X}+{1:X}", _header.Offset, _header.Size);
            Utils.Log(LogLevelEnum.Debug, "{0:X}+{1:X}", Offset, Size);
        }
    }

    /// <summary>Gets a link to the SkinningInfo model.</summary>
    /// <returns>Link to the SkinningInfo model.</returns>
    public SkinningInfo GetSkinningInfo()
    {
        if (_model.SkinningInfo == null)
            _model.SkinningInfo = new SkinningInfo();
        
        return _model.SkinningInfo;
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
