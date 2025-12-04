using CgfConverter.Models.Structs;
using CgfConverter.Utilities;
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

        long startOffset = b.BaseStream.Position;
        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_901: Reading at offset 0x{startOffset:X}");

        AnimCount = b.ReadUInt32();
        Reserved = b.ReadUInt32();

        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_901: AnimCount={AnimCount}, Reserved=0x{Reserved:X8}");

        // Read metadata entries (44 bytes each)
        for (int i = 0; i < AnimCount; i++)
        {
            long entryOffset = b.BaseStream.Position;
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

            // Format quaternion without curly braces to avoid String.Format issues
            var rot = entry.StartRotation;
            var pos = entry.StartPosition;
            HelperMethods.Log(LogLevelEnum.Debug,
                $"ChunkIvoDBAMetadata_901: Entry[{i}] @ 0x{entryOffset:X}: " +
                $"NumKeys={entry.NumKeys}, BoneCount={entry.BoneCount}, Flags=0x{entry.Flags:X8}, " +
                $"PathLength={entry.PathLength}, StartPos=({pos.X:F3}, {pos.Y:F3}, {pos.Z:F3}), " +
                $"StartRot=({rot.X:F3}, {rot.Y:F3}, {rot.Z:F3}, {rot.W:F3}), Padding=0x{entry.Padding:X8}");
        }

        // Read string table - null-terminated animation paths
        // Version 901: String table appears to start 4 bytes earlier than expected
        // (last entry's "Padding" field contains first 4 chars of first string)
        // Seek back 4 bytes to capture the full first string
        b.BaseStream.Seek(-4, SeekOrigin.Current);

        long stringTableOffset = b.BaseStream.Position;
        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_901: String table at offset 0x{stringTableOffset:X} (adjusted -4 bytes)");

        for (int i = 0; i < AnimCount; i++)
        {
            var path = ReadNullTerminatedString(b);
            AnimPaths.Add(path);
            HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_901: AnimPath[{i}] = \"{path}\"");
        }

        long endOffset = b.BaseStream.Position;
        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAMetadata_901: Finished at offset 0x{endOffset:X}, read {endOffset - startOffset} bytes");
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
