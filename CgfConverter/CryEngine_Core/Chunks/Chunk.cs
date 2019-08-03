using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CgfConverter.CryEngine_Core
{
    public abstract class Chunk : IBinaryChunk
    {

        private static Dictionary<Type, Dictionary<UInt32, Func<dynamic>>> _chunkFactoryCache = new Dictionary<Type, Dictionary<UInt32, Func<dynamic>>> { };
        
        public static Chunk New(ChunkTypeEnum chunkType, UInt32 version)
        {
            switch (chunkType)
            {
                case ChunkTypeEnum.SourceInfo:
                    return Chunk.New<ChunkSourceInfo>(version);
                case ChunkTypeEnum.Timing:
                    return Chunk.New<ChunkTimingFormat>(version);
                case ChunkTypeEnum.ExportFlags:
                    return Chunk.New<ChunkExportFlags>(version);
                case ChunkTypeEnum.MtlName:
                    return Chunk.New<ChunkMtlName>(version);
                case ChunkTypeEnum.DataStream:
                    return Chunk.New<ChunkDataStream>(version);
                case ChunkTypeEnum.Mesh:
                    return Chunk.New<ChunkMesh>(version);
                case ChunkTypeEnum.MeshSubsets:
                    return Chunk.New<ChunkMeshSubsets>(version);
                case ChunkTypeEnum.Node:
                    return Chunk.New<ChunkNode>(version);
                case ChunkTypeEnum.Helper:
                    return Chunk.New<ChunkHelper>(version);
                case ChunkTypeEnum.Controller:
                    return Chunk.New<ChunkController>(version);
                case ChunkTypeEnum.SceneProps:
                    return Chunk.New<ChunkSceneProp>(version);
                case ChunkTypeEnum.MeshPhysicsData:
                    return Chunk.New<ChunkMeshPhysicsData>(version);
                case ChunkTypeEnum.BoneAnim:
                    return Chunk.New<ChunkBoneAnim>(version);
                // Compiled chunks
                case ChunkTypeEnum.CompiledBones:
                    return Chunk.New<ChunkCompiledBones>(version);
                case ChunkTypeEnum.CompiledPhysicalProxies:
                    return Chunk.New<ChunkCompiledPhysicalProxies>(version);
                case ChunkTypeEnum.CompiledPhysicalBones:
                    return Chunk.New<ChunkCompiledPhysicalBones>(version);
                case ChunkTypeEnum.CompiledIntSkinVertices:
                    return Chunk.New<ChunkCompiledIntSkinVertices>(version);
                case ChunkTypeEnum.CompiledMorphTargets:
                    return Chunk.New<ChunkCompiledMorphTargets>(version);
                case ChunkTypeEnum.CompiledExt2IntMap:
                    return Chunk.New<ChunkCompiledExtToIntMap>(version);
                case ChunkTypeEnum.CompiledIntFaces:
                    return Chunk.New<ChunkCompiledIntFaces>(version);
                // Star Citizen equivalents
                case ChunkTypeEnum.CompiledBonesSC:
                    return Chunk.New<ChunkCompiledBones>(version);
                case ChunkTypeEnum.CompiledPhysicalBonesSC:
                    return Chunk.New<ChunkCompiledPhysicalBones>(version);
                case ChunkTypeEnum.CompiledExt2IntMapSC:
                    return Chunk.New<ChunkCompiledExtToIntMap>(version);
                case ChunkTypeEnum.CompiledIntFacesSC:
                    return Chunk.New<ChunkCompiledIntFaces>(version);
                case ChunkTypeEnum.CompiledIntSkinVerticesSC:
                    return Chunk.New<ChunkCompiledIntSkinVertices>(version);
                case ChunkTypeEnum.CompiledMorphTargetsSC:
                    return Chunk.New<ChunkCompiledMorphTargets>(version);
                case ChunkTypeEnum.CompiledPhysicalProxiesSC:
                    return Chunk.New<ChunkCompiledPhysicalProxies>(version);
                // Old chunks
                case ChunkTypeEnum.BoneNameList:
                    return Chunk.New<ChunkBoneNameList>(version);
                case ChunkTypeEnum.MeshMorphTarget:
                    return Chunk.New<ChunkMeshMorphTargets>(version);
                case ChunkTypeEnum.Mtl:
                    //Utils.Log(LogLevelEnum.Debug, "Mtl Chunk here");  // Obsolete.  Not used
                default:
                    return new ChunkUnknown();
            }
        }

        public static T New<T>(UInt32 version) where T : Chunk
        {
            Dictionary<UInt32, Func<dynamic>> versionMap = null;
            Func<dynamic> factory = null;

            if (!_chunkFactoryCache.TryGetValue(typeof(T), out versionMap))
            {
                versionMap = new Dictionary<UInt32, Func<dynamic>> { };
                _chunkFactoryCache[typeof(T)] = versionMap;
            }

            if (!versionMap.TryGetValue(version, out factory))
            {
                var targetType = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                  where !type.IsAbstract
                                  where type.IsClass
                                  where !type.IsGenericType
                                  where typeof(T).IsAssignableFrom(type)
                                  where type.Name == String.Format("{0}_{1:X}", typeof(T).Name, version)
                                  select type).FirstOrDefault();

                if (targetType != null)
                {
                    factory = () => { return Activator.CreateInstance(targetType) as T; };
                }

                _chunkFactoryCache[typeof(T)][version] = factory;
            }

            if (factory != null)
            {
                return factory.Invoke() as T;
            }

            throw new NotSupportedException(String.Format("Version {0:X} of {1} is not supported", version, typeof(T).Name));
        }

        public void Load(CryEngine_Core.Model model, ChunkHeader header)
        {
            this._model = model;
            this._header = header;
        }

        internal ChunkHeader _header;
        internal CryEngine_Core.Model _model;

        /// <summary>
        /// Position of the start of the chunk
        /// </summary>
        public UInt32 Offset { get; internal set; }
        /// <summary>
        /// The Type of the Chunk
        /// </summary>
        public ChunkTypeEnum ChunkType { get; internal set; }
        /// <summary>
        /// The Version of this Chunk
        /// </summary>
        public UInt32 Version;
        /// <summary>
        /// The ID of this Chunk
        /// </summary>
        public int ID;
        /// <summary>
        /// The Size of this Chunk (in Bytes)
        /// </summary>
        public UInt32 Size;
        /// <summary>
        /// Size of the data in the chunk.  This is the chunk size, minus the header (if there is one)
        /// </summary>
        public UInt32 DataSize { get; set; }

        public Dictionary<Int64, Byte> SkippedBytes = new Dictionary<Int64, Byte> { };

        public void SkipBytes(BinaryReader reader, Int64? bytesToSkip = null)
        {
            if (reader == null)
                return;

            if ((reader.BaseStream.Position > this.Offset + this.Size) && (this.Size > 0))
                Utils.Log(LogLevelEnum.Debug, "Buffer Overflow in {2} 0x{0:X} ({1} bytes)", this.ID, reader.BaseStream.Position - this.Offset - this.Size, this.GetType().Name);

            if (reader.BaseStream.Length < this.Offset + this.Size)
                Utils.Log(LogLevelEnum.Debug, "Corrupt Headers in {1} 0x{0:X}", this.ID, this.GetType().Name);

            if (!bytesToSkip.HasValue)
                bytesToSkip = (Int64)(this.Size - Math.Max(reader.BaseStream.Position - this.Offset, 0));

            for (Int64 i = 0; i < bytesToSkip; i++)
            {
                this.SkippedBytes[reader.BaseStream.Position - this.Offset] = reader.ReadByte();

                // if (this.SkippedBytes[reader.BaseStream.Position - this.Offset - 1] == 0)
                //     this.SkippedBytes.Remove(reader.BaseStream.Position - this.Offset - 1);
            }
        }

        public virtual void Read(BinaryReader reader)
        {
            this.ChunkType = this._header.ChunkType;
            this.Version = this._header.Version;
            this.Offset = this._header.Offset;
            this.ID = this._header.ID;
            this.Size = this._header.Size;
            this.DataSize = this.Size;          // For SC files, there is no header in chunks.  But need Datasize to calculate things.

            reader.BaseStream.Seek(this._header.Offset, 0);

            // Star Citizen files don't have the type, version, offset and ID at the start of a chunk, so don't read them.
            if (this._model.FileVersion == FileVersionEnum.CryTek_3_4 || this._model.FileVersion == FileVersionEnum.CryTek_3_5)
            {
                this.ChunkType = (ChunkTypeEnum)Enum.ToObject(typeof(ChunkTypeEnum), reader.ReadUInt32());
                this.Version = reader.ReadUInt32();
                this.Offset = reader.ReadUInt32();
                this.ID = reader.ReadInt32();
                this.DataSize = this.Size - 16;
            }

            if (this.Offset != this._header.Offset || this.Size != this._header.Size)
            {
                Utils.Log(LogLevelEnum.Warning, "Conflict in chunk definition");
                Utils.Log(LogLevelEnum.Warning, "{0:X}+{1:X}", this._header.Offset, this._header.Size);
                Utils.Log(LogLevelEnum.Warning, "{0:X}+{1:X}", this.Offset, this.Size);
            }
        }

        /// <summary>
        /// Gets a link to the SkinningInfo model.
        /// </summary>
        /// <returns>Link to the SkinningInfo model.</returns>
        public SkinningInfo GetSkinningInfo()
        {
            if (this._model.SkinningInfo == null)
            {
                this._model.SkinningInfo = new SkinningInfo();
            }
            return this._model.SkinningInfo;
        }

        public virtual void Write(BinaryWriter writer) { throw new NotImplementedException(); }

        public virtual void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** CHUNK ***");
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType: {0}", this.ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    ChunkVersion: {0:X}", this.Version);
            Utils.Log(LogLevelEnum.Verbose, "    Offset: {0:X}", this.Offset);
            Utils.Log(LogLevelEnum.Verbose, "    ID: {0:X}", this.ID);
            Utils.Log(LogLevelEnum.Verbose, "    Size: {0:X}", this.Size);
            Utils.Log(LogLevelEnum.Verbose, "*** END CHUNK ***");
        }
    }
}
