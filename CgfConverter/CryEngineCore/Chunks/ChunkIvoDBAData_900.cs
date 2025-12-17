using CgfConverter.Models.Structs;
using CgfConverter.Utilities;
using System.IO;
using System.Text;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// DBA animation data chunk version 0x900.
/// Parses multiple #dba animation blocks.
/// </summary>
internal sealed class ChunkIvoDBAData_900 : ChunkIvoDBAData
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        TotalDataSize = b.ReadUInt32();

        long dataEnd = b.BaseStream.Position + TotalDataSize - 4;

        // Parse #dba blocks until we reach the end
        while (b.BaseStream.Position < dataEnd)
        {
            // Check for #dba signature
            long blockStart = b.BaseStream.Position;
            string sig = Encoding.ASCII.GetString(b.ReadBytes(4));

            if (sig != "#dba")
            {
                HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAData_900: Expected #dba signature at offset {blockStart:X}, got '{sig}'");
                break;
            }

            // Read block header
            var header = new IvoAnimBlockHeader
            {
                Signature = sig,
                BoneCount = b.ReadUInt16(),
                Magic = b.ReadUInt16(),
                DataSize = b.ReadUInt32()
            };

            if (header.Magic != 0xAA55)
            {
                HelperMethods.Log(LogLevelEnum.Warning, $"ChunkIvoDBAData_900: Expected magic 0xAA55, got 0x{header.Magic:X4}");
            }

            int numBones = header.BoneCount;

            // Read bone hash array
            var boneHashes = new uint[numBones];
            for (int i = 0; i < numBones; i++)
            {
                boneHashes[i] = b.ReadUInt32();
            }

            // Read controller entries (24 bytes per bone)
            // Per 010 template: rotation track (12 bytes) + position track (12 bytes)
            // Note: For DBA, offsets are relative to keyframe data start (different from CAF)
            var controllers = new IvoAnimControllerEntry[numBones];
            for (int i = 0; i < numBones; i++)
            {
                controllers[i] = new IvoAnimControllerEntry
                {
                    // Rotation track info (12 bytes)
                    NumRotKeys = b.ReadUInt16(),
                    RotFormatFlags = b.ReadUInt16(),
                    RotTimeOffset = b.ReadUInt32(),
                    RotDataOffset = b.ReadUInt32(),

                    // Position track info (12 bytes)
                    NumPosKeys = b.ReadUInt16(),
                    PosFormatFlags = b.ReadUInt16(),
                    PosTimeOffset = b.ReadUInt32(),
                    PosDataOffset = b.ReadUInt32()
                };
            }

            // Read keyframe data
            long keyframeDataStart = b.BaseStream.Position;
            long blockEnd = blockStart + 12 + header.DataSize;
            int keyframeDataSize = (int)(blockEnd - keyframeDataStart);

            byte[] keyframeData;
            if (keyframeDataSize > 0)
            {
                keyframeData = b.ReadBytes(keyframeDataSize);
            }
            else
            {
                keyframeData = [];
            }

            AnimationBlocks.Add(new IvoAnimationBlock
            {
                Header = header,
                BoneHashes = boneHashes,
                Controllers = controllers,
                KeyframeData = keyframeData
            });
        }

        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAData_900: Parsed {AnimationBlocks.Count} animation blocks");
    }

    public override string ToString() =>
        $"ChunkIvoDBAData_900: TotalSize={TotalDataSize}, Animations={AnimationBlocks.Count}";
}
