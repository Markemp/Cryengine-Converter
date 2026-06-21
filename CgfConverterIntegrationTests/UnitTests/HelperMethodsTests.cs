using CgfConverter.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class HelperMethodsTests
{
    [TestMethod]
    public void GetNullSeparatedStrings_BasicAscii_ReturnsCorrectStrings()
    {
        // "bone1\0bone2\0bone3\0"
        var buffer = Encoding.UTF8.GetBytes("bone1\0bone2\0bone3\0");

        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        var result = HelperMethods.GetNullSeparatedStrings(3, reader);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("bone1", result[0]);
        Assert.AreEqual("bone2", result[1]);
        Assert.AreEqual("bone3", result[2]);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_Utf8MultiByte_ReturnsCorrectStrings()
    {
        // Test with multi-byte UTF-8 characters (e.g., accented characters, emoji)
        var buffer = Encoding.UTF8.GetBytes("café\0naïve\0日本語\0");

        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        var result = HelperMethods.GetNullSeparatedStrings(3, reader);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("café", result[0]);
        Assert.AreEqual("naïve", result[1]);
        Assert.AreEqual("日本語", result[2]);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_EmptyStrings_ReturnsEmptyStrings()
    {
        // Three empty strings: "\0\0\0"
        var buffer = new byte[] { 0, 0, 0 };

        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        var result = HelperMethods.GetNullSeparatedStrings(3, reader);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("", result[0]);
        Assert.AreEqual("", result[1]);
        Assert.AreEqual("", result[2]);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_SingleString_ReturnsCorrectString()
    {
        var buffer = Encoding.UTF8.GetBytes("single_bone\0");

        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        var result = HelperMethods.GetNullSeparatedStrings(1, reader);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("single_bone", result[0]);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_Bounded_ReadsExactByteCount()
    {
        // Buffer contains "bone1\0bone2\0" followed by garbage
        var stringData = Encoding.UTF8.GetBytes("bone1\0bone2\0");
        var garbage = new byte[] { 0xFF, 0xFE, 0xFD, 0xFC };
        var buffer = new byte[stringData.Length + garbage.Length];
        stringData.CopyTo(buffer, 0);
        garbage.CopyTo(buffer, stringData.Length);

        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        // Only read the string data portion
        var result = HelperMethods.GetNullSeparatedStrings(2, stringData.Length, reader);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("bone1", result[0]);
        Assert.AreEqual("bone2", result[1]);

        // Verify stream position is exactly at end of string table
        Assert.AreEqual(stringData.Length, stream.Position);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_Bounded_StopsAtBufferEnd()
    {
        // Request more strings than exist in buffer
        var buffer = Encoding.UTF8.GetBytes("bone1\0bone2\0");

        using var stream = new MemoryStream(buffer);
        using var reader = new BinaryReader(stream);

        // Ask for 5 strings but buffer only has 2
        var result = HelperMethods.GetNullSeparatedStrings(5, buffer.Length, reader);

        // Should return only what's available
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("bone1", result[0]);
        Assert.AreEqual("bone2", result[1]);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_ByteArray_ParsesCorrectly()
    {
        var buffer = Encoding.UTF8.GetBytes("alpha\0beta\0gamma\0");

        var result = HelperMethods.GetNullSeparatedStrings(3, buffer);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("alpha", result[0]);
        Assert.AreEqual("beta", result[1]);
        Assert.AreEqual("gamma", result[2]);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_ByteArray_HandlesNoTrailingNull()
    {
        // Last string without null terminator
        var buffer = Encoding.UTF8.GetBytes("bone1\0bone2\0bone3");

        var result = HelperMethods.GetNullSeparatedStrings(3, buffer);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("bone1", result[0]);
        Assert.AreEqual("bone2", result[1]);
        Assert.AreEqual("bone3", result[2]);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_ByteArray_EmptyBuffer()
    {
        var buffer = Array.Empty<byte>();

        var result = HelperMethods.GetNullSeparatedStrings(3, buffer);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetNullSeparatedStrings_Bounded_Utf8MultiByte()
    {
        // Ensure bounded version handles UTF-8 correctly
        var stringData = Encoding.UTF8.GetBytes("Ñoño\0größe\0");

        using var stream = new MemoryStream(stringData);
        using var reader = new BinaryReader(stream);

        var result = HelperMethods.GetNullSeparatedStrings(2, stringData.Length, reader);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Ñoño", result[0]);
        Assert.AreEqual("größe", result[1]);
    }
}
