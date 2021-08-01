using CgfConverter.Structs;
using CgfConverterIntegrationTests.Extensions;
using Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Numerics;
using static Extensions.BinaryReaderExtensions;

namespace CgfConverterIntegrationTests.UnitTests
{
    [TestClass]
    public class QuaternionExtensionsTests
    {
        // Sample QTangents
        private byte[] buffer1 = new byte[] { 0x02, 0xe5, 0x92, 0xbc, 0xd7, 0x61, 0xd6, 0xd8 };   // m_ccc_heavy_armor_helmet_01.skinm 1st tangent
        private byte[] buffer2 = new byte[] { 0x5B, 0xEF, 0x12, 0xC1, 0xCA, 0x65, 0xC2, 0xD5 };  // m_ccc_heavy_armor_helmet_01.skinm 2nd tangent
        private byte[] archeAgeQTangent1 = new byte[] { 0x6A, 0x03, 0x66, 0x54, 0x92, 0xA1, 0xD5, 0xED };
        private byte[] archeAgeQTangent2 = new byte[] { 0xEA, 0x1D, 0x82, 0x31, 0xD5, 0x92, 0x8E, 0xDE };
        byte[] avengerTangent = { 0xFE, 0xBF, 0x00, 0x80, 0xFF, 0x3F, 0xB8, 0xDF };
        byte[] avengerBitangent = { 0xFE, 0xBF, 0x00, 0x80, 0xFF, 0x3F, 0xB8, 0xDF };

    [TestMethod]
        public void GetNormal_NormalizedQuaternion1()
        {
            var expected = new Vector3(1.1086464E-05f, 0.9344362f, -0.35585153f);

            using var source = new MemoryStream(buffer1);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion(InputType.Int16);
                      
            var normal = quat.GetNormal();

            Assert.AreEqual(expected, normal);
        }

        [TestMethod]
        public void GetNormal_NormalizedQuaternion2()
        {
            var expected = new Vector3(-0.11768986f, 0.8678087f, -0.4826852f);

            using var source = new MemoryStream(buffer2);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion(InputType.Int16);

            var normal = quat.GetNormal();

            Assert.AreEqual(expected, normal);
        }

        [TestMethod]
        public void GetNormal_NormalizedQuaternion3()
        {
            var expected = new Vector3(0.22654425f, 0.96535337f, -0.12885821f);

            using var source = new MemoryStream(archeAgeQTangent1);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion(InputType.Int16);

            var normal = quat.GetNormal();

            Assert.AreEqual(expected, normal);
        }

        [TestMethod]
        public void GetNormal_NormalizedQuaternion4()
        {
            var expected = new Vector3(0.60080105f, 0.5376527f, -0.59143436f);

            using var source = new MemoryStream(archeAgeQTangent2);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion(InputType.Int16);

            var normal = quat.GetNormal();

            Assert.AreEqual(expected, normal);
        }

        [TestMethod]
        public void GetNormal_NormalizedQuaternion5()
        {
            var expected = new Vector3(-0.004364252f, 1.2522434f, 0.37281585f);

            using var source = new MemoryStream(avengerTangent);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion(InputType.Int16);
            var normalizedQuat = Quaternion.Normalize(quat);  // Should be close to input

            var normal = quat.GetNormal();
            var normalizedNormal = normalizedQuat.GetNormal();

            Assert.AreEqual(expected, normal);
        }

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
