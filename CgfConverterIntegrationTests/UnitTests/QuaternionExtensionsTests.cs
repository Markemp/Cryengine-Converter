using CgfConverter.Structs;
using CgfConverterIntegrationTests.Extensions;
using Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace CgfConverterTests.UnitTests;

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
