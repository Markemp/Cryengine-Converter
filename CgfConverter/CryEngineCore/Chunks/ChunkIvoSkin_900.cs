using System;
using System.IO;
using System.Linq;

namespace CgfConverter.CryEngineCore.Chunks
{
    class ChunkIvoSkin_900 : ChunkIvoSkin
    {
        /*
         * Node (Chunk) IDs for Ivo models
         * 1: NodeChunk
         * 2: MeshChunk
         * 3: MeshSubsets
         * 4: Indices
         * 5: VertsUVs (Contains vertices, UVs and colors)
         * 6: Normals
         * 7: Tangents
         * 8: Bonemap  (Assume all #ivo files have armatures)
         * 9: Colors
         * 10: Colors2  (Probably won't use these in the Collada exporter)
         */
        private bool hasNormalsChunk = false;  // If Flags2 of the meshchunk is 5, there is a separate normals chunk

        public override void Read(BinaryReader b)
        {
            var model = _model;

            base.Read(b);
            SkipBytes(b, 4);

            ChunkMesh_900 meshChunk = new()
            {
                _model = _model,
                _header = _header
            };
            meshChunk._header.Offset = (uint)b.BaseStream.Position;
            meshChunk.ChunkType = ChunkType.Mesh;
            meshChunk.Read(b);
            meshChunk.ID = 2;
            meshChunk.MeshSubsetsData = 3;
            model.ChunkMap.Add(meshChunk.ID, meshChunk);

            // If chunk has a NORMALS or NORMALS2 chunk, don't calculate normals from Tangents
            var normalChunk = model.ChunkMap.Values.Where(a => a.ID == 6);

            if (meshChunk.Flags2 == 5)
                hasNormalsChunk = true;

            SkipBytes(b, 120);  // Unknown data.  All 0x00

            ChunkMeshSubsets_900 subsetsChunk = new(meshChunk.NumVertSubsets)
            {
                // Create dummy header info here (ChunkType, version, size, offset)
                _model = _model,
                _header = _header
            };
            subsetsChunk._header.Offset = (uint)b.BaseStream.Position; 
            subsetsChunk.Read(b);
            subsetsChunk.ChunkType = ChunkType.MeshSubsets;
            subsetsChunk.ID = 3;
            model.ChunkMap.Add(subsetsChunk.ID, subsetsChunk);

            while (b.BaseStream.Position != b.BaseStream.Length)
            {
                var chunkType = (DatastreamType)Enum.ToObject(typeof(DatastreamType), b.ReadUInt32());
                b.BaseStream.Position = b.BaseStream.Position - 4;
                switch (chunkType)
                {
                    case DatastreamType.IVOINDICES:
                        // Indices datastream
                        ChunkDataStream_900 indicesDatastreamChunk = new((uint)meshChunk.NumIndices)
                        {
                            _model = _model,
                            _header = _header
                        };
                        indicesDatastreamChunk._header.Offset = (uint)b.BaseStream.Position;
                        indicesDatastreamChunk.Read(b);
                        indicesDatastreamChunk.DataStreamType = DatastreamType.INDICES;
                        indicesDatastreamChunk.ChunkType = ChunkType.DataStream;
                        indicesDatastreamChunk.ID = 4;
                        model.ChunkMap.Add(indicesDatastreamChunk.ID, indicesDatastreamChunk);
                        break;
                    case DatastreamType.IVOVERTSUVS:
                        ChunkDataStream_900 vertsUvsDatastreamChunk = new((uint)meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header
                        };
                        vertsUvsDatastreamChunk._header.Offset = (uint)b.BaseStream.Position; 
                        vertsUvsDatastreamChunk.Read(b);
                        vertsUvsDatastreamChunk.DataStreamType = DatastreamType.VERTSUVS;
                        vertsUvsDatastreamChunk.ChunkType = ChunkType.DataStream;
                        vertsUvsDatastreamChunk.ID = 5;
                        model.ChunkMap.Add(vertsUvsDatastreamChunk.ID, vertsUvsDatastreamChunk);

                        // Create colors chunk
                        ChunkDataStream_900 c = new((uint)meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header,
                            ChunkType = ChunkType.DataStream,
                            BytesPerElement = 4,
                            DataStreamType = DatastreamType.COLORS,
                            Colors = vertsUvsDatastreamChunk.Colors,
                            ID = 9
                        };
                        model.ChunkMap.Add(c.ID, c);
                        break;
                    case DatastreamType.IVONORMALS:
                    case DatastreamType.IVONORMALS2:
                        ChunkDataStream_900 normals = new((uint)meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header
                        };
                        normals._header.Offset = (uint)b.BaseStream.Position;
                        normals.Read(b);
                        normals.DataStreamType = DatastreamType.IVOCOLORS2;
                        normals.ChunkType = ChunkType.DataStream;
                        normals.ID = 6;
                        model.ChunkMap.Add(normals.ID, normals);
                        break;
                    case DatastreamType.IVOCOLORS2:
                        ChunkDataStream_900 colors2 = new((uint)meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header
                        };
                        colors2._header.Offset = (uint)b.BaseStream.Position; 
                        colors2.Read(b);
                        colors2.DataStreamType = DatastreamType.IVOCOLORS2;
                        colors2.ChunkType = ChunkType.DataStream;
                        colors2.ID = 10;
                        model.ChunkMap.Add(colors2.ID, colors2);
                        break;
                    case DatastreamType.IVOTANGENTS:
                        ChunkDataStream_900 tangents = new((uint)meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header
                        };
                        tangents._header.Offset = (uint)b.BaseStream.Position; 
                        tangents.Read(b);
                        tangents.DataStreamType = DatastreamType.TANGENTS;
                        tangents.ChunkType = ChunkType.DataStream;
                        tangents.ID = 7;
                        model.ChunkMap.Add(tangents.ID, tangents);
                        var existingNormalChunk = model.ChunkMap.Values.Where(a => a.ID == 6).Count();
                        if (existingNormalChunk == 0)
                        {
                            // Create a normals chunk from Tangents data
                            ChunkDataStream_900 norms = new((uint)meshChunk.NumVertices)
                            {
                                _model = _model,
                                _header = _header,
                                //norms._header.Offset = (uint)b.BaseStream.Position;
                                ChunkType = ChunkType.DataStream,
                                BytesPerElement = 4,
                                DataStreamType = DatastreamType.NORMALS,
                                Normals = tangents.Normals,
                                ID = 6
                            };
                            model.ChunkMap.Add(norms.ID, norms);
                        }
                        break;
                    case DatastreamType.IVOBONEMAP:
                        ChunkDataStream_900 bonemap = new((uint)meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header
                        };
                        bonemap._header.Offset = (uint)b.BaseStream.Position; 
                        bonemap.Read(b);
                        bonemap.DataStreamType = DatastreamType.BONEMAP;
                        bonemap.ChunkType = ChunkType.DataStream;
                        bonemap.ID = 8;
                        model.ChunkMap.Add(bonemap.ID, bonemap);
                        break;
                    default:
                        b.BaseStream.Position = b.BaseStream.Position + 4;
                        break;
                }
            }
        }
    }
}
