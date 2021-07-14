using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

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
    }
}
