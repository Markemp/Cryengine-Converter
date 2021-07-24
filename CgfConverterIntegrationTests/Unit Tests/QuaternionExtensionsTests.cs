using CgfConverter.Structs;
using CgfConverterIntegrationTests.Extensions;
using Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CgfConverterIntegrationTests.UnitTests
{
    [TestClass]
    public class QuaternionExtensionsTests
    {
        [TestMethod]
        public void ConvertToRotationMatrix_Identity()
        {
            var q = Quaternion.Identity;
            var expectedMatrix = Matrix3x3.Identity;
            var actualMatrix = q.ConvertToRotationMatrix();

            AssertExtensions.AreEqual(expectedMatrix, actualMatrix, 0.000005);
        }

        [TestMethod]
        public void ConvertToRotationMatrix_TestQuaternion1()
        {
            // [-0.181986, 0.000000, -0.000000, 0.983301] to
            //[  1.0000000,  0.0000000,  0.0000000;
            //   0.0000000,  0.9337622,  0.3578941;
            //   0.0000000, -0.3578941,  0.9337622 ]

            var q = new Quaternion(-0.181986f, 0.000000f, -0.000000f, 0.983301f);
            var expectedMatrix = new Matrix3x3(1.0000000f, 0.0000000f, 0.0000000f, 0.0000000f, 0.9337622f, 0.3578941f, 0.0000000f, -0.3578941f, 0.9337622f);

            var actualMatrix = q.ConvertToRotationMatrix();

            AssertExtensions.AreEqual(expectedMatrix, actualMatrix, 0.000001);
        }

        [TestMethod]
        public void ConvertToRotationMatrix_TestQuaternion2()
        {
            // [0.000000, -0.470351, 0.882480, 0.000000] to 
            // [ -1.0000000,  0.0000000,  0.0000000;
            //    0.0000000, -0.5575403, -0.8301499;
            //    0.0000000, -0.8301499,  0.5575403 ]

            var q = new Quaternion(0.000000f, -0.470351f, 0.882480f, 0.000000f);
            var expectedMatrix = new Matrix3x3(-1, 0, 0, 0, -0.5575403f, -0.8301499f, 0, -0.8301499f, 0.5575403f);

            var actualMatrix = q.ConvertToRotationMatrix();

            AssertExtensions.AreEqual(expectedMatrix, actualMatrix, 0.000001);
        }

        [TestMethod]
        public void ConvertToRotationMatrix_TestQuaternion3()
        {
            // [0.258819, -0.000000, 0.000000, 0.965926] to
            // [  1.0000000,  0.0000000,  0.0000000;
            //    0.0000000,  0.8660255, -0.4999999;
            //    0.0000000,  0.4999999,  0.8660255 ]

            var q = new Quaternion(0.258819f, -0.000000f, 0.000000f, 0.965926f);
            var expectedMatrix = new Matrix3x3(1, 0, 0, 0, 0.8660255f, -0.4999999f, 0, 0.4999999f, 0.8660255f);

            var actualMatrix = q.ConvertToRotationMatrix();

            AssertExtensions.AreEqual(expectedMatrix, actualMatrix, 0.000001);
        }
    }
}
