using Extensions;
using System;
using System.IO;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.CryEngineCore.Chunks;

internal sealed class ChunkNodeMeshCombo_900 : ChunkNodeMeshCombo
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        ZeroPad = b.ReadInt32();
        NumberOfNodes = b.ReadInt32();
        NumberOfMeshes = b.ReadInt32();
        Unknown2 = b.ReadInt32(); 
        NumberOfMeshSubsets = b.ReadInt32();
        StringTableSize = b.ReadInt32();
        Unknown1 = b.ReadInt32(); // related to number of nodes 
        Unknown3 = b.ReadInt32(); // 0 if no mesh chunk for this node

        NodeMeshCombos = [];
        for (int i = 0; i < NumberOfNodes; i++)
        {
            NodeMeshCombos.Add(new NodeMeshCombo
            {
                WorldToBone = b.ReadMatrix3x4(),
                BoneToWorld = b.ReadMatrix3x4(),
                ScaleComponent = b.ReadVector3(),
                Id = b.ReadUInt32(),
                Unknown2 = b.ReadUInt32(),
                ParentIndex = b.ReadUInt16(), // 0xffff if root
                GeometryType = (IvoGeometryType)Enum.ToObject(typeof(IvoGeometryType), b.ReadUInt16()),
                BoundingBoxMin = b.ReadVector3(),
                BoundingBoxMax = b.ReadVector3(),
                Unknown3 = [b.ReadUInt32(), b.ReadUInt32(), b.ReadUInt32(), b.ReadUInt32()],
                NumberOfVertices = b.ReadUInt32(),
                NumberOfChildren = b.ReadUInt16(),
                MeshChunkId = b.ReadUInt16()
            });
            SkipBytes(b, 40); // Skip unknown data
        }
        UnknownIndices = [];
        MaterialIndices = [];
        for (int i = 0; i < Unknown2; i++)
        {
            UnknownIndices.Add(b.ReadUInt16());
        }
        for (int i = 0; i < NumberOfMeshSubsets; i++)
        {
            MaterialIndices.Add(b.ReadUInt16());
        }

        NodeNames = GetNullSeparatedStrings(NumberOfNodes, b);
        // There is more data after here but it's unknown.
    }
}
