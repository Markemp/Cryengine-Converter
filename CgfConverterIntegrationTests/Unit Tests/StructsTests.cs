using CgfConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CgfConverterTests.Unit_Tests
{
    [TestClass]
    public class StructsTests
    {
        [TestMethod]
        public void GetTransformFromParts_TwoParameters()
        {
            Matrix4x4 actual = new Matrix4x4();
            Vector3 vector3 = GetTestVector3();
            Matrix3x3 rotation = GetTestMatrix33();

            Matrix4x4 matrix = actual.GetTransformFromParts(vector3, rotation);
            Assert.AreEqual(0.11f, matrix.M11);
        }

        private Vector3 GetTestVector3()
        {
            Vector3 vector = new Vector3();
            vector.X = 0.5f;
            vector.Y = 0.6f;
            vector.Z = -0.5f;

            return vector;
        }

        private Matrix3x3 GetTestMatrix33()
        {
            Matrix3x3 matrix = new Matrix3x3();
            matrix.m11 = 0.11f;
            matrix.m12 = 0.12f;
            matrix.m13 = 0.13f;
            matrix.m21 = 0.21f;
            matrix.m22 = 0.22f;
            matrix.m23 = 0.23f;
            matrix.m31 = 0.31f;
            matrix.m32 = 0.32f;
            matrix.m33 = 0.33f;

            return matrix;
        }
    }
}
