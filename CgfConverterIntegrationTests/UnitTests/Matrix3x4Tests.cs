using Microsoft.VisualStudio.TestTools.UnitTesting;
using CgfConverter.Structs;
using System.Numerics;
using CgfConverterIntegrationTests.Extensions;

namespace CgfConverterTests.UnitTests;

[TestClass]
public class Matrix3x4Tests
{
    [TestMethod]
    [TestCategory("unit")]
    public void CreateFromQuaternion_IdentityQuaternion()
    {
        Quaternion q = Quaternion.Identity;

        var expectedMatrix = Matrix3x3.Identity;

        var actualRotationMatrix = Matrix3x4.CreateFromQuaternion(q);

        Assert.AreEqual(expectedMatrix.M11, actualRotationMatrix.M11);
        Assert.AreEqual(expectedMatrix.M12, actualRotationMatrix.M12);
        Assert.AreEqual(expectedMatrix.M13, actualRotationMatrix.M13);
        Assert.AreEqual(expectedMatrix.M21, actualRotationMatrix.M21);
        Assert.AreEqual(expectedMatrix.M22, actualRotationMatrix.M22);
        Assert.AreEqual(expectedMatrix.M23, actualRotationMatrix.M23);
        Assert.AreEqual(expectedMatrix.M31, actualRotationMatrix.M31);
        Assert.AreEqual(expectedMatrix.M32, actualRotationMatrix.M32);
        Assert.AreEqual(expectedMatrix.M33, actualRotationMatrix.M33);
        Assert.AreEqual(0, actualRotationMatrix.M14);
        Assert.AreEqual(0, actualRotationMatrix.M24);
        Assert.AreEqual(0, actualRotationMatrix.M34);
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

        var actualRotationMatrix = Matrix3x4.CreateFromQuaternion(q);

        AssertExtensions.AreEqual(expectedMatrix, actualRotationMatrix.Rotation, 0.000001);
        Assert.AreEqual(0, actualRotationMatrix.M14);
        Assert.AreEqual(0, actualRotationMatrix.M24);
        Assert.AreEqual(0, actualRotationMatrix.M34);
    }

    [TestMethod]
    public void CreateFromParts_IdentityQuaternionAndVector()
    {
        Quaternion q = Quaternion.Identity;

        var expectedMatrix = Matrix3x3.Identity;
        var expectedTranslation = new Vector3(0.1f, 0.2f, 0.3f);

        var actual3x4Matrix = Matrix3x4.CreateFromParts(q, expectedTranslation);

        AssertExtensions.AreEqual(expectedMatrix, actual3x4Matrix.Rotation, 0.000001);
        Assert.AreEqual(0.1f, actual3x4Matrix.M14);
        Assert.AreEqual(0.2f, actual3x4Matrix.M24);
        Assert.AreEqual(0.3f, actual3x4Matrix.M34);
    }
}
