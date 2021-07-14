using CgfConverter.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;
using Extensions;
using CgfConverterTests.TestUtilities;

namespace CgfConverterIntegrationTests.Unit_Tests
{
    [TestClass]
    public class Matrix4x4Tests
    {
        // Tests
        // SetRotationFromQuaternion
        // SetRotationFromMatrix3x3
        // SetTranslationFromVector3
        // SetScaleFromVector3
        // GetRotationAsQuaternion
        // GetRotationAsMatrix3x3
        // CreateFromQuaternion(Quaternion)

        private const float delta = 0.000001f;

        [TestMethod]
        public void SetRotationAndTranslationFromQuaternion()
        {
            var quat = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
            var translation = new Vector3(2.0f, 3.0f, 4.0f);

            var matrix = Matrix4x4.CreateFromQuaternion(quat);
            matrix.Translation = translation;

            Assert.AreEqual(1.0, matrix.M11, delta);
            Assert.AreEqual(0.0, matrix.M21, delta);
            Assert.AreEqual(2.0, matrix.M41, delta);
            Assert.AreEqual(3.0, matrix.M42, delta);
            Assert.AreEqual(4.0, matrix.M43, delta);
        }

        [TestMethod]
        public void GetTransformFromParts_TwoParameters()
        {
            Vector3 vector3 = GetTestVector3();
            Matrix3x3 rotation = GetTestMatrix33();

            Matrix4x4 matrix = Matrix4x4Extensions.CreateTransformFromParts(vector3, rotation);
            Assert.AreEqual(0.11f, matrix.M11);
            Vector3 scale = matrix.GetScale();
        }

        [TestMethod]
        public void Matrix4x4_CreateScale_ProperElements()
        {
            var matrix = Matrix4x4.CreateScale(GetTestVector3());

            Assert.AreEqual(0.5, matrix.M11, TestUtils.delta);
            Assert.AreEqual(0.600000023, matrix.M22, TestUtils.delta);
            Assert.AreEqual(-0.5, matrix.M33, TestUtils.delta);
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
            matrix.M11 = 0.11f;
            matrix.M12 = 0.12f;
            matrix.M13 = 0.13f;
            matrix.M21 = 0.21f;
            matrix.M22 = 0.22f;
            matrix.M23 = 0.23f;
            matrix.M31 = 0.31f;
            matrix.M32 = 0.32f;
            matrix.M33 = 0.33f;

            return matrix;
        }

        private Matrix4x4 GetTestMatrix4x4()
        {
            Matrix4x4 matrix = new()
            {
                M11 = 0.11f,
                M12 = 0.12f,
                M13 = 0.13f,
                M14 = 1.0f,
                M21 = 0.21f,
                M22 = 0.22f,
                M23 = 0.23f,
                M24 = 2.0f,
                M31 = 0.31f,
                M32 = 0.32f,
                M33 = 0.33f,
                M34 = 3.0f,
                M41 = 10f,
                M42 = 11f,
                M43 = 12f,
                M44 = 1f
            };

            return matrix;
        }
    }
}
