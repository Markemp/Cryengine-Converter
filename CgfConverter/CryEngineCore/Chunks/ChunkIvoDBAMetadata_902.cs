using CgfConverter.Models.Structs;
using CgfConverter.Utilities;
using Extensions;
using System.IO;
using System.Text;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// DBA metadata chunk version 0x902.
/// Same structure as 0x901 but each entry has two additional uint32 fields:
/// - Unknown0 (before Flags)
/// - Unknown3 (after NumControllers)
/// Entry size: 52 bytes (vs 44 in 0x901).
/// </summary>
internal sealed class ChunkIvoDBAMetadata_902 : ChunkIvoDBAMetadata
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        long startOffset = b.BaseStream.Position;
        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_902: Reading at offset 0x{startOffset:X}");

        AnimCount = b.ReadUInt32();
        SkipBytes(b, 4); // Padding
        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_902: AnimCount={AnimCount}");

        // Read metadata entries (48 bytes each)
        // Layout:Flags(4), FPS(2), NumControllers(2), Unknown3(4), Unknown1(4), Unknown2(4), StartRotation(16), StartPosition(12)
        for (int i = 0; i < AnimCount; i++)
        {
            long entryOffset = b.BaseStream.Position;
            var entry = new IvoDBAMetaEntry
            {
                Flags = b.ReadUInt32(),
                FramesPerSecond = b.ReadUInt16(),
                NumControllers = b.ReadUInt16(),
                Unknown3 = b.ReadUInt32(),
                Unknown1 = b.ReadUInt32(),
                Unknown2 = b.ReadUInt32(),
                StartRotation = b.ReadQuaternion(),
                StartPosition = b.ReadVector3()
            };
            Entries.Add(entry);

            var rot = entry.StartRotation;
            var pos = entry.StartPosition;
            HelperMethods.Log(LogLevelEnum.Debug,
                $"ChunkIvoDBAMetadata_902: Entry[{i}] @ 0x{entryOffset:X}: " +
                $"Unknown0=0x{entry.Unknown0:X}, Flags=0x{entry.Flags:X}, FPS={entry.FramesPerSecond}, Controllers={entry.NumControllers}, " +
                $"Unknown3=0x{entry.Unknown3:X}, Unknown1=0x{entry.Unknown1:X}, Unknown2=0x{entry.Unknown2:X}, " +
                $"StartRot=({rot.X:F3}, {rot.Y:F3}, {rot.Z:F3}, {rot.W:F3}), " +
                $"StartPos=({pos.X:F3}, {pos.Y:F3}, {pos.Z:F3})");
        }

        // Read string table - null-terminated animation paths
        long stringTableOffset = b.BaseStream.Position;
        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_902: String table at offset 0x{stringTableOffset:X}");

        for (int i = 0; i < AnimCount; i++)
        {
            var path = ReadNullTerminatedString(b);
            AnimPaths.Add(path);
            HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_902: AnimPath[{i}] = \"{path}\"");
        }

        long endOffset = b.BaseStream.Position;
        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_902: Finished at offset 0x{endOffset:X}, read {endOffset - startOffset} bytes");
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
