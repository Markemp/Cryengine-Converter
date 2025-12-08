using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CgfConverter.Services;
using CgfConverter.Utilities;
using CgfConverter.Utils;
using Extensions;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Controller chunk version 0x905 - DBA animation database format.
/// Supports both standard mode and in-place streaming mode (detected by negative offsets).
/// </summary>
internal sealed class ChunkController_905 : ChunkController
{
    private readonly TaggedLogger Log = new TaggedLogger(nameof(ChunkController_905));

    public uint NumKeyPos { get; internal set; }
    public uint NumKeyRot { get; internal set; }
    public uint NumKeyTime { get; internal set; }
    public uint NumAnims { get; internal set; }

    public List<List<float>>? KeyTimes { get; internal set; }
    public List<List<Vector3>>? KeyPositions { get; internal set; }
    public List<List<Quaternion>>? KeyRotations { get; internal set; }
    public List<Animation>? Animations { get; internal set; }

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        // Assume little endian for now
        ((EndiannessChangeableBinaryReader)b).IsBigEndian = false;

        NumKeyPos = b.ReadUInt32();
        NumKeyRot = b.ReadUInt32();
        NumKeyTime = b.ReadUInt32();
        NumAnims = b.ReadUInt32();

        Log.D($"Header: NumKeyPos={NumKeyPos}, NumKeyRot={NumKeyRot}, NumKeyTime={NumKeyTime}, NumAnims={NumAnims}");

        // Test: if there exists a number that exceeds 0x10000, it may not be in little endian.
        var endianCheck =
            (NumKeyPos >= 0x10000 ? 1 : 0) |
            (NumKeyRot >= 0x10000 ? 2 : 0) |
            (NumKeyTime >= 0x10000 ? 4 : 0) |
            (NumAnims >= 0x10000 ? 8 : 0);

        switch (endianCheck)
        {
            case 0: // most likely to be little endian
                break; // do nothing

            case 15: // most likely to be big endian
                NumKeyPos = BinaryPrimitives.ReverseEndianness(NumKeyPos);
                NumKeyRot = BinaryPrimitives.ReverseEndianness(NumKeyRot);
                NumKeyTime = BinaryPrimitives.ReverseEndianness(NumKeyTime);
                NumAnims = BinaryPrimitives.ReverseEndianness(NumAnims);
                ((EndiannessChangeableBinaryReader)b).IsBigEndian = true;
                Log.I($"Assuming big endian: {this}");
                break;

            default:
                Log.W($"Could not deduce endianness(endianCheck={endianCheck}); assuming little endian: {this}");
                break;
        }

        // Read size arrays (uint16 per track)
        var keyTimeLengths = Enumerable.Range(0, (int)NumKeyTime).Select(_ => b.ReadUInt16()).ToList();
        var keyTimeFormats = ReadKeyTimeFormatCounts(b);

        var keyPosLengths = Enumerable.Range(0, (int)NumKeyPos).Select(_ => b.ReadUInt16()).ToList();
        var keyPosFormats = ReadCompressionFormatCounts(b);

        var keyRotLengths = Enumerable.Range(0, (int)NumKeyRot).Select(_ => b.ReadUInt16()).ToList();
        var keyRotFormats = ReadCompressionFormatCounts(b);

        // Read offset arrays (int32 - signed for in-place streaming detection)
        var keyTimeOffsets = Enumerable.Range(0, (int)NumKeyTime).Select(_ => b.ReadInt32()).ToList();
        var keyPosOffsets = Enumerable.Range(0, (int)NumKeyPos).Select(_ => b.ReadInt32()).ToList();
        // Rotation offsets array includes terminator (+1)
        var keyRotOffsets = Enumerable.Range(0, (int)NumKeyRot + 1).Select(_ => b.ReadInt32()).ToList();

        // Detect in-place streaming mode (negative offsets)
        bool isInPlaceStream = NumKeyTime > 0 && keyTimeOffsets[0] < 0;
        Log.D($"In-place streaming mode: {isInPlaceStream}");

        Log.D($"Position after reading offsets: 0x{b.BaseStream.Position:X}");

        if (isInPlaceStream)
        {
            // In-place streaming: read padding length and skip padding
            var paddingLength = b.ReadUInt32();
            Log.D($"In-place padding length: {paddingLength}");
            if (paddingLength > 0)
                b.ReadBytes((int)paddingLength);
        }

        // Align to 4 bytes
        var pos = b.BaseStream.Position;
        if ((pos & 3) != 0)
            b.BaseStream.Position = (pos & ~3) + 4;

        Log.D($"Position after alignment: 0x{b.BaseStream.Position:X}");

