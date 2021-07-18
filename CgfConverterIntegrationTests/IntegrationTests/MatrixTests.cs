using CgfConverterIntegrationTests.Extensions;
using CgfConverterTests.TestUtilities;
using Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CgfConverterIntegrationTests.UnitTests
{
    [TestClass]
    public class MatrixTests
    {
        // Joint node values in VisualScenes
        private Matrix4x4 correctBone0Matrix4x4ForVisualScene = new Matrix4x4(
            0,          1,          0,          0,
            0,          0,          -1,         0,
            -1,         0,          0,          0.023305f,
            0,          0,          0,          1);

        private Matrix4x4 correctBone1Matrix4x4ForVisualScene = new Matrix4x4(
            1,          0.000089f,  0,          0.023396f,
            -0.000089f, 1,          0.000009f,  0,
            0,          -0.000009f, 1,          0,
            0,          0,          0,          1);

        private Matrix4x4 correctBone2Matrix4x4ForVisualScene = new Matrix4x4(
           1,           0.000002f,  0,          0.026363f,
           -0.000002f,  1,          0,          0,
           0,           0,          1,          0,
           0,           0,          0,          1);

        // BPM matrix should come straight from W2B
        private Matrix4x4 correctBone0BPMMatrix = new Matrix4x4(0, 0, -1, 0.023305f, 1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 1);
        private Matrix4x4 correctBone1BPMMatrix = new Matrix4x4(-0.000089f, 0, -1, -0.000092f, 1, 0.000008f, -0.000089f, 0, 0.000008f, -1, 0, 0, 0, 0, 0, 1);
        private Matrix4x4 correctBone2BPMMatrix = new Matrix4x4(-0.000091f, 0, -1, -0.026455f, 1, 0.000008f, -0.000091f, 0, 0.000008f, -1, 0, 0, 0, 0, 0, 1);

        // For BPM
        private Matrix4x4 givenBone0W2B = new Matrix4x4(-0.000000f, -0.000000f, -1.000000f, 0.023305f, 1.000000f, -0.000000f, -0.000000f, -0.000000f, -0.000000f, -1.000000f, 0.000000f, -0.000000f, 0, 0, 0, 1);
        private Matrix4x4 givenBone1W2B = new Matrix4x4(-0.000089f, -0.000000f, -1.000000f, -0.000092f, 1.000000f, 0.000008f, -0.000089f, -0.000000f, 0.000008f, -1.000000f, 0.000000f, -0.000000f, 0, 0, 0, 1);
        private Matrix4x4 givenBone2W2B = new Matrix4x4(-0.000091f, -0.000000f, -1.000000f, -0.026455f, 1.000000f, 0.000008f, -0.000091f, -0.000000f, 0.000008f, -1.000000f, 0.000000f, -0.000000f, 0, 0, 0, 1);
        
        // For LocalTransform ((parent localrot).Transpose * localrot for rotation component, and parent.localtranslation * (localtranslation - parent.localtranslation) for translation component)
        private Matrix4x4 givenBone0B2W = new Matrix4x4(-0.000000f, 1.000000f, -0.000000f, 0.000000f, -0.000000f, -0.000000f, -1.000000f, -0.000000f, -1.000000f, -0.000000f, 0.000000f, 0.023305f, 0, 0, 0, 1);
        private Matrix4x4 givenBone1B2W = new Matrix4x4(-0.000089f, 1.000000f, 0.000008f, 0.000000f, -0.000000f, 0.000008f, -1.000000f, -0.000000f, -1.000000f, -0.000089f, 0.000000f, -0.000092f, 0, 0, 0, 1);
        private Matrix4x4 givenBone2B2W = new Matrix4x4(-0.000091f, 1.000000f, 0.000008f, -0.000002f, -0.000000f, 0.000008f, -1.000000f, -0.000000f, -1.000000f, -0.000091f, 0.000000f, -0.026455f, 0, 0, 0, 1);

        [TestMethod]
        public void BonesFromW2BWorldToBoneHasCorrectBPM()
        {
            AssertExtensions.AreEqual(correctBone0BPMMatrix, givenBone0W2B, TestUtils.delta);
            AssertExtensions.AreEqual(correctBone1BPMMatrix, givenBone1W2B, TestUtils.delta);
            AssertExtensions.AreEqual(correctBone2BPMMatrix, givenBone2W2B, TestUtils.delta);
        }

        [TestMethod]
        public void LocalTransformBone0FromB2W()
        {
            AssertExtensions.AreEqual(correctBone0Matrix4x4ForVisualScene, givenBone0B2W, TestUtils.delta);
        }

        [TestMethod]
        public void LocalTransformBone1()
        {
            var actualLocalTransform = Matrix4x4Extensions.CreateLocalTransformFromB2W(givenBone0B2W, givenBone1B2W);
            AssertExtensions.AreEqual(correctBone1Matrix4x4ForVisualScene, actualLocalTransform, 0.000005);
        }

        [TestMethod]
        public void LocalTransformBone2()
        {
            var actualLocalTransform = Matrix4x4Extensions.CreateLocalTransformFromB2W(givenBone1B2W, givenBone2B2W);
            AssertExtensions.AreEqual(correctBone2Matrix4x4ForVisualScene, actualLocalTransform, 0.000005); ;
        }

        [TestMethod]
        public void CreateMatrix4x4FromQuatAndTranslationVector()
        {
            Vector3 v = new Vector3(2.0f, 3.0f, 4.0f);
            Quaternion q = new Quaternion(0, 0, 0, 1);

            Matrix4x4 actual = Matrix4x4.CreateFromQuaternion(q);
            actual.M14 = v.X;
            actual.M24 = v.Y;
            actual.M34 = v.Z;

            Assert.AreEqual(1, actual.M11, TestUtils.delta);
            Assert.AreEqual(0, actual.M12, TestUtils.delta);
            Assert.AreEqual(2.0, actual.M14, TestUtils.delta);
            Assert.AreEqual(1, actual.M22, TestUtils.delta);
        }

        [TestMethod]
        public void MultiplyBone0W2BandBone1W2B()
        {
            var result = givenBone0W2B * givenBone1W2B;
        }

        [TestMethod]
        public void InvertBone0Matrix()
        {
            // Inverting an identiy matrix returns another identity matrix
            var bone0 = correctBone0Matrix4x4ForVisualScene;
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
            var bone0 = correctBone0Matrix4x4ForVisualScene;
            var bone1 = correctBone1Matrix4x4ForVisualScene;
            Matrix4x4 bone1BPM;
            Matrix4x4.Invert(bone1, out bone1BPM);

            // Inverted correctBone1Matrix (these values are the correct collada values
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
            Matrix4x4.Invert(correctBone0Matrix4x4ForVisualScene, out bone0W2B);

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
            var bone2 = correctBone2Matrix4x4ForVisualScene;
            Matrix4x4 actual;
            Matrix4x4.Invert(bone2, out actual);

            // { 1    0   0  0        }
            // { 0    1   0  0.033961 }
            // { 0   -0   1  0.057669 }
            // { 0    0   0  1        }
        }

    }
}
