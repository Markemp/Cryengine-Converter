using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                case ChunkTypeEnum.Mtl:
                    //Console.WriteLine("Mtl Chunk here");  // Obsolete.  Not used?
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
                case ChunkTypeEnum.CompiledBones:
                    return Chunk.New<ChunkCompiledBones>(version);
                case ChunkTypeEnum.Helper:
                    return Chunk.New<ChunkHelper>(version);
                case ChunkTypeEnum.Controller:
                    return Chunk.New<ChunkController>(version);
                case ChunkTypeEnum.SceneProps:
                    return Chunk.New<ChunkSceneProp>(version);
                case ChunkTypeEnum.CompiledPhysicalProxies:
                    return Chunk.New<ChunkCompiledPhysicalProxies>(version);
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

        public void Load(CryEngine.Model model, ChunkHeader header)
        {
            this._model = model;
            this._header = header;
        }

        internal ChunkHeader _header;
        internal CryEngine.Model _model;

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
        public UInt32 ID;
        /// <summary>
        /// The Size of this Chunk (in Bytes)
        /// </summary>
        public UInt32 Size;

        public Dictionary<Int64, Byte> SkippedBytes = new Dictionary<Int64, Byte> { };

        public void SkipBytes(BinaryReader reader, Int64? bytesToSkip = null)
        {
            if (reader == null)
                return;

            if (reader.BaseStream.Position > this.Offset + this.Size)
                Console.WriteLine("Buffer Overflow in {2} 0x{0:X} ({1} bytes)", this.ID, reader.BaseStream.Position - this.Offset - this.Size, this.GetType().Name);

            if (reader.BaseStream.Length < this.Offset + this.Size)
                Console.WriteLine("Corrupt Headers in {1} 0x{0:X}", this.ID, this.GetType().Name);

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

            reader.BaseStream.Seek(this._header.Offset, 0);

            if (this.ChunkType == ChunkTypeEnum.SourceInfo && CryEngine.Model.FILE_VERSION == FileVersionEnum.CryTek_3_5)
                return;

            if (CryEngine.Model.FILE_VERSION == FileVersionEnum.CryTek_3_4 || CryEngine.Model.FILE_VERSION == FileVersionEnum.CryTek_3_5)
            {
                this.ChunkType = (ChunkTypeEnum)Enum.ToObject(typeof(ChunkTypeEnum), reader.ReadUInt32());
                this.Version = reader.ReadUInt32();
                this.Offset = reader.ReadUInt32();
                this.ID = reader.ReadUInt32();
            }
        }

        public virtual void Write(BinaryWriter writer) { throw new NotImplementedException(); }

        public virtual void WriteChunk()
        {
            Console.WriteLine("*** CHUNK ***");
            Console.WriteLine("    ChunkType: {0}", this.ChunkType);
            Console.WriteLine("    ChunkVersion: {0:X}", this.Version);
            Console.WriteLine("    Offset: {0:X}", this.Offset);
            Console.WriteLine("    ID: {0:X}", this.ID);
            Console.WriteLine("    Size: {0:X}", this.Size);
            Console.WriteLine("*** END CHUNK ***");
        }
    }
}
