using System.IO;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.CryEngineCore.Chunks;

internal class ChunkNodeDetails_901 : ChunkNodeDetails
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        NumberOfNodes = b.ReadInt32();
        StringTableSize = b.ReadInt32();
        Flags1 = b.ReadInt32();
        Flags2 = b.ReadInt32();
        NodeDetails = new();
        for (int i = 0; i < NumberOfNodes; i++)
        {
            NodeDetails.Add(new NodeDetail
            {
                ControllerId = b.ReadUInt32(),
                Unknown = b.ReadInt16(),
                ParentIndex = b.ReadInt16(),
                NumberOfChildren = b.ReadInt16(),
                ObjectNodeId = b.ReadInt16(),
                Unknown2 = b.ReadInt16(),
                Unknown3 = b.ReadInt16()
            });
        }
        NodeNames = GetNullSeparatedStrings(NumberOfNodes, b);
    }
}
