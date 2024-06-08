using CgfConverter.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CgfConverterIntegrationTests.Extensions;

public static class AssertExtensions
{
    public static void AreEqual(Matrix4x4 expected, Matrix4x4 actual, double delta)
    {
        Assert.AreEqual(expected.M11, actual.M11, delta);
        Assert.AreEqual(expected.M12, actual.M12, delta);
        Assert.AreEqual(expected.M13, actual.M13, delta);
        Assert.AreEqual(expected.M14, actual.M14, delta);
        Assert.AreEqual(expected.M21, actual.M21, delta);
        Assert.AreEqual(expected.M22, actual.M22, delta);
        Assert.AreEqual(expected.M23, actual.M23, delta);
        Assert.AreEqual(expected.M24, actual.M24, delta);
        Assert.AreEqual(expected.M31, actual.M31, delta);
        Assert.AreEqual(expected.M32, actual.M32, delta);
        Assert.AreEqual(expected.M33, actual.M33, delta);
        Assert.AreEqual(expected.M34, actual.M34, delta);
        Assert.AreEqual(expected.M41, actual.M41, delta);
        Assert.AreEqual(expected.M42, actual.M42, delta);
        Assert.AreEqual(expected.M43, actual.M43, delta);
        Assert.AreEqual(expected.M44, actual.M44, delta);
    }

    public static void AreEqual(Matrix3x3 expected, Matrix3x3 actual, double delta)
    {
        Assert.AreEqual(expected.M11, actual.M11, delta);
        Assert.AreEqual(expected.M12, actual.M12, delta);
        Assert.AreEqual(expected.M13, actual.M13, delta);
        Assert.AreEqual(expected.M21, actual.M21, delta);
        Assert.AreEqual(expected.M22, actual.M22, delta);
        Assert.AreEqual(expected.M23, actual.M23, delta);
        Assert.AreEqual(expected.M31, actual.M31, delta);
        Assert.AreEqual(expected.M32, actual.M32, delta);
        Assert.AreEqual(expected.M33, actual.M33, delta);
    }

    public static void AreEqual(System.Collections.Generic.List<float> expected, System.Collections.Generic.List<float> actual, double delta = 0.0)
    {
        Assert.AreEqual(expected.Count, actual.Count, "Size of 2 lists don't match");
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.AreEqual(expected[i], actual[i], delta);
        }   
    }
}
