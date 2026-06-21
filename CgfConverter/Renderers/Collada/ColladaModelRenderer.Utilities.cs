using System.Numerics;
using System.Text;
using static CgfConverter.Utilities.HelperMethods;

namespace CgfConverter.Renderers.Collada;

/// <summary>
/// ColladaModelRenderer partial class - Utility methods for string formatting
/// </summary>
public partial class ColladaModelRenderer
{
    private static string CreateStringFromVector3(Vector3 vector)
    {
        StringBuilder vectorValues = new();
        vectorValues.AppendFormat("{0:F6} {1:F6} {2:F6}", vector.X, vector.Y, vector.Z);
        CleanNumbers(vectorValues);
        return vectorValues.ToString();
    }

    private static string CreateStringFromVector4(Vector4 vector)
    {
        StringBuilder vectorValues = new();
        vectorValues.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6}", vector.X, vector.Y, vector.Z, vector.W);
        CleanNumbers(vectorValues);
        return vectorValues.ToString();
    }

    private static string CreateStringFromMatrix4x4(Matrix4x4 matrix)
    {
        StringBuilder matrixValues = new();
        matrixValues.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
            matrix.M11,
            matrix.M12,
            matrix.M13,
            matrix.M14,
            matrix.M21,
            matrix.M22,
            matrix.M23,
            matrix.M24,
            matrix.M31,
            matrix.M32,
            matrix.M33,
            matrix.M34,
            matrix.M41,
            matrix.M42,
            matrix.M43,
            matrix.M44);
        CleanNumbers(matrixValues);
        return matrixValues.ToString();
    }
}
