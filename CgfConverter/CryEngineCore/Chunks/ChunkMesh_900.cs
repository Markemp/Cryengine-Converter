using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkMesh_900 : ChunkMesh
    {
        public override void Read(BinaryReader b)
        {
            Flags1 = b.ReadInt32();
            Flags2 = 0;
            NumVertices = b.ReadInt32();
            NumIndices = b.ReadInt32();
            NumVertSubsets = b.ReadInt32();
            SkipBytes(b, 4);
            MinBound.x = b.ReadSingle();
            MinBound.y = b.ReadSingle();
            MinBound.z = b.ReadSingle();
            MaxBound.x = b.ReadSingle();
            MaxBound.y = b.ReadSingle();
            MaxBound.z = b.ReadSingle();

            ID = 2; // Node chunk ID = 1

            VertsUVsData = 4;
            NormalsData = 5;
            TangentsData = 6;
            ColorsData = 7;
        }
    }
}
