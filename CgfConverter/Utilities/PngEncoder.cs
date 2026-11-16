using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.IO.Hashing;
using System.Text;

namespace CgfConverter.Utilities;

/// <summary>
/// Minimal PNG writer: 8-bit truecolor (RGB) or truecolor with alpha (RGBA), non-interlaced.
/// Each scanline uses the PNG "minimum sum of absolute differences" adaptive filter heuristic,
/// which is what most encoders (libpng, ImageSharp) default to.
/// </summary>
public static class PngEncoder
{
    private static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>Encodes interleaved 8-bit pixels (RGBA if <paramref name="hasAlpha"/>, else RGB) as a PNG.</summary>
    public static byte[] Encode(ReadOnlySpan<byte> pixels, int width, int height, bool hasAlpha)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), $"Invalid image size {width}x{height}.");

        int bpp = hasAlpha ? 4 : 3;
        int stride = checked(width * bpp);
        int imageBytes = checked(stride * height);
        if (pixels.Length < imageBytes)
            throw new ArgumentException($"Expected {imageBytes} bytes for {width}x{height}x{bpp}, got {pixels.Length}.", nameof(pixels));
        pixels = pixels[..imageBytes]; // like ImageSharp's LoadPixelData, ignore any trailing data

        using var output = new MemoryStream();
        output.Write(Signature);

        Span<byte> ihdr = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr, width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr[4..], height);
        ihdr[8] = 8;                          // bit depth
        ihdr[9] = (byte)(hasAlpha ? 6 : 2);   // color type: 6 = RGBA, 2 = RGB
        ihdr[10] = 0;                         // compression: deflate
        ihdr[11] = 0;                         // filter method: adaptive
        ihdr[12] = 0;                         // interlace: none
        WriteChunk(output, "IHDR", ihdr);

        WriteChunk(output, "IDAT", CompressScanlines(pixels, stride, height, bpp));
        WriteChunk(output, "IEND", []);

        return output.ToArray();
    }

    private static byte[] CompressScanlines(ReadOnlySpan<byte> pixels, int stride, int height, int bpp)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            // candidates[f] holds the filter-type byte followed by the row filtered with filter f.
            var candidates = new byte[5][];
            for (int f = 0; f < 5; f++)
            {
                candidates[f] = new byte[stride + 1];
                candidates[f][0] = (byte)f;
            }

            ReadOnlySpan<byte> previous = new byte[stride]; // row above the first row is all zeros
            for (int y = 0; y < height; y++)
            {
                ReadOnlySpan<byte> row = pixels.Slice(y * stride, stride);
                int best = 0;
                long bestScore = long.MaxValue;
                for (int f = 0; f < 5; f++)
                {
                    long score = FilterRow(f, row, previous, candidates[f].AsSpan(1), bpp);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = f;
                    }
                }
                zlib.Write(candidates[best]);
                previous = row;
            }
        }
        return compressed.ToArray();
    }

    /// <summary>Applies PNG filter <paramref name="type"/> and returns the sum of absolute (signed) values.</summary>
    private static long FilterRow(int type, ReadOnlySpan<byte> row, ReadOnlySpan<byte> previous, Span<byte> dest, int bpp)
    {
        long score = 0;
        for (int i = 0; i < row.Length; i++)
        {
            int left = i >= bpp ? row[i - bpp] : 0;
            int up = previous[i];
            int upLeft = i >= bpp ? previous[i - bpp] : 0;
            int predictor = type switch
            {
                0 => 0,
                1 => left,
                2 => up,
                3 => (left + up) >> 1,
                _ => Paeth(left, up, upLeft),
            };
            byte value = (byte)(row[i] - predictor);
            dest[i] = value;
            score += Math.Abs((int)(sbyte)value); // widen first: Math.Abs(sbyte.MinValue) throws
        }
        return score;
    }

    internal static int Paeth(int a, int b, int c)
    {
        int p = a + b - c;
        int pa = Math.Abs(p - a);
        int pb = Math.Abs(p - b);
        int pc = Math.Abs(p - c);
        if (pa <= pb && pa <= pc) return a;
        return pb <= pc ? b : c;
    }

    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> header = stackalloc byte[8];
        BinaryPrimitives.WriteInt32BigEndian(header, data.Length);
        Encoding.ASCII.GetBytes(type, header[4..]);
        output.Write(header);
        output.Write(data);

        // CRC covers the chunk type and data, not the length.
        var crc = new Crc32();
        crc.Append(header[4..]);
        crc.Append(data);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, crc.GetCurrentHashAsUInt32());
        output.Write(crcBytes);
    }
}
