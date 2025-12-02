using CgfConverter.Models.Structs;
using CgfConverter.Utilities;
using Extensions;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// CAF animation data chunk version 0x900.
/// Parses #caf animation blocks with bone controllers and compressed keyframe data.
/// </summary>
internal sealed class ChunkIvoCAF_900 : ChunkIvoCAF
{
    private long _blockStartOffset;

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        _blockStartOffset = b.BaseStream.Position;

        // Read block header (12 bytes)
        Header = new IvoAnimBlockHeader
        {
            Signature = Encoding.ASCII.GetString(b.ReadBytes(4)),
            BoneCount = b.ReadByte(),
            Padding = b.ReadByte(),
            Magic = b.ReadUInt16(),
            DataSize = b.ReadUInt32()
        };

        if (Header.Signature != "#caf")
        {
            HelperMethods.Log(LogLevelEnum.Warning, $"ChunkIvoCAF_900: Expected #caf signature, got '{Header.Signature}'");
            return;
        }

        // Magic value can be 0xAA55 (DBA) or 0xFFFF (CAF)
        if (Header.Magic != 0xAA55 && Header.Magic != 0xFFFF)
        {
            HelperMethods.Log(LogLevelEnum.Warning, $"ChunkIvoCAF_900: Unexpected magic 0x{Header.Magic:X4}");
        }

        int numBones = Header.BoneCount;

        // Read bone hash array (4 bytes per bone)
        BoneHashes = new uint[numBones];
        for (int i = 0; i < numBones; i++)
        {
            BoneHashes[i] = b.ReadUInt32();
        }

        // Read controller entries (24 bytes per bone)
        Controllers = new IvoAnimControllerEntry[numBones];
        for (int i = 0; i < numBones; i++)
        {
            Controllers[i] = new IvoAnimControllerEntry
            {
                NumKeys = b.ReadByte(),
                Padding = b.ReadByte(),
                FormatFlags = b.ReadUInt16(),
                RotDataOffset = b.ReadUInt32(),
                RotTimeOffset = b.ReadUInt32(),
                PosDataOffset = b.ReadUInt32(),
                PosTimeOffset = b.ReadUInt32(),
                Reserved = b.ReadUInt32()
            };
        }

        // Calculate keyframe data size and read it
        long keyframeDataStart = b.BaseStream.Position;
        long blockEnd = _blockStartOffset + 12 + Header.DataSize;
        int keyframeDataSize = (int)(blockEnd - keyframeDataStart);

        if (keyframeDataSize > 0)
        {
            KeyframeData = b.ReadBytes(keyframeDataSize);
        }
        else
        {
            KeyframeData = [];
        }

        // Parse the keyframe data for each bone
        ParseKeyframeData();
    }

    private void ParseKeyframeData()
    {
        using var ms = new MemoryStream(KeyframeData);
        using var br = new BinaryReader(ms);

        int dataOffset = GetDataOffset();

        for (int i = 0; i < Controllers.Length; i++)
        {
            var ctrl = Controllers[i];
            uint boneHash = BoneHashes[i];

            if (ctrl.NumKeys == 0)
                continue;

            // Parse rotation data
            if (ctrl.RotDataOffset > 0)
            {
                long rotPos = ctrl.RotDataOffset - dataOffset;
                if (rotPos < 0 || rotPos >= KeyframeData.Length)
                    continue;
                ms.Position = rotPos;
                var rotations = ReadRotationKeys(br, ctrl.NumKeys, ctrl.CompressionFormat);
                Rotations[boneHash] = rotations;
            }

            // Parse rotation times
            if (ctrl.RotTimeOffset > 0)
            {
                long timePos = ctrl.RotTimeOffset - dataOffset;
                if (timePos >= 0 && timePos < KeyframeData.Length)
                {
                    ms.Position = timePos;
                    var times = ReadKeyTimes(br, ctrl.NumKeys);
                    RotationTimes[boneHash] = times;
                }
            }

            // Parse position data (if present)
            if (ctrl.HasPosition && ctrl.PosDataOffset > 0)
            {
                long posPos = ctrl.PosDataOffset - dataOffset;
                if (posPos >= 0 && posPos < KeyframeData.Length)
                {
                    ms.Position = posPos;
                    var positions = ReadPositionKeys(br, ctrl.NumKeys);
                    Positions[boneHash] = positions;
                }
            }

            // Parse position times (if present)
            if (ctrl.HasPosition && ctrl.PosTimeOffset > 0)
            {
                long posTimePos = ctrl.PosTimeOffset - dataOffset;
                if (posTimePos >= 0 && posTimePos < KeyframeData.Length)
                {
                    ms.Position = posTimePos;
                    var times = ReadKeyTimes(br, ctrl.NumKeys);
                    PositionTimes[boneHash] = times;
                }
            }
        }
    }

    /// <summary>
    /// Gets the offset from block start to keyframe data start.
    /// This is: 12 (header) + 4*numBones (hashes) + 24*numBones (controllers).
    /// </summary>
    private int GetDataOffset()
    {
        return 12 + (4 * Header.BoneCount) + (24 * Header.BoneCount);
    }

    private List<Quaternion> ReadRotationKeys(BinaryReader b, int count, byte format)
    {
        var rotations = new List<Quaternion>(count);

        for (int i = 0; i < count; i++)
        {
            Quaternion rot = format switch
            {
                0x00 => b.ReadQuaternion(),                    // NoCompressQuat (16 bytes)
                0x42 => b.ReadSmallTree48BitQuat(),            // SmallTree48BitQuat (6 bytes)
                0x43 => b.ReadSmallTree64BitQuat(),            // SmallTree64BitQuat (8 bytes)
                _ => Quaternion.Identity
            };
            rotations.Add(rot);
        }

        return rotations;
    }

    private static List<Vector3> ReadPositionKeys(BinaryReader b, int count)
    {
        var positions = new List<Vector3>(count);

        for (int i = 0; i < count; i++)
        {
            positions.Add(b.ReadVector3());
        }

        return positions;
    }

    private static List<float> ReadKeyTimes(BinaryReader b, int count)
    {
        var times = new List<float>(count);

        for (int i = 0; i < count; i++)
        {
            // Time keys are typically stored as uint16
            times.Add(b.ReadUInt16());
        }

        return times;
    }

    public override string ToString() =>
        $"ChunkIvoCAF_900: {Header.Signature} Bones={Header.BoneCount}, DataSize={Header.DataSize}";
}
