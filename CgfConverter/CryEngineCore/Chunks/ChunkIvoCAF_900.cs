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
            if (ctrl.HasPosition && ctrl.PosFormatFlags != 0xC040 && ctrl.PosFormatFlags != 0xC142 && ctrl.PosFormatFlags != 0xC242)
            {
                HelperMethods.Log(LogLevelEnum.Warning,
                    $"ChunkIvoCAF_900: Bone {i} (0x{BoneHashes[i]:X08}) has unknown position format flag 0x{ctrl.PosFormatFlags:X4} (expected 0xC040, 0xC142, or 0xC242)");
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
                // Parse rotation time keys based on format flag
                // Low nibble: 0x40 = ubyte time array, 0x42 = uint16 time with 8-byte header
                var times = new List<float>(ctrl.NumRotKeys);
                byte rotTimeFormat = (byte)(ctrl.RotFormatFlags & 0x0F);

                if (ctrl.RotTimeOffset > 0)
                {
                    b.BaseStream.Seek(controllerStart + ctrl.RotTimeOffset, SeekOrigin.Begin);

                    if (rotTimeFormat == 0x00)
                    {
                        // 0x8040: ubyte time array (padded to 4-byte boundary)
                        for (int t = 0; t < ctrl.NumRotKeys; t++)
                            times.Add(b.ReadByte());
                    }
                    else
                    {
                        // 0x8042: 8-byte time header (startTime uint16, endTime uint16, marker uint32)
                        // followed by uint16 time values
                        b.ReadUInt16(); // startTime
                        b.ReadUInt16(); // endTime
                        b.ReadUInt32(); // marker
                        // Time values would follow, but we use sequential for now
                        for (int t = 0; t < ctrl.NumRotKeys; t++)
                            times.Add(t);
                    }
                }
                else
                {
                    // No time offset - use sequential frame numbers
                    for (int t = 0; t < ctrl.NumRotKeys; t++)
                        times.Add(t);
                }

                RotationTimes[boneHash] = times;

                // Rotation data is at controllerStart + rotDataOffset
                b.BaseStream.Seek(controllerStart + ctrl.RotDataOffset, SeekOrigin.Begin);
                var rotations = ReadRotationKeys(b, ctrl.NumRotKeys, ctrl.RotFormatFlags);
                Rotations[boneHash] = rotations;
            }

            // Skip position data for #ivo CAF animations
            // Position format codes determined by format flags:
            // - 0xC040 = ubyte time array, numPosKeys positions
            // - 0xC142 = uint16 time header (8 bytes), 2 positions
            // - 0xC242 = uint16 time header (8 bytes), data header (8 bytes), 1 position
            //
            // Character animations typically only animate rotations. Position data in #ivo CAF
            // appears to be either root motion direction vectors or uses compression we can't decode.
            // The skeleton's rest pose provides correct bone translations.
            if (ctrl.HasPosition && ctrl.NumPosKeys > 0)
            {
                HelperMethods.Log(LogLevelEnum.Debug,
                    $"  Bone {i} (0x{boneHash:X08}): Skipping position track (format=0x{ctrl.PosFormatFlags:X4}, {ctrl.NumPosKeys} keys) - using rest pose");
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

    public override string ToString() =>
        $"ChunkIvoCAF_900: {Header.Signature} Bones={Header.BoneCount}, DataSize={Header.DataSize}";
}
