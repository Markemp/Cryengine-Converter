using CgfConverter.Models.Structs;
using Extensions;
using System.IO;
using System.Text;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// DBA metadata chunk version 0x901.
/// Contains animation metadata entries (44 bytes each) and path string table.
/// </summary>
internal sealed class ChunkIvoDBAMetadata_901 : ChunkIvoDBAMetadata
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        AnimCount = b.ReadUInt32();
        Reserved = b.ReadUInt32();

        // Read metadata entries (44 bytes each)
        for (int i = 0; i < AnimCount; i++)
        {
            var entry = new IvoDBAMetaEntry
            {
                NumKeys = b.ReadUInt16(),
                BoneCount = b.ReadUInt16(),
                Flags = b.ReadUInt32(),
                PathLength = b.ReadUInt32(),
                StartPosition = b.ReadVector3(),
                StartRotation = b.ReadQuaternion(),
                Padding = b.ReadUInt32()
            };
            Entries.Add(entry);
        }

        // Read string table - null-terminated animation paths
        for (int i = 0; i < AnimCount; i++)
        {
            var path = ReadNullTerminatedString(b);
            AnimPaths.Add(path);
        }
    }

    private static string ReadNullTerminatedString(BinaryReader b)
    {
        var sb = new StringBuilder();
        while (true)
        {
            byte c = b.ReadByte();
            if (c == 0)
                break;
            sb.Append((char)c);
        }
        return sb.ToString();
    }

    public override string ToString() =>
        $"ChunkIvoDBAMetadata_901: AnimCount={AnimCount}";
}
