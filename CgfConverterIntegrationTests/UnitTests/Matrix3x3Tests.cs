using CgfConverter.Structs;
using CgfConverterIntegrationTests.Extensions;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CgfConverterTests.UnitTests
{
    [TestClass]
    [TestCategory("unit")]
    public class Matrix3x3Tests
    {
        [TestMethod]
        public void CreateFromQuaternion_IdentityQuaternion()
        {
            Quaternion q = Quaternion.Identity;

            var expectedMatrix = Matrix3x3.Identity;

            var actualRotationMatrix = Matrix3x3.CreateFromQuaternion(q);

            Assert.AreEqual(expectedMatrix.M11, actualRotationMatrix.M11);
            Assert.AreEqual(expectedMatrix.M12, actualRotationMatrix.M12);
            Assert.AreEqual(expectedMatrix.M13, actualRotationMatrix.M13);
            Assert.AreEqual(expectedMatrix.M21, actualRotationMatrix.M21);
            Assert.AreEqual(expectedMatrix.M22, actualRotationMatrix.M22);
            Assert.AreEqual(expectedMatrix.M23, actualRotationMatrix.M23);
            Assert.AreEqual(expectedMatrix.M31, actualRotationMatrix.M31);
            Assert.AreEqual(expectedMatrix.M32, actualRotationMatrix.M32);
            Assert.AreEqual(expectedMatrix.M33, actualRotationMatrix.M33);
        }

        [TestMethod]
        public void CreateFromQuaternion_TestQuaternion1()
        {
            // [-0.181986, 0.000000, -0.000000, 0.983301] to
            //[  1.0000000,  0.0000000,  0.0000000;
            //   0.0000000,  0.9337622,  0.3578941;
            //   0.0000000, -0.3578941,  0.9337622 ]

            var q = new Quaternion(-0.181986f, 0.000000f, -0.000000f, 0.983301f);
            var expectedMatrix = new Matrix3x3(1.0000000f, 0.0000000f, 0.0000000f, 0.0000000f, 0.9337622f, 0.3578941f, 0.0000000f, -0.3578941f, 0.9337622f);

            var actualRotationMatrix = Matrix3x3.CreateFromQuaternion(q);

            AssertExtensions.AreEqual(expectedMatrix, actualRotationMatrix, TestUtils.delta);
        }

        [TestMethod]
        public void CreateFromQuaternion_TestQuaternion2()
        {
            // [0.000000, -0.470351, 0.882480, 0.000000] to
            // [ -1.0000000,  0.0000000,  0.0000000;
            //    0.0000000, -0.5575403, -0.8301499;
            //    0.0000000, -0.8301499,  0.5575403 ]
            var q = new Quaternion(0.000000f, -0.470351f, 0.882480f, 0.000000f);
            var expectedMatrix = new Matrix3x3(-1, 0, 0, 0, -0.5575403f, -0.8301499f, 0, -0.8301499f, 0.5575403f);

            var actualRotationMatrix = Matrix3x3.CreateFromQuaternion(q);

            AssertExtensions.AreEqual(expectedMatrix, actualRotationMatrix, TestUtils.delta);
        }
    }
}
