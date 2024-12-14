using Extensions;
using System.IO;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.CryEngineCore.Chunks;

internal sealed class ChunkNodeMeshCombo_900 : ChunkNodeMeshCombo
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        Flags1 = b.ReadInt32();
        NumberOfNodes = b.ReadInt32();
        NumberOfMeshes = b.ReadInt32();
        Unknown = b.ReadInt32(); // related to number of nodes (off by 1)
        NumberOfMeshSubsets = b.ReadInt32();
        StringTableSize = b.ReadInt32();
        Unknown2 = b.ReadInt32(); // related to number of nodes 
        Unknown3 = b.ReadInt32(); // related to number of meshes. Real geometry meshes?

        NodeMeshCombos = new();
        for (int i = 0; i < NumberOfNodes; i++)
        {
            // Read NodeMeshCombo
            NodeMeshCombos.Add(new NodeMeshCombo
            {
                WorldToBone = b.ReadMatrix3x4(),
                BoneToWorld = b.ReadMatrix3x4(),
                ScaleComponent = b.ReadVector3(),
                Unknown = b.ReadUInt32(),
                Unknown2 = b.ReadUInt32(),
                ParentIndex = b.ReadUInt16(),
                Filler = b.ReadUInt16(),
                BoundingBoxMin = b.ReadVector3(),
                BoundingBoxMax = b.ReadVector3(),
                Unknown3 = new uint[4] { b.ReadUInt32(), b.ReadUInt32(), b.ReadUInt32(), b.ReadUInt32() },
                NumberOfVertices = b.ReadUInt32(),
                NumberOfChildren = b.ReadUInt16(),
                Flag = b.ReadUInt16()
            });
        }
        UnknownIndices = new();
        MaterialIndices = new();
        for (int i = 0; i < Unknown; i++)
        {
            UnknownIndices.Add(b.ReadUInt16());
        }
        for (int i = 0; i < NumberOfMeshSubsets; i++)
        {
            MaterialIndices.Add(b.ReadUInt16());
        }

        NodeNames = GetNullSeparatedStrings(NumberOfNodes, b);
        // After reading strings, need to align to the next word.
        // There is more data after here but it's unknown.
    }
}
