using CgfConverter.Structs;
using System.Numerics;
using A = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace CgfConverterIntegrationTests.Extensions
{
    public static class AssertExtensions
    {
        public static void AreEqual(Matrix4x4 expected, Matrix4x4 actual, double delta)
        {
            A.AreEqual(expected.M11, actual.M11, delta);
            A.AreEqual(expected.M12, actual.M12, delta);
            A.AreEqual(expected.M13, actual.M13, delta);
            A.AreEqual(expected.M14, actual.M14, delta);
            A.AreEqual(expected.M21, actual.M21, delta);
            A.AreEqual(expected.M22, actual.M22, delta);
            A.AreEqual(expected.M23, actual.M23, delta);
            A.AreEqual(expected.M24, actual.M24, delta);
            A.AreEqual(expected.M31, actual.M31, delta);
            A.AreEqual(expected.M32, actual.M32, delta);
            A.AreEqual(expected.M33, actual.M33, delta);
            A.AreEqual(expected.M34, actual.M34, delta);
            A.AreEqual(expected.M41, actual.M41, delta);
            A.AreEqual(expected.M42, actual.M42, delta);
            A.AreEqual(expected.M43, actual.M43, delta);
            A.AreEqual(expected.M44, actual.M44, delta);
        }

        public static void AreEqual(Matrix3x3 expected, Matrix3x3 actual, double delta)
        {
            A.AreEqual(expected.M11, actual.M11, delta);
            A.AreEqual(expected.M12, actual.M12, delta);
            A.AreEqual(expected.M13, actual.M13, delta);
            A.AreEqual(expected.M21, actual.M21, delta);
            A.AreEqual(expected.M22, actual.M22, delta);
            A.AreEqual(expected.M23, actual.M23, delta);
            A.AreEqual(expected.M31, actual.M31, delta);
            A.AreEqual(expected.M32, actual.M32, delta);
            A.AreEqual(expected.M33, actual.M33, delta);
        }
    }
}
