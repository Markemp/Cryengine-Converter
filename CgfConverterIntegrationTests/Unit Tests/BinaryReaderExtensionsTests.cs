using BinaryReaderExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using static BinaryReaderExtensions.BinaryReaderExtensions;

namespace CgfConverterIntegrationTests.Unit_Tests
{
    [TestClass]
    public class BinaryReaderExtensionsTests
    {
        [TestMethod]
        public void ReadQuaternion_SingleType_0001()
        {
            var buffer = new byte[] {
            0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x80, 0x3F
            };

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion();

            Assert.AreEqual(0, quat.x);
            Assert.AreEqual(0, quat.y);
            Assert.AreEqual(0, quat.z);
            Assert.AreEqual(1, quat.w);
        }

        [TestMethod]
        public void ReadQuaternion_HalfType_0001()
        {
            var buffer = new byte[] {
            0x0, 0x0,
            0x0, 0x0,
            0x0, 0x0,
            0x80, 0x3F
            };

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion(InputType.Half);

            Assert.AreEqual(0, quat.x);
            Assert.AreEqual(0, quat.y);
            Assert.AreEqual(0, quat.z);
            Assert.AreEqual(1, quat.w);
        }

        [TestMethod]
        public void ReadQuaternion_SingleType_4Floats()
        {

            var buffer = new byte[] {
                0xA3, 0x1F, 0xD2, 0x3D,
                0x9D, 0x2B, 0x2A, 0x3C,
                0xD7, 0xFB, 0x48, 0x3F,
                0xFE, 0x58, 0x1C, 0x3F
            };

            using var source = new MemoryStream(buffer);
            using var reader = new BinaryReader(source);
            var quat = reader.ReadQuaternion(InputType.Half);

            Assert.AreEqual(0, quat.x);
            Assert.AreEqual(0, quat.y);
            Assert.AreEqual(0, quat.z);
            Assert.AreEqual(1, quat.w);
        }
    }
}
