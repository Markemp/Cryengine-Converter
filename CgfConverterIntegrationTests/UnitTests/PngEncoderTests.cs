using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.IO.Hashing;
using System.Text;
using CgfConverter.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class PngEncoderTests
{
    [TestMethod]
    [DataRow(true, 1, 1)]
    [DataRow(true, 7, 5)]
    [DataRow(false, 7, 5)]
    [DataRow(true, 64, 33)]
    [DataRow(false, 33, 64)]
    public void Encode_RoundTripsPixels(bool hasAlpha, int width, int height)
    {
        int bpp = hasAlpha ? 4 : 3;
        var pixels = new byte[width * height * bpp];
        // Mix of gradients and noise so every filter type gets chosen somewhere.
        var rng = new Random(1234);
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = (i / bpp) % 3 == 0 ? (byte)rng.Next(256) : (byte)(i * 7 + (i / (width * bpp)) * 13);

        byte[] png = PngEncoder.Encode(pixels, width, height, hasAlpha);

        var (w, h, colorType, decoded) = Decode(png);
        Assert.AreEqual(width, w);
        Assert.AreEqual(height, h);
        Assert.AreEqual(hasAlpha ? 6 : 2, colorType);
        CollectionAssert.AreEqual(pixels, decoded);
    }

    [TestMethod]
    public void Encode_IgnoresTrailingData()
    {
        var pixels = new byte[2 * 2 * 4 + 10];
        pixels[0] = 200;
        var (_, _, _, decoded) = Decode(PngEncoder.Encode(pixels, 2, 2, hasAlpha: true));
        Assert.AreEqual(16, decoded.Length);
        Assert.AreEqual(200, decoded[0]);
    }

    [TestMethod]
    public void Encode_TooLittleData_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() => PngEncoder.Encode(new byte[11], 2, 2, hasAlpha: false));
    }

    /// <summary>Independent minimal PNG decoder: validates signature and CRCs, inflates and unfilters IDAT.</summary>
    private static (int Width, int Height, int ColorType, byte[] Pixels) Decode(byte[] png)
    {
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        CollectionAssert.AreEqual(signature, png[..8]);

        int width = 0, height = 0, colorType = 0;
        var idat = new MemoryStream();
        int pos = 8;
        string type;
        do
        {
            int length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(pos));
            type = Encoding.ASCII.GetString(png, pos + 4, 4);
            var data = png.AsSpan(pos + 8, length);
            uint crc = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(pos + 8 + length));
            Assert.AreEqual(Crc32.HashToUInt32(png.AsSpan(pos + 4, length + 4)), crc, $"Bad CRC on {type}");

            if (type == "IHDR")
            {
                width = BinaryPrimitives.ReadInt32BigEndian(data);
                height = BinaryPrimitives.ReadInt32BigEndian(data[4..]);
                Assert.AreEqual(8, data[8], "bit depth");
                colorType = data[9];
            }
            else if (type == "IDAT")
                idat.Write(data);

            pos += 12 + length;
        } while (type != "IEND");
        Assert.AreEqual(png.Length, pos, "Trailing bytes after IEND");

        idat.Position = 0;
        using var inflated = new MemoryStream();
        using (var zlib = new ZLibStream(idat, CompressionMode.Decompress))
            zlib.CopyTo(inflated);
        byte[] raw = inflated.ToArray();

        int bpp = colorType == 6 ? 4 : 3;
        int stride = width * bpp;
        Assert.AreEqual(height * (stride + 1), raw.Length);
        var pixels = new byte[height * stride];
        for (int y = 0; y < height; y++)
        {
            byte filter = raw[y * (stride + 1)];
            for (int i = 0; i < stride; i++)
            {
                int a = i >= bpp ? pixels[y * stride + i - bpp] : 0;
                int b = y > 0 ? pixels[(y - 1) * stride + i] : 0;
                int c = y > 0 && i >= bpp ? pixels[(y - 1) * stride + i - bpp] : 0;
                int predictor = filter switch
                {
                    0 => 0,
                    1 => a,
                    2 => b,
                    3 => (a + b) / 2,
                    4 => PaethReference(a, b, c),
                    _ => throw new AssertFailedException($"Invalid filter type {filter}"),
                };
                pixels[y * stride + i] = (byte)(raw[y * (stride + 1) + 1 + i] + predictor);
            }
        }
        return (width, height, colorType, pixels);
    }

    private static int PaethReference(int a, int b, int c)
    {
        int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }
}
