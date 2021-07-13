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

        [TestMethod]
        public void SetRotationFromQuaternion()
        {
            var quat = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
            var translation = new Vector3(2.0f, 3.0f, 4.0f);

            var matrix = Matrix4x4.CreateFromQuaternion(quat);
            matrix.Translation = translation;
            
        }
    }
}
