using System;
using System.IO;

namespace CgfConverter.CryEngineCore.Chunks
{
    class ChunkIvoSkin_900 : ChunkIvoSkin
    {
        /*
         * Node IDs for Ivo models
         * 1: NodeChunk
         * 2: MeshChunk
         * 3: MeshSubsets
         * 4: Indices
         * 5: VertsUVs (contains vertices, UVs and colors)
         * 6: Normals
         * 7: Tangents
         * 8: Bonemap  (assume all #ivo files have armatures)
         */
        private bool hasNormalsChunk = false;  // If Flags2 of the meshchunk is 5, there is a separate normals chunk

        public override void Read(BinaryReader b)
        {
            var model = _model;

            base.Read(b);

            ChunkMesh_900 meshChunk = new ChunkMesh_900();
            meshChunk._model = _model;
            meshChunk._header = _header;
            meshChunk._header.Offset = (uint)b.BaseStream.Position;
            meshChunk.ChunkType = ChunkType.Mesh;
            meshChunk.Read(b);
            meshChunk.ID = 2;
            meshChunk.MeshSubsets = 3;
            meshChunk.IndicesData = 4;
            meshChunk.VertsUVsData = 5;
            meshChunk.NormalsData = 6;
            meshChunk.TangentsData = 7;
            meshChunk.BoneMapData = 8;
            model.ChunkMap.Add(meshChunk.ID, meshChunk);

            if (meshChunk.Flags2 == 5)
                hasNormalsChunk = true;

            SkipBytes(b, 120);  // Unknown data.  All 0x00

            ChunkMeshSubsets_900 subsetsChunk = new ChunkMeshSubsets_900(meshChunk.NumVertSubsets);
            // Create dummy header info here (ChunkType, version, size, offset)
            subsetsChunk._model = _model;
            subsetsChunk._header = _header;
            subsetsChunk._header.Offset = (uint)b.BaseStream.Position;
            subsetsChunk.ChunkType = ChunkType.MeshSubsets;
            subsetsChunk.Read(b);
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
                        ChunkDataStream_900 indicesDatastreamChunk = new ChunkDataStream_900((uint)meshChunk.NumIndices);
                        indicesDatastreamChunk._model = _model;
                        indicesDatastreamChunk._header = _header;
                        indicesDatastreamChunk._header.Offset = (uint)b.BaseStream.Position;
                        indicesDatastreamChunk.ChunkType = ChunkType.DataStream;
                        indicesDatastreamChunk.Read(b);
                        indicesDatastreamChunk.ID = 4;
                        model.ChunkMap.Add(indicesDatastreamChunk.ID, indicesDatastreamChunk);
                        break;
                    case DatastreamType.IVOVERTSUVS:
                        ChunkDataStream_900 vertsUvsDatastreamChunk = new ChunkDataStream_900((uint)meshChunk.NumVertices);
                        vertsUvsDatastreamChunk._model = _model;
                        vertsUvsDatastreamChunk._header = _header;
                        vertsUvsDatastreamChunk._header.Offset = (uint)b.BaseStream.Position;
                        vertsUvsDatastreamChunk.ChunkType = ChunkType.DataStream;
                        vertsUvsDatastreamChunk.Read(b);
                        vertsUvsDatastreamChunk.ID = 5;
                        model.ChunkMap.Add(vertsUvsDatastreamChunk.ID, vertsUvsDatastreamChunk);
                        break;
                    case DatastreamType.IVONORMALS:
                        ChunkDataStream_900 normals = new ChunkDataStream_900((uint)meshChunk.NumVertices);
                        normals._model = _model;
                        normals._header = _header;
                        normals._header.Offset = (uint)b.BaseStream.Position;
                        normals.ChunkType = ChunkType.DataStream;
                        normals.Read(b);
                        normals.ID = 6;
                        model.ChunkMap.Add(normals.ID, normals);
                        break;
                    case DatastreamType.IVOTANGENTS:
                        ChunkDataStream_900 tangents = new ChunkDataStream_900((uint)meshChunk.NumVertices);
                        tangents._model = _model;
                        tangents._header = _header;
                        tangents._header.Offset = (uint)b.BaseStream.Position;
                        tangents.ChunkType = ChunkType.DataStream;
                        tangents.Read(b);
                        tangents.ID = 7;
                        model.ChunkMap.Add(tangents.ID, tangents);
                        break;
                    case DatastreamType.IVOBONEMAP:
                        ChunkDataStream_900 bonemap = new ChunkDataStream_900((uint)meshChunk.NumVertices);
                        bonemap._model = _model;
                        bonemap._header = _header;
                        bonemap._header.Offset = (uint)b.BaseStream.Position;
                        bonemap.ChunkType = ChunkType.DataStream;
                        bonemap.Read(b);
                        bonemap.ID = 8;
                        model.ChunkMap.Add(bonemap.ID, bonemap);
                        break;
                }
            }
        }
    }
}
