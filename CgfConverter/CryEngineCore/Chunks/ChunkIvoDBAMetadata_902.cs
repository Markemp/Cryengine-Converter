using CgfConverter.Models.Structs;
using Extensions;
using System.IO;
using System.Text;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// DBA metadata chunk version 0x902 (Star Citizen 4.6+).
/// Differs from v900/v901: extra uint32 after AnimCount, and 48-byte entries (3 unknowns instead of 2).
/// </summary>
internal sealed class ChunkIvoDBAMetadata_902 : ChunkIvoDBAMetadata
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        AnimCount = b.ReadUInt32();
        _ = b.ReadUInt32(); // Reserved/unknown (new in v902)

        // Read metadata entries (48 bytes each)
        // Layout: Flags(4), FPS(2), NumControllers(2), Unknown1(4), Unknown2(4), Unknown3(4), StartRotation(16), StartPosition(12)
        for (int i = 0; i < AnimCount; i++)
        {
            var entry = new IvoDBAMetaEntry
            {
                Flags = b.ReadUInt32(),
                FramesPerSecond = b.ReadUInt16(),
                NumControllers = b.ReadUInt16(),
                Unknown1 = b.ReadUInt32(),
                Unknown2 = b.ReadUInt32(),
                Unknown3 = b.ReadUInt32(),
                StartRotation = b.ReadQuaternion(),
                StartPosition = b.ReadVector3()
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
        $"ChunkIvoDBAMetadata_902: AnimCount={AnimCount}";
}
