using CgfConverter.Structs;
using CgfConverterIntegrationTests.Extensions;
using Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class QuaternionExtensionsTests
{
    [TestMethod]
    public void ArmorWarfare_Chicken_Convert4x4ToGltf()
    {
        string leftLegThighArmature = "-0.995234 0.007154 -0.097251 -0.035172 " +
                                      "0.046991 0.909053 -0.414023 0.028731 " +
                                      "0.085444 -0.416620 -0.905056 0.223571 " +
                                      "0 0 0 1";

        var stringValues = leftLegThighArmature.Split(' ');
        float[] floatValues = new float[stringValues.Length];  // create new float array

        for (int i = 0; i < stringValues.Length; i++)  // loop over string array
        {
            floatValues[i] = float.Parse(stringValues[i]);  // convert string to float and store in float array
        }

        Matrix4x4 rotationMatrix = new(
            floatValues[0], floatValues[1], floatValues[2], floatValues[3],
            floatValues[4], floatValues[5], floatValues[6], floatValues[7],
            floatValues[8], floatValues[9], floatValues[10], floatValues[11],
            floatValues[12], floatValues[13], floatValues[14], floatValues[15]);

        var expectedQuat = Quaternion.CreateFromRotationMatrix(rotationMatrix);
        var expectedTranslation = new Vector3(floatValues[3], floatValues[7], floatValues[11]);

        Assert.AreEqual(0.01387, expectedQuat.X, 0.001f);
        Assert.AreEqual(0.9758769, expectedQuat.Y, 0.001f);
        Assert.AreEqual(-0.212794, expectedQuat.Z, 0.001f);
        Assert.AreEqual(0.04680, expectedQuat.W, 0.001f);

        Assert.AreEqual(-0.035f, expectedTranslation.X, 0.001f);
        Assert.AreEqual(0.028f, expectedTranslation.Y, 0.001f);
        Assert.AreEqual(0.223f, expectedTranslation.Z, 0.001f);

        // 0.03517237, -0.013496172, 0.028730806  chicken thigh translation
        // X:-0.013871077 Y:-0.21279413 Z:0.9758769 W:-0.0468028 chicken thigh rotation

    }

    [TestMethod]
    public void Avenger_Door_Convert4x4ToGltf()
    {
        string colladaMatrix = "1 0 0 0.300001 0 0.939693 0.342020 0.512432 0 -0.342020 0.939693 -1.835138 0 0 0 1";

        var stringValues = colladaMatrix.Split(' ');
        float[] floatValues = new float[stringValues.Length];  // create new float array

        for (int i = 0; i < stringValues.Length; i++)  // loop over string array
        {
            floatValues[i] = float.Parse(stringValues[i]);  // convert string to float and store in float array
        }

        Matrix4x4 rotationMatrix = new(
            floatValues[0], floatValues[1], floatValues[2], floatValues[3],
            floatValues[4], floatValues[5], floatValues[6], floatValues[7],
            floatValues[8], floatValues[9], floatValues[10], floatValues[11],
            floatValues[12], floatValues[13], floatValues[14], floatValues[15]);

        var expectedQuat = Quaternion.CreateFromRotationMatrix(rotationMatrix);

        Assert.AreEqual(0.174f, expectedQuat.X, 0.001f);
        Assert.AreEqual(0, expectedQuat.Y, 0.001f);
        Assert.AreEqual(0, expectedQuat.Z, 0.001f);
        Assert.AreEqual(0.985f, expectedQuat.W, 0.001f);
    }

    [TestMethod]
    public void Avenger_Grip_Convert4x4_ToGltf()
    {
        string colladaMatrix = "-0.282173 -0.548292 -0.787244 -1.412314 -0.882792 0.469642 -0.010671 1.660965 0.375574 0.691962 -0.616549 0.021400 0 0 0 1";
        var stringValues = colladaMatrix.Split(' ');
        float[] floatValues = new float[stringValues.Length];  // create new float array

        for (int i = 0; i < stringValues.Length; i++)  // loop over string array
        {
            floatValues[i] = float.Parse(stringValues[i]);  // convert string to float and store in float array
        }

        Matrix4x4 rotationMatrix = new(
            floatValues[0], floatValues[1], floatValues[2], floatValues[3],
            floatValues[4], floatValues[5], floatValues[6], floatValues[7],
            floatValues[8], floatValues[9], floatValues[10], floatValues[11],
            floatValues[12], floatValues[13], floatValues[14], floatValues[15]);

        var quat = Quaternion.CreateFromRotationMatrix(rotationMatrix);

        Assert.AreEqual(-0.465f, quat.X, 0.001f);
        Assert.AreEqual(0.770f, quat.Y, 0.001f);
        Assert.AreEqual(0.221f, quat.Z, 0.001f);
        Assert.AreEqual(0.378f, quat.W, 0.001f);
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
    public void ToAxisAngle_HulaGirlUpperBody()
    {
        var q = new Quaternion(0.102484673f, -0.0468823574f, -0.00384537759f, 0.9936217f);
        var expectedAxisAngle = new Vector4(0.90883744f, -0.4157543f, -0.0341009349f, 12.949439f);

        var actualAxisAngle = q.ToAxisAngle();
        Assert.AreEqual(expectedAxisAngle.X, actualAxisAngle.X, 0.0001);
        Assert.AreEqual(expectedAxisAngle.Y, actualAxisAngle.Y, 0.0001);
        Assert.AreEqual(expectedAxisAngle.Z, actualAxisAngle.Z, 0.0001);
        Assert.AreEqual(expectedAxisAngle.W, actualAxisAngle.W, 0.0001);
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

    [TestMethod]
    public void ConvertToRotationMatrix_TestQuaternion4()
    {
        // cover_plate for Ivo behr rifle model

        var q = new Quaternion(0.0f, 0.999048f, 0.0f, -0.043619f);
        var expectedMatrix = new Matrix3x3(-0.996195f, 0, 0.087156f, 0, 1f, 0, -0.087156f, 0, -0.996195f);

        var actualMatrix = q.ConvertToRotationMatrix();
        var transposed = Matrix3x3.Transpose(actualMatrix);

        AssertExtensions.AreEqual(expectedMatrix, transposed, 0.00001);
    }

    [TestMethod]
    public void ConvertToRotationMatrix_LeftIkGripBehrRifle()
    {
        var expectedMatrix = new Matrix4x4(0.232957f, 0.956833f, 0.173787f, -0.116760f, 0.961095f, -0.253795f, 0.109013f, 0.084470f, 0.148414f, 0.141630f, -0.978731f, 0.001254f, -0, 0, 0, 1f);

        // L_IkGripTarget (bone 8) for Ivo behr rifle model v3.12
        var worldQuat = new Quaternion(0.785093f, 0.610733f, 0.102599f, 0.010386f);
        var worldTranslation = new Vector3(-0.054170f, 0.132980f, 0.012310f);

        var worldMatrix = Matrix4x4.CreateFromQuaternion(worldQuat);
        worldMatrix.M14 = worldTranslation.X;
        worldMatrix.M24 = worldTranslation.Y;
        worldMatrix.M34 = worldTranslation.Z;

        Matrix4x4.Invert(worldMatrix, out Matrix4x4 actualMatrix);

        AssertExtensions.AreEqual(expectedMatrix, actualMatrix, 0.0001);
    }
}
