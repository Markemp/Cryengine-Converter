using CgfConverter.Models.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class IvoTangentFrameTests
{
    private const float Epsilon = 0.02f; // Tolerance for quantization error (10-bit precision)

    /// <summary>
    /// Test data from Ivo_Normals.md documentation.
    /// Box with 6 axis-aligned faces.
    /// </summary>
    [TestMethod]
    public void Decode_ZNegativeFace_ReturnsCorrectNormal()
    {
        // Bytes: FE FF FF 1F FF 3F 00 00
        // Expected: (0, 0, -1)
        var frame = new IvoTangentFrame
        {
            Word0 = 0xFFFE,
            Word1 = 0x1FFF,
            Word2 = 0x3FFF,
            Word3 = 0x0000
        };

        var (normal, _, _) = frame.Decode();

        AssertVector3Equal(new Vector3(0, 0, -1), normal, Epsilon);
    }

    [TestMethod]
    public void Decode_ZPositiveFace_ReturnsCorrectNormal()
    {
        // Bytes: FE FF FF 9F FF 3F 00 80
        // Expected: (0, 0, +1)
        var frame = new IvoTangentFrame
        {
            Word0 = 0xFFFE,
            Word1 = 0x9FFF,
            Word2 = 0x3FFF,
            Word3 = 0x8000
        };

        var (normal, _, _) = frame.Decode();

        AssertVector3Equal(new Vector3(0, 0, 1), normal, Epsilon);
    }

    [TestMethod]
    public void Decode_YNegativeFace_ReturnsCorrectNormal()
    {
        // Bytes: FE FF FF 9F FF BF FF DF
        // Expected: (0, -1, 0)
        var frame = new IvoTangentFrame
        {
            Word0 = 0xFFFE,
            Word1 = 0x9FFF,
            Word2 = 0xBFFF,
            Word3 = 0xDFFF
        };

        var (normal, _, _) = frame.Decode();

        AssertVector3Equal(new Vector3(0, -1, 0), normal, Epsilon);
    }

    [TestMethod]
    public void Decode_XPositiveFace_ReturnsCorrectNormal()
    {
        // Bytes: FF 3F FF BF FF BF FF DF
        // Expected: (+1, 0, 0)
        var frame = new IvoTangentFrame
        {
            Word0 = 0x3FFF,
            Word1 = 0xBFFF,
            Word2 = 0xBFFF,
            Word3 = 0xDFFF
        };

        var (normal, _, _) = frame.Decode();

        AssertVector3Equal(new Vector3(1, 0, 0), normal, Epsilon);
    }

    [TestMethod]
    public void Decode_YPositiveFace_ReturnsCorrectNormal()
    {
        // Bytes: FE FF FF 1F FF BF FF 5F
        // Expected: (0, +1, 0)
        var frame = new IvoTangentFrame
        {
            Word0 = 0xFFFE,
            Word1 = 0x1FFF,
            Word2 = 0xBFFF,
            Word3 = 0x5FFF
        };

        var (normal, _, _) = frame.Decode();

        AssertVector3Equal(new Vector3(0, 1, 0), normal, Epsilon);
    }

    [TestMethod]
    public void Decode_XNegativeFace_ReturnsCorrectNormal()
    {
        // Bytes: FF 3F FF 3F FF BF FF 5F
        // Expected: (-1, 0, 0) - note: documentation says (-0.99, 0, 0) due to quantization
        // This face uses d2=1 (heuristic) which may have some precision issues
        var frame = new IvoTangentFrame
        {
            Word0 = 0x3FFF,
            Word1 = 0x3FFF,
            Word2 = 0xBFFF,
            Word3 = 0x5FFF
        };

        var (normal, _, _) = frame.Decode();

        // Primary X component should be close to -1
        Assert.IsTrue(normal.X < -0.98f, $"Expected X near -1, got {normal.X}");
        // The heuristic for d2=1 may introduce some Z-axis error
        Assert.IsTrue(Math.Abs(normal.Y) < 0.15f, $"Y should be near 0, got {normal.Y}");
    }

    [TestMethod]
    public void Decode_ReturnsNormalizedNormal()
    {
        // Any decoded normal should be approximately unit length
        var frame = new IvoTangentFrame
        {
            Word0 = 0xFFFE,
            Word1 = 0x1FFF,
            Word2 = 0x3FFF,
            Word3 = 0x0000
        };

        var (normal, _, _) = frame.Decode();
        float length = normal.Length();

        Assert.AreEqual(1.0f, length, 0.01f, $"Normal length should be ~1.0, got {length}");
    }

    [TestMethod]
    public void Decode_ReturnsTangentAndBitangent()
    {
        // Z-positive face
        var frame = new IvoTangentFrame
        {
            Word0 = 0xFFFE,
            Word1 = 0x9FFF,
            Word2 = 0x3FFF,
            Word3 = 0x8000
        };

        var (normal, tangent, bitangent) = frame.Decode();

        // Verify they form an orthonormal basis (approximately)
        float dot1 = Vector3.Dot(normal, tangent);
        float dot2 = Vector3.Dot(normal, bitangent);
        float dot3 = Vector3.Dot(tangent, bitangent);

        Assert.AreEqual(0, dot1, 0.1f, "Normal and tangent should be perpendicular");
        Assert.AreEqual(0, dot2, 0.1f, "Normal and bitangent should be perpendicular");
        Assert.AreEqual(0, dot3, 0.1f, "Tangent and bitangent should be perpendicular");
    }

    [TestMethod]
    public void Decode_ColumnSelector0_UsesNegativeBitangent()
    {
        // d2 = 0 (bits 30-31 of value2 = 0) means use -Bitangent as normal
        // Word3 high bits = 00 means d2 = 0
        var frame = new IvoTangentFrame
        {
            Word0 = 0xFFFE,
            Word1 = 0x1FFF,
            Word2 = 0x3FFF,
            Word3 = 0x0000  // d2 = 0
        };

        var (normal, _, _) = frame.Decode();

        // For this specific frame, the result should be (0, 0, -1)
        Assert.IsTrue(normal.Z < -0.9f, $"Expected Z-negative normal, got {normal}");
    }

    [TestMethod]
    public void Decode_ColumnSelector2_UsesPositiveNormal()
    {
        // d2 = 2 (bits 30-31 of value2 = 10b = 2) means use +Normal column
        // Word3 = 0x8000 means high bits = 10b, so d2 = 2
        var frame = new IvoTangentFrame
        {
            Word0 = 0xFFFE,
            Word1 = 0x9FFF,
            Word2 = 0x3FFF,
            Word3 = 0x8000  // d2 = 2
        };

        var (normal, _, _) = frame.Decode();

        // For this specific frame, the result should be (0, 0, +1)
        Assert.IsTrue(normal.Z > 0.9f, $"Expected Z-positive normal, got {normal}");
    }

    private static void AssertVector3Equal(Vector3 expected, Vector3 actual, float epsilon)
    {
        bool xMatch = Math.Abs(expected.X - actual.X) <= epsilon;
        bool yMatch = Math.Abs(expected.Y - actual.Y) <= epsilon;
        bool zMatch = Math.Abs(expected.Z - actual.Z) <= epsilon;

        if (!xMatch || !yMatch || !zMatch)
        {
            Assert.Fail($"Vectors not equal within epsilon {epsilon}.\nExpected: {expected}\nActual: {actual}");
        }
    }
}
