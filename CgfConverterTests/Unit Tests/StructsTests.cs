using CgfConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.Unit_Tests
{
    [TestClass]
    public class StructsTests
    {
        [TestMethod]
        public void GetTransformFromParts_TwoParameters()
        {
            Matrix44 actual = new Matrix44();
            Vector3 vector3 = GetTestVector3();
            Matrix33 rotation = GetTestMatrix33();

            Matrix44 matrix = actual.GetTransformFromParts(vector3, rotation);
            Assert.AreEqual(0.11f, matrix.m11);
        }

        private Vector3 GetTestVector3()
        {
            Vector3 vector = new Vector3();
            vector.x = 0.5f;
            vector.y = 0.6f;
            vector.z = -0.5f;

            return vector;
        }

        private Matrix33 GetTestMatrix33()
        {
            Matrix33 matrix = new Matrix33();
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
