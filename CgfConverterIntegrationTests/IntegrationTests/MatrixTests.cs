using CgfConverterIntegrationTests.Extensions;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CgfConverterIntegrationTests.IntegrationTests
{
    [TestClass]
    public class MatrixTests
    {
        // Armature values
        private Matrix4x4 correctBone0Matrix4x4 = new Matrix4x4(
            0,          1,          0,          0,
            0,          0,          -1,         0,
            -1,         0,          0,          0.023305f,
            0,          0,          0,          1);

        private Matrix4x4 correctBone1Matrix4x4 = new Matrix4x4(
            1,          0.000089f,  0,          0.023396f,
            -0.000089f, 1,          0.000009f,  0,
            0,          -0.000009f, 1,          0,
            0,          0,          0,          1);

        private Matrix4x4 correctBone2Matrix4x4 = new Matrix4x4(
           1,           0.000002f,  0,          0.026363f,
           -0.000002f,  1,          0,          0,
           0,           0,          1,          0,
           0,           0,          0,          1);

        private Matrix4x4 givenBone0W2B = new Matrix4x4(-0.000000f, -0.000000f, -1.000000f, 0.023305f, 1.000000f, -0.000000f, -0.000000f, -0.000000f, -0.000000f, -1.000000f, 0.000000f, -0.000000f, 0, 0, 0, 1);
        private Matrix4x4 givenBone0B2W = new Matrix4x4(-0.000000f, 1.000000f, -0.000000f, 0.000000f, -0.000000f, -0.000000f, -1.000000f, -0.000000f, -1.000000f, -0.000000f, 0.000000f, 0.023305f, 0, 0, 0, 1);
        private Matrix4x4 givenBone1W2B = new Matrix4x4(-0.000089f, -0.000000f, -1.000000f, -0.000092f, 1.000000f, 0.000008f, -0.000089f, -0.000000f, 0.000008f, -1.000000f, 0.000000f, -0.000000f, 0, 0, 0, 1);
        private Matrix4x4 givenBone1B2W = new Matrix4x4(-0.000089f, 1.000000f, 0.000008f, 0.000000f, -0.000000f, 0.000008f, -1.000000f, -0.000000f, -1.000000f, -0.000089f, 0.000000f, -0.000092f, 0, 0, 0, 1);
        private Matrix4x4 givenBone2W2B = new Matrix4x4(-0.000091f, -0.000000f, -1.000000f, -0.026455f, 1.000000f, 0.000008f, -0.000091f, -0.000000f, 0.000008f, -1.000000f, 0.000000f, -0.000000f, 0, 0, 0, 1);
        private Matrix4x4 givenBone2B2W = new Matrix4x4(-0.000091f, 1.000000f, 0.000008f, -0.000002f, -0.000000f, 0.000008f, -1.000000f, -0.000000f, -1.000000f, -0.000091f, 0.000000f, -0.026455f, 0, 0, 0, 1);

        [TestMethod]
        public void Bone0FromW2B()
        {
            Matrix4x4 w2bInverse;
            Matrix4x4.Invert(givenBone0W2B, out w2bInverse);

            AssertExtensions.AreEqual(correctBone0Matrix4x4, w2bInverse, TestUtils.delta);
        }

        [TestMethod]
        public void Bone1FromW2B()
        {
            Matrix4x4 w2bInverse;
            Matrix4x4.Invert(givenBone1W2B, out w2bInverse);
            Matrix4x4 b2wInverse;
            Matrix4x4.Invert(givenBone1B2W, out b2wInverse);

            AssertExtensions.AreEqual(correctBone1Matrix4x4, b2wInverse * correctBone1Matrix4x4, TestUtils.delta);
            //AssertExtensions.AreEqual(correctBone1Matrix4x4, w2bInverse * correctBone1Matrix4x4, TestUtils.delta);
            //AssertExtensions.AreEqual(correctBone1Matrix4x4, correctBone1Matrix4x4 * b2wInverse, TestUtils.delta);
            //AssertExtensions.AreEqual(correctBone1Matrix4x4, correctBone1Matrix4x4 * w2bInverse, TestUtils.delta);
            //AssertExtensions.AreEqual(correctBone1Matrix4x4, b2wInverse, TestUtils.delta);
            //AssertExtensions.AreEqual(correctBone1Matrix4x4, w2bInverse, TestUtils.delta);

        }


        [TestMethod]
        public void InvertBone0Matrix()
        {
            // Inverting an identiy matrix returns another identity matrix
            var bone0 = correctBone0Matrix4x4;
            Matrix4x4 bone0BPM;
            Matrix4x4.Invert(bone0, out bone0BPM);

            // { 0    0     -1       0.023305}   // Bone 0 World2Bone
            // { 1    0      0      -0       }
            // { 0   -1      0       0       }
            // { 0    0     -0       1       }
            // Matches W2B in source code

            Assert.AreEqual(0, bone0BPM.M11);
            Assert.AreEqual(-1, bone0BPM.M13);
            Assert.AreEqual(1, bone0BPM.M21);
            Assert.AreEqual(-1, bone0BPM.M32);

            Matrix4x4 actual2;
            Matrix4x4.Invert(bone0BPM, out actual2);
            Assert.AreEqual(bone0, actual2);
        }

        [TestMethod]
        public void InvertBone1Matrix()
        {
            // Inverting a matrix twice returns original matrix.
            var bone0 = correctBone0Matrix4x4;
            var bone1 = correctBone1Matrix4x4;
            Matrix4x4 bone1BPM;
            Matrix4x4.Invert(bone1, out bone1BPM);

            // {1               -0.000089       0               -0.023396   }
            // {0.000089        1               -0.000009       -0.0000021  }
            // {0               0.0000009       1               0           }
            // {-0              0               0               1           }

            Matrix4x4 actual2;
            Matrix4x4.Invert(bone1BPM, out actual2);
            AssertExtensions.AreEqual(bone1, actual2, TestUtils.delta);

            // Expected W2B for bone1
            // [-0.000089, -0,        -1,        -0.000092]
            // [1,          0.000008, -0.000089, -0]
            // [0.000008,  -1,         0,        -0]]

            // Multiply parent bone by inverse of Bone1 W2B
            Matrix4x4 bone1W2B = new Matrix4x4(
                -0.000089f,     -0,           -1,          -0.000092f, 
                1,              0.000008f,    -0.000089f,  -0, 
                0.000008f,      -1f,           0,          -0, 
                0,               0,            0,           1         );

            Matrix4x4 bone0W2B;
            Matrix4x4.Invert(correctBone0Matrix4x4, out bone0W2B);

            var result1 = bone0 * bone1BPM;
            // result
            // {0.000089        1               -0.000009      -0.0000021}
            // {0              -0.000009       -1               0}
            // {-1              0.000089       0               0.046701}
            // {0               0               0               1}

        }

        [TestMethod]
        public void InvertBone2Matrix()
        {
            var bone2 = correctBone2Matrix4x4;
            Matrix4x4 actual;
            Matrix4x4.Invert(bone2, out actual);

            // { 1    0   0  0        }
            // { 0    1   0  0.033961 }
            // { 0   -0   1  0.057669 }
            // { 0    0   0  1        }
        }

    }
}
