using System.IO;

namespace CgfConverter.CryEngineCore.Chunks
{
    class ChunkIvoSkin_900 : ChunkIvoSkin
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            SkipBytes(b, 4);
            
            ChunkMesh_900 meshChunk = new ChunkMesh_900();
            meshChunk._model = _model;
            meshChunk._header = _header;
            meshChunk._header.Offset = (uint)b.BaseStream.Position;
            meshChunk.ID = 2;
            meshChunk.Read(b);
            
            SkipBytes(b, 120);

            ChunkMeshSubsets_900 subsetsChunk = new ChunkMeshSubsets_900(meshChunk.NumVertSubsets);
            // Create dummy header info here (ChunkType, version, size, offset)
            subsetsChunk._model = _model;
            subsetsChunk._header = _header;
            subsetsChunk._header.Offset = (uint)b.BaseStream.Position;
            subsetsChunk.ID = 3; 
            subsetsChunk.Read(b);

            // Indices datastream
            ChunkDataStream_900 indicesDatastreamChunk = new ChunkDataStream_900((uint)meshChunk.NumIndices);
            indicesDatastreamChunk._model = _model;
            indicesDatastreamChunk._header = _header;
            indicesDatastreamChunk._header.Offset = (uint)b.BaseStream.Position;
            indicesDatastreamChunk.ID = 4;
            indicesDatastreamChunk.Read(b);

            // VertsUV datastream
            ChunkDataStream_900 vertsUvsDatastreamChunk = new ChunkDataStream_900((uint)meshChunk.NumVertices);
            vertsUvsDatastreamChunk._model = _model;
            vertsUvsDatastreamChunk._header = _header;
            vertsUvsDatastreamChunk._header.Offset = (uint)b.BaseStream.Position;
            vertsUvsDatastreamChunk.ID = 5;
            vertsUvsDatastreamChunk.Read(b);

        }
    }
}
