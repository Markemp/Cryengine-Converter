using System.IO;

namespace CgfConverter.CryEngineCore.Chunks
{
    class ChunkIvoSkin_900 : ChunkIvoSkin
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);
            var geometryInfo = new GeometryInfo();

            SkipBytes(b, 4);
            ChunkMesh_900 meshChunk = new ChunkMesh_900();
            meshChunk.Read(b);
        }
    }
}
