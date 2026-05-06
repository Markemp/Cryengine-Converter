using CgfConverter.Models.Structs;
using CgfConverter.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace CgfConverter.CryEngineCore.Chunks;

/// <summary>
/// DBA animation data chunk version 0x900.
/// Parses multiple #dba animation blocks with full keyframe data.
/// </summary>
internal sealed class ChunkIvoDBAData_900 : ChunkIvoDBAData
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        TotalDataSize = b.ReadUInt32();

        long dataEnd = b.BaseStream.Position + TotalDataSize - 4;

        // Parse #dba blocks until we reach the end
        int blockIndex = 0;
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

            HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAData_900: Block {blockIndex} - {numBones} bones, size={header.DataSize}");

            // Read bone hash array
            var boneHashes = new uint[numBones];
            for (int i = 0; i < numBones; i++)
            {
                boneHashes[i] = b.ReadUInt32();
            }

            // Read controller entries (24 bytes per bone)
            // Offsets are relative to the start of each controller entry (same as CAF)
            var controllers = new IvoAnimControllerEntry[numBones];
            var controllerOffsets = new long[numBones];
            for (int i = 0; i < numBones; i++)
            {
                controllerOffsets[i] = b.BaseStream.Position;

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

            // The next #dba block starts immediately after this block's controller
            // headers. Verified against AEGS Avenger landing-gear DBA: blocks are
            // packed header-back-to-header (12 + 4*bones + 24*bones), and the
            // shared keyframe pool follows all the headers. DataSize is NOT the
            // stride to the next block — it's the per-animation keyframe-pool
            // size, used only as a hint. Capture the position now, before
            // ParseAnimationData seeks around inside the keyframe pool.
            long positionAfterHeaders = b.BaseStream.Position;

            // Create animation block with parsed data
            var animBlock = new IvoAnimationBlock
            {
                Header = header,
                BoneHashes = boneHashes,
                Controllers = controllers,
                ControllerOffsets = controllerOffsets
            };

            // Parse keyframe data for each bone (this seeks around within the data area)
            ParseAnimationData(b, animBlock);

            AnimationBlocks.Add(animBlock);

            // Restore position to the start of the next block's header
            b.BaseStream.Seek(positionAfterHeaders, SeekOrigin.Begin);
            blockIndex++;
        }

        HelperMethods.Log(LogLevelEnum.Debug, $"ChunkIvoDBAData_900: Parsed {AnimationBlocks.Count} animation blocks");
    }

    private void ParseAnimationData(BinaryReader b, IvoAnimationBlock block)
    {
        int rotationCount = 0;
        int positionCount = 0;

        for (int i = 0; i < block.Controllers.Length; i++)
        {
            var ctrl = block.Controllers[i];
            uint boneHash = block.BoneHashes[i];
            long controllerStart = block.ControllerOffsets[i];

            // Parse rotation data (if present)
            if (ctrl.HasRotation && ctrl.NumRotKeys > 0)
            {
                // Parse rotation time keys
                List<float> rotTimes;
                if (ctrl.RotTimeOffset > 0)
                {
                    b.BaseStream.Seek(controllerStart + ctrl.RotTimeOffset, SeekOrigin.Begin);
                    rotTimes = IvoAnimationHelpers.ReadTimeKeys(b, ctrl.NumRotKeys, ctrl.RotFormatFlags);
                }
                else
                {
                    // No time offset - use sequential frame numbers
                    rotTimes = new List<float>(ctrl.NumRotKeys);
                    for (int t = 0; t < ctrl.NumRotKeys; t++)
                        rotTimes.Add(t);
                }

                block.RotationTimes[boneHash] = rotTimes;

                // Parse rotation data
                b.BaseStream.Seek(controllerStart + ctrl.RotDataOffset, SeekOrigin.Begin);
                var rotations = IvoAnimationHelpers.ReadRotationKeys(b, ctrl.NumRotKeys, ctrl.RotFormatFlags);
                block.Rotations[boneHash] = rotations;
                rotationCount++;
            }

            // Parse position data (if present)
            if (ctrl.HasPosition && ctrl.NumPosKeys > 0)
            {
                // Parse position time keys
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

                block.PositionTimes[boneHash] = posTimes;

                // Parse position data
                b.BaseStream.Seek(controllerStart + ctrl.PosDataOffset, SeekOrigin.Begin);
                var positions = IvoAnimationHelpers.ReadPositionKeys(b, ctrl.NumPosKeys, ctrl.PosFormatFlags);
                if (positions.Count > 0)
                {
                    block.Positions[boneHash] = positions;
                    positionCount++;
                }
            }
        }

        HelperMethods.Log(LogLevelEnum.Debug,
            $"  Parsed {rotationCount} rotation tracks, {positionCount} position tracks");
    }

    public override string ToString() =>
        $"ChunkIvoDBAData_900: TotalSize={TotalDataSize}, Animations={AnimationBlocks.Count}";
}