        // Get track data size from terminator offset
        int trackDataSize = isInPlaceStream
            ? -keyRotOffsets[(int)NumKeyRot]
            : keyRotOffsets[(int)NumKeyRot];

        Log.D($"Track data size: {trackDataSize}, Stream length: {b.BaseStream.Length}");

        // Build complete offset chains for data reading
        // Add terminators: keyTime ends where keyPos starts, keyPos ends where keyRot starts
        // Note: In in-place mode, offsets are already negative - don't negate again!
        if (NumKeyRot > 0)
            keyPosOffsets.Add(keyRotOffsets[0]);
        if (NumKeyPos > 0)
            keyTimeOffsets.Add(keyPosOffsets[0]);

        long trackOffset;

        if (!isInPlaceStream)
        {
            // Standard mode: track data comes before animations
            trackOffset = b.BaseStream.Position;

            // Read track data
            ReadTrackData(b, trackOffset, trackDataSize, isInPlaceStream,
                keyTimeLengths, keyTimeFormats, keyTimeOffsets,
                keyPosLengths, keyPosFormats, keyPosOffsets,
                keyRotLengths, keyRotFormats, keyRotOffsets);

            // Seek past track data to read animations
            b.BaseStream.Position = trackOffset + trackDataSize;

            // Read animation entries
            Animations = new List<Animation>();
            for (int i = 0; i < NumAnims; i++)
            {
                Animations.Add(new Animation(b, isInPlaceStream: false));
            }
        }
        else
        {
            // In-place streaming: animations come first
            Log.D($"Reading {NumAnims} animations in in-place streaming mode at position 0x{b.BaseStream.Position:X}");
            Animations = new List<Animation>();
            for (int i = 0; i < NumAnims; i++)
            {
                Log.D($"Reading animation {i} at position 0x{b.BaseStream.Position:X}");
                Animations.Add(new Animation(b, isInPlaceStream: true));
            }

            // Controller headers block (for all animations)
            int totalControllers = Animations.Sum(a => a.ControllerCount);
            Log.D($"Reading {totalControllers} controller headers at position 0x{b.BaseStream.Position:X}");
            foreach (var anim in Animations)
            {
                anim.Controllers = new List<CControllerInfo>();
                for (int j = 0; j < anim.ControllerCount; j++)
                    anim.Controllers.Add(new CControllerInfo(b));
            }

            // Align to 4 bytes before track data (per 010 template)
            var posBeforeAlign = b.BaseStream.Position;
            if ((posBeforeAlign & 3) != 0)
                b.BaseStream.Position = (posBeforeAlign & ~3) + 4;

            // Track data is at the end for in-place streaming
            trackOffset = b.BaseStream.Position;
            Log.D($"Reading track data at offset 0x{trackOffset:X} (aligned from 0x{posBeforeAlign:X})");

            ReadTrackData(b, trackOffset, trackDataSize, isInPlaceStream,
                keyTimeLengths, keyTimeFormats, keyTimeOffsets,
                keyPosLengths, keyPosFormats, keyPosOffsets,
                keyRotLengths, keyRotFormats, keyRotOffsets);
        }
    }

    private List<uint> ReadKeyTimeFormatCounts(BinaryReader b)
    {
        // 7 format types: eF32, eUINT16, eByte, eF32StartStop, eUINT16StartStop, eByteStartStop, eBitset
        return Enumerable.Range(0, 7).Select(_ => b.ReadUInt32()).ToList();
    }

    private List<uint> ReadCompressionFormatCounts(BinaryReader b)
    {
        // 9 format types: eNoCompress through eSmallTree64BitExtQuat (not eAutomaticQuat)
        return Enumerable.Range(0, 9).Select(_ => b.ReadUInt32()).ToList();
    }

    private void ReadTrackData(BinaryReader b, long trackOffset, int trackDataSize, bool isInPlaceStream,
        List<ushort> keyTimeLengths, List<uint> keyTimeFormats, List<int> keyTimeOffsets,
        List<ushort> keyPosLengths, List<uint> keyPosFormats, List<int> keyPosOffsets,
        List<ushort> keyRotLengths, List<uint> keyRotFormats, List<int> keyRotOffsets)
    {
        // Clone format counts since we decrement them
        var timeFormats = new List<uint>(keyTimeFormats);
        var posFormats = new List<uint>(keyPosFormats);
        var rotFormats = new List<uint>(keyRotFormats);

        // In-place streaming: offsets are negative and relative to END of track data
        // Standard mode: offsets are positive and relative to START of track data
        long trackDataEnd = trackOffset + trackDataSize;

        // Read key times
        KeyTimes = new List<List<float>>();
        for (int i = 0; i < keyTimeLengths.Count; i++)
        {
            long position = isInPlaceStream
                ? trackDataEnd + keyTimeOffsets[i]  // negative offset from end
                : trackOffset + keyTimeOffsets[i];   // positive offset from start
            b.BaseStream.Position = position;
            KeyTimes.Add(ReadKeyTimeTrack(b, keyTimeLengths[i], timeFormats));
        }

        // Read positions
        KeyPositions = new List<List<Vector3>>();
        for (int i = 0; i < keyPosLengths.Count; i++)
        {
            long position = isInPlaceStream
                ? trackDataEnd + keyPosOffsets[i]
                : trackOffset + keyPosOffsets[i];
            b.BaseStream.Position = position;
            KeyPositions.Add(ReadPositionTrack(b, keyPosLengths[i], posFormats));
        }

        // Read rotations
        KeyRotations = new List<List<Quaternion>>();
        for (int i = 0; i < keyRotLengths.Count; i++)
        {
            long position = isInPlaceStream
                ? trackDataEnd + keyRotOffsets[i]
                : trackOffset + keyRotOffsets[i];
            b.BaseStream.Position = position;
            KeyRotations.Add(ReadRotationTrack(b, keyRotLengths[i], rotFormats));
        }
    }

    private List<float> ReadKeyTimeTrack(BinaryReader b, int elementCount, List<uint> formats)
    {
        List<float> data;

        if (formats[(int)EKeyTimesFormat.eF32] > 0)
        {
            --formats[(int)EKeyTimesFormat.eF32];
            data = Enumerable.Range(0, elementCount).Select(_ => b.ReadSingle()).ToList();
        }
        else if (formats[(int)EKeyTimesFormat.eUINT16] > 0)
        {
            --formats[(int)EKeyTimesFormat.eUINT16];
            data = Enumerable.Range(0, elementCount).Select(_ => (float)b.ReadUInt16()).ToList();
        }
        else if (formats[(int)EKeyTimesFormat.eByte] > 0)
        {
            --formats[(int)EKeyTimesFormat.eByte];
            data = b.ReadBytes(elementCount).Select(x => (float)x).ToList();
        }
        else if (formats[(int)EKeyTimesFormat.eF32StartStop] > 0)
        {
            throw new NotImplementedException("eF32StartStop key time format");
        }
        else if (formats[(int)EKeyTimesFormat.eUINT16StartStop] > 0)
        {
            throw new NotImplementedException("eUINT16StartStop key time format");
        }
        else if (formats[(int)EKeyTimesFormat.eByteStartStop] > 0)
        {
            throw new NotImplementedException("eByteStartStop key time format");
        }
        else if (formats[(int)EKeyTimesFormat.eBitset] > 0)
        {
            --formats[(int)EKeyTimesFormat.eBitset];
            // eBitset: elementCount is the number of uint16s in the track
            var start = b.ReadUInt16();
            var end = b.ReadUInt16();
            var size = b.ReadUInt16();

            data = new List<float>(size);
            var keyValue = start;
            for (var i = 3; i < elementCount; i++)
            {
                var curr = b.ReadUInt16();
                for (var j = 0; j < 16; ++j)
                {
                    if (((curr >> j) & 1) != 0)
                        data.Add(keyValue);
                    ++keyValue;
                }
            }

            if (data.Count != size)
                HelperMethods.Log(LogLevelEnum.Warning, "eBitset: Expected {0} items, got {1} items", size, data.Count);
            if (data.Any() && Math.Abs(data[^1] - end) > float.Epsilon)
                HelperMethods.Log(LogLevelEnum.Warning, "eBitset: Expected last as {0}, got {1}", end, data[^1]);
        }
        else
        {
            throw new Exception("sum(count per format) != count of keytimes");
        }

        // Get rid of decreasing entries at end (zero-pads)
        while (data.Count >= 2 && data[^2] > data[^1])
            data.RemoveAt(data.Count - 1);

        return data;
    }

    private List<Vector3> ReadPositionTrack(BinaryReader b, int elementCount, List<uint> formats)
    {
        List<Vector3> data;

        if (formats[(int)ECompressionFormat.eNoCompress] > 0)
        {
            throw new NotImplementedException("eNoCompress position format");
        }
        else if (formats[(int)ECompressionFormat.eNoCompressQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eNoCompressQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => b.ReadQuaternion().DropW()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eNoCompressVec3] > 0)
        {
            --formats[(int)ECompressionFormat.eNoCompressVec3];
            data = Enumerable.Range(0, elementCount).Select(_ => b.ReadVector3()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eShotInt3Quat] > 0)
        {
            --formats[(int)ECompressionFormat.eShotInt3Quat];
            data = Enumerable.Range(0, elementCount).Select(_ => ((Quaternion)b.ReadShortInt3Quat()).DropW()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eSmallTreeDWORDQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eSmallTreeDWORDQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => ((Quaternion)b.ReadSmallTreeDWORDQuat()).DropW()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eSmallTree48BitQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eSmallTree48BitQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => ((Quaternion)b.ReadSmallTree48BitQuat()).DropW()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eSmallTree64BitQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eSmallTree64BitQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => ((Quaternion)b.ReadSmallTree64BitQuat()).DropW()).ToList();
        }
        else if (formats[(int)ECompressionFormat.ePolarQuat] > 0)
        {
            throw new NotImplementedException("ePolarQuat position format");
        }
        else if (formats[(int)ECompressionFormat.eSmallTree64BitExtQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eSmallTree64BitExtQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => ((Quaternion)b.ReadSmallTree64BitExtQuat()).DropW()).ToList();
        }
        else
        {
            throw new Exception("sum(count per format) != count of keypos");
        }

        return data;
    }

    private List<Quaternion> ReadRotationTrack(BinaryReader b, int elementCount, List<uint> formats)
    {
        List<Quaternion> data;

        if (formats[(int)ECompressionFormat.eNoCompress] > 0)
        {
            throw new NotImplementedException("eNoCompress rotation format");
        }
        else if (formats[(int)ECompressionFormat.eNoCompressQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eNoCompressQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => b.ReadQuaternion()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eNoCompressVec3] > 0)
        {
            --formats[(int)ECompressionFormat.eNoCompressVec3];
            data = Enumerable.Range(0, elementCount).Select(_ => new Quaternion(b.ReadVector3(), float.NaN)).ToList();
        }
        else if (formats[(int)ECompressionFormat.eShotInt3Quat] > 0)
        {
            --formats[(int)ECompressionFormat.eShotInt3Quat];
            data = Enumerable.Range(0, elementCount).Select(_ => (Quaternion)b.ReadShortInt3Quat()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eSmallTreeDWORDQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eSmallTreeDWORDQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => (Quaternion)b.ReadSmallTreeDWORDQuat()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eSmallTree48BitQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eSmallTree48BitQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => (Quaternion)b.ReadSmallTree48BitQuat()).ToList();
        }
        else if (formats[(int)ECompressionFormat.eSmallTree64BitQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eSmallTree64BitQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => (Quaternion)b.ReadSmallTree64BitQuat()).ToList();
        }
        else if (formats[(int)ECompressionFormat.ePolarQuat] > 0)
        {
            throw new NotImplementedException("ePolarQuat rotation format");
        }
        else if (formats[(int)ECompressionFormat.eSmallTree64BitExtQuat] > 0)
        {
            --formats[(int)ECompressionFormat.eSmallTree64BitExtQuat];
            data = Enumerable.Range(0, elementCount).Select(_ => (Quaternion)b.ReadSmallTree64BitExtQuat()).ToList();
        }
        else
        {
            throw new Exception("sum(count per format) != count of keyRot");
        }

        return data;
    }

    public enum EKeyTimesFormat
    {
        eF32,
        eUINT16,
        eByte,
        eF32StartStop,
        eUINT16StartStop,
        eByteStartStop,
        eBitset,
    }

    public enum ECompressionFormat
    {
        eNoCompress = 0,
        eNoCompressQuat = 1,
        eNoCompressVec3 = 2,
        eShotInt3Quat = 3,
        eSmallTreeDWORDQuat = 4,
        eSmallTree48BitQuat = 5,
        eSmallTree64BitQuat = 6,
        ePolarQuat = 7,
        eSmallTree64BitExtQuat = 8,
        eAutomaticQuat = 9
    }

    [Flags]
    public enum AssetFlags : uint
    {
        Additive = 0x001,
        Cycle = 0x002,
        Loaded = 0x004,
        Lmg = 0x008,
        LmgValid = 0x020,
        Created = 0x800,
        Requested = 0x1000,
        Ondemand = 0x2000,
        Aimpose = 0x4000,
        AimposeUnloaded = 0x8000,
        NotFound = 0x10000,
        Tcb = 0x20000,
        Internaltype = 0x40000,
        BigEndian = 0x80000000,
    }

    public struct MotionParams905
    {
        public AssetFlags AssetFlags;
        public uint Compression;

        public int TicksPerFrame;
        public float SecsPerTick;
        public int Start;
        public int End;

        public float MoveSpeed;
        public float TurnSpeed;
        public float AssetTurn;
        public float Distance;
        public float Slope;

        public Quaternion StartLocationQ;
        public Vector3 StartLocationV;
        public Quaternion EndLocationQ;
        public Vector3 EndLocationV;

        public float LHeelStart;
        public float LHeelEnd;
        public float LToe0Start;
        public float LToe0End;
        public float RHeelStart;
        public float RHeelEnd;
        public float RToe0Start;
        public float RToe0End;

        public MotionParams905()
        {
            AssetFlags = 0;
            Compression = 0xFFFFFFFFu;
            TicksPerFrame = 0;
            SecsPerTick = 0;
            Start = 0;
            End = 0;
            MoveSpeed = -1;
            TurnSpeed = -1;
            AssetTurn = -1;
            Distance = -1;
            Slope = -1;
            StartLocationQ = Quaternion.Identity;
            StartLocationV = Vector3.One;
            EndLocationQ = Quaternion.Identity;
            EndLocationV = Vector3.One;
            LHeelStart = -1;
            LHeelEnd = -1;
            LToe0Start = -1;
            LToe0End = -1;
            RHeelStart = -1;
            RHeelEnd = -1;
            RToe0Start = -1;
            RToe0End = -1;
        }

        public MotionParams905(BinaryReader r)
        {
            AssetFlags = (AssetFlags)r.ReadUInt32();
            Compression = r.ReadUInt32();
            TicksPerFrame = r.ReadInt32();
            SecsPerTick = r.ReadSingle();
            Start = r.ReadInt32();
            End = r.ReadInt32();
            MoveSpeed = r.ReadSingle();
            TurnSpeed = r.ReadSingle();
            AssetTurn = r.ReadSingle();
            Distance = r.ReadSingle();
            Slope = r.ReadSingle();
            StartLocationQ = r.ReadQuaternion();
            StartLocationV = r.ReadVector3();
            EndLocationQ = r.ReadQuaternion();
            EndLocationV = r.ReadVector3();
            LHeelStart = r.ReadSingle();
            LHeelEnd = r.ReadSingle();
            LToe0Start = r.ReadSingle();
            LToe0End = r.ReadSingle();
            RHeelStart = r.ReadSingle();
            RHeelEnd = r.ReadSingle();
            RToe0Start = r.ReadSingle();
            RToe0End = r.ReadSingle();
        }
    }

    public struct CControllerInfo
    {
        public const int InvalidTrack = -1;
        public uint ControllerID;
        public int PosKeyTimeTrack;
        public int PosTrack;
        public int RotKeyTimeTrack;
        public int RotTrack;

        public bool HasPosTrack => PosTrack != InvalidTrack && PosKeyTimeTrack != InvalidTrack;
        public bool HasRotTrack => RotTrack != InvalidTrack && RotKeyTimeTrack != InvalidTrack;

        public CControllerInfo()
        {
            ControllerID = int.MaxValue;
            PosKeyTimeTrack = PosTrack = RotKeyTimeTrack = RotTrack = InvalidTrack;
        }

        public CControllerInfo(BinaryReader r)
        {
            ControllerID = r.ReadUInt32();
            PosKeyTimeTrack = r.ReadInt32();
            PosTrack = r.ReadInt32();
            RotKeyTimeTrack = r.ReadInt32();
            RotTrack = r.ReadInt32();
        }
    }

    public class Animation
    {
        public string Name;
        public MotionParams905 MotionParams;
        public byte[] FootPlanBits;
        public int ControllerCount;
        public int OffsetToControllerHeaders; // Only used in in-place streaming mode
        public List<CControllerInfo> Controllers;

        public Animation(BinaryReader b, bool isInPlaceStream)
        {
            var nameLen = b.ReadUInt16();
            Name = Encoding.UTF8.GetString(b.ReadBytes(nameLen));

            MotionParams = new MotionParams905(b);

            var footPlantBitsCount = b.ReadUInt16();
            FootPlanBits = b.ReadBytes(footPlantBitsCount);

            ControllerCount = b.ReadUInt16();

            if (isInPlaceStream)
            {
                // In-place streaming: controllers are in separate block, just store offset
                OffsetToControllerHeaders = b.ReadInt32();
                Controllers = new List<CControllerInfo>(); // Will be filled later
            }
            else
            {
                // Standard: controllers inline
                Controllers = new List<CControllerInfo>();
                for (var j = 0; j < ControllerCount; j++)
                    Controllers.Add(new CControllerInfo(b));
            }
        }
    }
}
