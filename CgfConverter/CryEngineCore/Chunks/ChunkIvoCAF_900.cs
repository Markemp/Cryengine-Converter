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
/// Parses #caf animation blocks with bone controllers and keyframe data.
///
/// Structure per 010 template:
/// - Block header (12 bytes): signature (4), boneCount (uint16), magic (uint16), dataSize (uint32)
/// - Bone hashes (4 bytes each): CRC32 identifiers
/// - Controller entries (24 bytes each): rotation track (12 bytes) + position track (12 bytes)
///   - Each track: numKeys (uint16), formatFlags (uint16), timeOffset (uint32), dataOffset (uint32)
///   - Offsets are relative to the start of each controller, not the controllers array
/// - Animation data: scattered throughout block, accessed via controller offsets
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
            BoneCount = b.ReadUInt16(),
            Magic = b.ReadUInt16(),
            DataSize = b.ReadUInt32()
        };

        if (Header.Signature != "#caf")
        {
            HelperMethods.Log(LogLevelEnum.Warning, $"ChunkIvoCAF_900: Expected #caf signature, got '{Header.Signature}'");
            return;
        }

        // Magic value can be 0xAA55 (DBA) or 0xFFFF (CAF).  NEEDS VERIFICATION
        if (Header.Magic != 0xAA55 && Header.Magic != 0xFFFF)
            HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoCAF_900: Magic 0x{Header.Magic:X4}");

        int numBones = Header.BoneCount;

        // Read bone hash array (4 bytes per bone)
        BoneHashes = new uint[numBones];
        for (int i = 0; i < numBones; i++)
        {
            BoneHashes[i] = b.ReadUInt32();
        }

        // Track start of controllers array - offsets are relative to each controller's start
        long controllersArrayStart = b.BaseStream.Position;

        // Read controller entries (24 bytes per bone)
        // Per 010 template: rotation track (12 bytes) + position track (12 bytes)
        // Offsets are relative to the start of each controller (not the array start)
        Controllers = new IvoAnimControllerEntry[numBones];
        ControllerOffsets = new long[numBones];
        for (int i = 0; i < numBones; i++)
        {
            // Track the file position where this controller starts (needed for offset calculations)
            ControllerOffsets[i] = b.BaseStream.Position;

            Controllers[i] = new IvoAnimControllerEntry
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

            var ctrl = Controllers[i];

            // Warn about unknown rotation format flags
            // Known formats: 0x8040 = ubyte time array, 0x8042 = uint16 time with 8-byte header
            if (ctrl.HasRotation && ctrl.RotFormatFlags != 0x8040 && ctrl.RotFormatFlags != 0x8042)
            {
                HelperMethods.Log(LogLevelEnum.Warning,
                    $"ChunkIvoCAF_900: Bone {i} (0x{BoneHashes[i]:X08}) has unknown rotation format flag 0x{ctrl.RotFormatFlags:X4} (expected 0x8040 or 0x8042)");
            }

            // Warn about unknown position format flags
            // Known formats: 0xC0xx (float), 0xC1xx (SNORM full), 0xC2xx (SNORM packed)
            if (ctrl.HasPosition)
            {
                var posFormat = IvoAnimationHelpers.GetPositionFormat(ctrl.PosFormatFlags);
                if (posFormat == IvoPositionFormat.None)
                {
                    HelperMethods.Log(LogLevelEnum.Warning,
                        $"ChunkIvoCAF_900: Bone {i} (0x{BoneHashes[i]:X08}) has unknown position format flag 0x{ctrl.PosFormatFlags:X4}");
                }
            }

            // Debug: Log controller entry details
            HelperMethods.Log(LogLevelEnum.Debug,
                $"  Bone {i} (0x{BoneHashes[i]:X08}): " +
                $"Rot={ctrl.NumRotKeys}keys @time=0x{ctrl.RotTimeOffset:X} @data=0x{ctrl.RotDataOffset:X} (flags=0x{ctrl.RotFormatFlags:X4}), " +
                $"Pos={ctrl.NumPosKeys}keys @time=0x{ctrl.PosTimeOffset:X} @data=0x{ctrl.PosDataOffset:X} (flags=0x{ctrl.PosFormatFlags:X4})");
        }

        // Parse animation data for each bone
        // Offsets are relative to each controller's start position in the file
        ParseAnimationData(b);

        // Seek to end of block
        long blockEnd = _blockStartOffset + Header.DataSize;
        b.BaseStream.Seek(blockEnd, SeekOrigin.Begin);
    }

    private void ParseAnimationData(BinaryReader b)
    {
        for (int i = 0; i < Controllers.Length; i++)
        {
            var ctrl = Controllers[i];
            uint boneHash = BoneHashes[i];
            long controllerStart = ControllerOffsets[i];

            // Parse rotation data (if present)
            if (ctrl.HasRotation && ctrl.NumRotKeys > 0)
            {
                // Parse rotation time keys using shared helper
                List<float> times;
                if (ctrl.RotTimeOffset > 0)
                {
                    b.BaseStream.Seek(controllerStart + ctrl.RotTimeOffset, SeekOrigin.Begin);
                    times = IvoAnimationHelpers.ReadTimeKeys(b, ctrl.NumRotKeys, ctrl.RotFormatFlags);
                }
                else
                {
                    // No time offset - use sequential frame numbers
                    times = new List<float>(ctrl.NumRotKeys);
                    for (int t = 0; t < ctrl.NumRotKeys; t++)
                        times.Add(t);
                }

                RotationTimes[boneHash] = times;

                // Rotation data is at controllerStart + rotDataOffset
                b.BaseStream.Seek(controllerStart + ctrl.RotDataOffset, SeekOrigin.Begin);
                var rotations = ReadRotationKeys(b, ctrl.NumRotKeys, ctrl.RotFormatFlags);
                Rotations[boneHash] = rotations;
            }

            // Parse position data (if present)
            if (ctrl.HasPosition && ctrl.NumPosKeys > 0)
            {
                // Parse position time keys using shared helper
                List<float> posTimes;
                if (ctrl.PosTimeOffset > 0)
                {
                    b.BaseStream.Seek(controllerStart + ctrl.PosTimeOffset, SeekOrigin.Begin);
                    posTimes = IvoAnimationHelpers.ReadTimeKeys(b, ctrl.NumPosKeys, ctrl.PosFormatFlags);
                }
                else
                {
                    // No time offset - use sequential frame numbers
                    posTimes = new List<float>(ctrl.NumPosKeys);
                    for (int t = 0; t < ctrl.NumPosKeys; t++)
                        posTimes.Add(t);
                }

                PositionTimes[boneHash] = posTimes;

                // Parse position data based on format (high byte)
                b.BaseStream.Seek(controllerStart + ctrl.PosDataOffset, SeekOrigin.Begin);
                var positions = ReadPositionKeys(b, ctrl.NumPosKeys, ctrl.PosFormatFlags, boneHash);
                if (positions.Count > 0)
                {
                    Positions[boneHash] = positions;
                }
            }
        }

        HelperMethods.Log(LogLevelEnum.Debug,
            $"ChunkIvoCAF_900: Parsed {Rotations.Count} rotation tracks, {Positions.Count} position tracks");
    }

    private List<Quaternion> ReadRotationKeys(BinaryReader b, int count, ushort formatFlags)
    {
        var rotations = new List<Quaternion>(count);

        // Per 010 template: #ivo CAF uses uncompressed quaternions (16 bytes each)
        // Format flag 0x8042 indicates standard rotation track with uncompressed quats
        byte compression = (byte)(formatFlags & 0xFF);

        for (int i = 0; i < count; i++)
        {
            Quaternion rot = compression switch
            {
                0x42 => b.ReadQuaternion(),                    // Standard uncompressed (16 bytes)
                0x40 => b.ReadQuaternion(),                    // Uncompressed variant (16 bytes)
                0x00 => b.ReadQuaternion(),                    // NoCompressQuat (16 bytes)
                _ => b.ReadQuaternion()                        // Default to uncompressed
            };
            rotations.Add(rot);
        }

        return rotations;
    }

    /// <summary>
    /// Reads position keyframes based on format flags.
    /// </summary>
    /// <param name="b">Binary reader positioned at position data.</param>
    /// <param name="count">Number of position keys.</param>
    /// <param name="formatFlags">Position format flags (high byte determines format).</param>
    /// <param name="boneHash">Bone hash for logging.</param>
    private List<Vector3> ReadPositionKeys(BinaryReader b, int count, ushort formatFlags, uint boneHash)
    {
        var positions = new List<Vector3>(count);
        var format = IvoAnimationHelpers.GetPositionFormat(formatFlags);

        switch (format)
        {
            case IvoPositionFormat.FloatVector3:
                // 0xC0xx: Float Vector3 (12 bytes per key), no header
                for (int i = 0; i < count; i++)
                {
                    positions.Add(b.ReadVector3());
                }
                HelperMethods.Log(LogLevelEnum.Debug,
                    $"    Position (0xC0 float): {count} keys");
                break;

            case IvoPositionFormat.SNormFull:
                // 0xC1xx: SNORM with 24-byte header, all channels (6 bytes per key)
                {
                    // Read 24-byte header: channelMask (12 bytes) + scale (12 bytes)
                    Vector3 channelMask = b.ReadVector3();
                    Vector3 scale = b.ReadVector3();

                    for (int i = 0; i < count; i++)
                    {
                        short sx = b.ReadInt16();
                        short sy = b.ReadInt16();
                        short sz = b.ReadInt16();

                        float x = IvoAnimationHelpers.DecompressSNorm(sx, scale.X);
                        float y = IvoAnimationHelpers.DecompressSNorm(sy, scale.Y);
                        float z = IvoAnimationHelpers.DecompressSNorm(sz, scale.Z);

                        positions.Add(new Vector3(x, y, z));
                    }
                    HelperMethods.Log(LogLevelEnum.Debug,
                        $"    Position (0xC1 SNORM full): {count} keys, scale=({scale.X:F4}, {scale.Y:F4}, {scale.Z:F4})");
                }
                break;

            case IvoPositionFormat.SNormPacked:
                // 0xC2xx: SNORM with 24-byte header, packed active channels only
                {
                    // Read 24-byte header: channelMask (12 bytes) + scale (12 bytes)
                    Vector3 channelMask = b.ReadVector3();
                    Vector3 scale = b.ReadVector3();

                    bool xActive = IvoAnimationHelpers.IsChannelActive(channelMask.X);
                    bool yActive = IvoAnimationHelpers.IsChannelActive(channelMask.Y);
                    bool zActive = IvoAnimationHelpers.IsChannelActive(channelMask.Z);

                    for (int i = 0; i < count; i++)
                    {
                        float x = 0, y = 0, z = 0;

                        if (xActive)
                        {
                            short sx = b.ReadInt16();
                            x = IvoAnimationHelpers.DecompressSNorm(sx, scale.X);
                        }
                        if (yActive)
                        {
                            short sy = b.ReadInt16();
                            y = IvoAnimationHelpers.DecompressSNorm(sy, scale.Y);
                        }
                        if (zActive)
                        {
                            short sz = b.ReadInt16();
                            z = IvoAnimationHelpers.DecompressSNorm(sz, scale.Z);
                        }

                        positions.Add(new Vector3(x, y, z));
                    }

                    string activeChannels = $"{(xActive ? "X" : "")}{(yActive ? "Y" : "")}{(zActive ? "Z" : "")}";
                    HelperMethods.Log(LogLevelEnum.Debug,
                        $"    Position (0xC2 SNORM packed): {count} keys, active=[{activeChannels}], scale=({scale.X:F4}, {scale.Y:F4}, {scale.Z:F4})");
                }
                break;

            default:
                HelperMethods.Log(LogLevelEnum.Warning,
                    $"ChunkIvoCAF_900: Bone 0x{boneHash:X08} has unknown position format 0x{formatFlags:X4}");
                break;
        }

        return positions;
    }

    public override string ToString() =>
        $"ChunkIvoCAF_900: {Header.Signature} Bones={Header.BoneCount}, DataSize={Header.DataSize}";
}
