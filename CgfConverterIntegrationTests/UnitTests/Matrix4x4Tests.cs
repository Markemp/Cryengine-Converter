using CgfConverter.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;
using Extensions;
using CgfConverterTests.TestUtilities;
using System.IO;
using CgfConverterIntegrationTests.Extensions;

namespace CgfConverterTests.UnitTests;

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

    private const float delta = 0.000001f;

    [TestMethod]
    [TestCategory("unit")]
    public void SetRotationAndTranslationFromQuaternion()
    {
        var quat = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        var translation = new Vector3(2.0f, 3.0f, 4.0f);

        var matrix = Matrix4x4.CreateFromQuaternion(quat);
        matrix.Translation = translation;

        Assert.AreEqual(1.0, matrix.M11, delta);
        Assert.AreEqual(0.0, matrix.M21, delta);
        Assert.AreEqual(2.0, matrix.M41, delta);
        Assert.AreEqual(3.0, matrix.M42, delta);
        Assert.AreEqual(4.0, matrix.M43, delta);
    }

    [TestMethod]
    public void GetTransformFromParts_TwoParameters()
    {
        Vector3 vector3 = GetTestVector3();
        Matrix3x3 rotation = GetTestMatrix33();

        Matrix4x4 matrix = Matrix4x4Extensions.CreateTransformFromParts(vector3, rotation);
        Assert.AreEqual(0.11f, matrix.M11);
        Vector3 scale = matrix.GetScale();
    }

    [TestMethod]
    public void Matrix4x4_CreateScale_ProperElements()
    {
        var matrix = Matrix4x4.CreateScale(GetTestVector3()); // (0.5, 0.6, -0.5)

        Assert.AreEqual(0.5, matrix.M11, TestUtils.delta);
        Assert.AreEqual(0.600000023, matrix.M22, TestUtils.delta);
        Assert.AreEqual(-0.5, matrix.M33, TestUtils.delta);
    }

    [TestMethod]
    public void Matrix4x4_Transpose()
    {
        var actual = GetTestMatrix4x4();
        var expected = Matrix4x4.Transpose(actual);

        Assert.AreEqual(expected.M11, actual.M11, TestUtils.delta);
        Assert.AreEqual(expected.M21, actual.M12, TestUtils.delta);
        Assert.AreEqual(expected.M31, actual.M13, TestUtils.delta);
        Assert.AreEqual(expected.M41, actual.M14, TestUtils.delta);
        Assert.AreEqual(expected.M12, actual.M21, TestUtils.delta);
        Assert.AreEqual(expected.M22, actual.M22, TestUtils.delta);
        Assert.AreEqual(expected.M32, actual.M23, TestUtils.delta);
    }

    [TestMethod]
    public void ConvertMatrix3x4_To_Matrix4x4()
    {
        var buffer = TestUtils.GetBone1WorldToBoneBytes();

        using var source = new MemoryStream(buffer);
        using var reader = new BinaryReader(source);
        var m34 = reader.ReadMatrix3x4();
        var m = m34.ConvertToTransformMatrix();

        Assert.AreEqual(0, m.M11, delta);
        Assert.AreEqual(0, m.M12, delta);
        Assert.AreEqual(-1, m.M13, delta);
        Assert.AreEqual(0.0233046, m.M14, delta);
        Assert.AreEqual(0.9999999, m.M21, delta);
        Assert.AreEqual(-1.629207e-07, m.M22, delta);
        Assert.AreEqual(-3.264332e-22, m.M23, delta);
        Assert.AreEqual(-1.659635e-16, m.M24, delta);
        Assert.AreEqual(-1.629207e-07, m.M31, delta);
        Assert.AreEqual(-0.9999999, m.M32, delta);
        Assert.AreEqual(7.549789e-08, m.M33, delta);
        Assert.AreEqual(-2.778125e-09, m.M34, delta);
        Assert.AreEqual(0, m.M41, delta);
        Assert.AreEqual(0, m.M42, delta);
        Assert.AreEqual(0, m.M43, delta);
        Assert.AreEqual(1, m.M44, delta);
    }

    [TestMethod]
    public void CompareBPM_Bone1()
    {
        var expectedBPM = TestUtils.GetExpectedBone1BPM();

        var buffer = TestUtils.GetBone1WorldToBoneBytes();

        using var source = new MemoryStream(buffer);
        using var reader = new BinaryReader(source);
        Matrix4x4 actualBPM;
        Matrix4x4.Invert(reader.ReadMatrix3x4().ConvertToTransformMatrix(), out actualBPM);

        AssertExtensions.AreEqual(expectedBPM, actualBPM, TestUtils.delta);
    }

    private Vector3 GetTestVector3()
    {
        Vector3 vector = new Vector3();
        vector.X = 0.5f;
        vector.Y = 0.6f;
        vector.Z = -0.5f;

        return vector;
    }

    private Matrix3x3 GetTestMatrix33()
    {
        Matrix3x3 matrix = new Matrix3x3();
        matrix.M11 = 0.11f;
        matrix.M12 = 0.12f;
        matrix.M13 = 0.13f;
        matrix.M21 = 0.21f;
        matrix.M22 = 0.22f;
        matrix.M23 = 0.23f;
        matrix.M31 = 0.31f;
        matrix.M32 = 0.32f;
        matrix.M33 = 0.33f;

        return matrix;
    }

    private Matrix4x4 GetTestMatrix4x4()
    {
        Matrix4x4 matrix = new()
        {
            M11 = 0.11f,
            M12 = 0.12f,
            M13 = 0.13f,
            M14 = 1.0f,
            M21 = 0.21f,
            M22 = 0.22f,
            M23 = 0.23f,
            M24 = 2.0f,
            M31 = 0.31f,
            M32 = 0.32f,
            M33 = 0.33f,
            M34 = 3.0f,
            M41 = 10f,
            M42 = 11f,
            M43 = 12f,
            M44 = 1f
        };

        return matrix;
    }

    private Matrix4x4 GetTestMatrix4x4WithTranslation()
    {
        Matrix4x4 matrix = new()
        {
            M11 = -1.000000f,
            M12 = 0.000001f,
            M13 = 0.000009f,
            M14 = 0.000000f,
            M21 = -0.000005f,
            M22 = -0.866025f,
            M23 = -0.500000f,
            M24 = 0.000000f,
            M31 = 0.000008f,
            M32 = -0.500000f,
            M33 = 0.866025f,
            M34 = 0.000000f,
            M41 = 183.048630f / 100,
            M42 = -244.434143f / 100,
            M43 = -154.250488f / 100,
            M44 = 0.000000f
        };

        return matrix;
    }
}
