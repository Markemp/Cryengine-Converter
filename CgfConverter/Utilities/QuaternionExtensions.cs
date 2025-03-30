using CgfConverter.Structs;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Extensions;

public static class QuaternionExtensions
{
    public static float GetComponent(this Quaternion q, int index) => index switch
    {
        0 => q.X,
        1 => q.Y,
        2 => q.Z,
        3 => q.W,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    public static Vector3 DropW(this Quaternion q) => new(q.X, q.Y, q.Z);

    public static Matrix3x3 ConvertToRotationMatrix(this Quaternion q)
    {
        // https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
        var rotationalMatrix = new Matrix3x3();
        float sqw = q.W * q.W;
        float sqx = q.X * q.X;
        float sqy = q.Y * q.Y;
        float sqz = q.Z * q.Z;

        // invs (inverse square length) is only required if quaternion is not already normalised
        float invs = 1 / (sqx + sqy + sqz + sqw);
        rotationalMatrix.M11 = (sqx - sqy - sqz + sqw) * invs; // since sqw + sqx + sqy + sqz =1/invs*invs
        rotationalMatrix.M22 = (-sqx + sqy - sqz + sqw) * invs;
        rotationalMatrix.M33 = (-sqx - sqy + sqz + sqw) * invs;

        float tmp1 = q.X * q.Y;
        float tmp2 = q.Z * q.W;
        rotationalMatrix.M21 = 2.0f * (tmp1 + tmp2) * invs;
        rotationalMatrix.M12 = 2.0f * (tmp1 - tmp2) * invs;

        tmp1 = q.X * q.Z;
        tmp2 = q.Y * q.W;
        rotationalMatrix.M31 = 2.0f * (tmp1 - tmp2) * invs;
        rotationalMatrix.M13 = 2.0f * (tmp1 + tmp2) * invs;
        tmp1 = q.Y * q.Z;
        tmp2 = q.X * q.W;
        rotationalMatrix.M32 = 2.0f * (tmp1 + tmp2) * invs;
        rotationalMatrix.M23 = 2.0f * (tmp1 - tmp2) * invs;

        return rotationalMatrix;
    }

    public static Vector4 ToAxisAngle(this Quaternion quaternion)
    {
        // Handle the case where the quaternion is the identity quaternion.
        // This condition checks if the quaternion is close enough to the identity quaternion.
        if (MathF.Abs(quaternion.W) >= 1.0f - float.Epsilon)
            return new Vector4(1, 0, 0, 0); // Arbitrary axis with 0 angle.

        // TODO:  Figure out why this is inverted.
        //Vector3 axis = new(-quaternion.X, -quaternion.Y, -quaternion.Z);
        Vector3 axis = new(quaternion.X, quaternion.Y, quaternion.Z);
        float scale = axis.Length();

        // Avoid division by zero when normalizing the axis.
        if (scale < float.Epsilon)
            scale = 1;

        axis /= scale;
        float angle = 2.0f * MathF.Acos(quaternion.W);

        angle *= (180.0f / MathF.PI);  // Convert the angle to degrees.

        return new Vector4(axis, angle);
    }

    /// <summary>
    /// Gets the normal vector from a quaternion stored as a qtangent.
    /// Based on CryEngine's qtangent normal calculation.
    /// </summary>
    /// <param name="q">The quaternion stored as a qtangent</param>
    /// <returns>The normal vector</returns>
    public static Vector3 GetNormalFromQTangent(this Quaternion q)
    {
        // Calculate the rotated Z axis (0,0,1) using the quaternion
        // This is equivalent to GetColumn2() in CryEngine
        float x = 2.0f * (q.X * q.Z + q.Y * q.W);
        float y = 2.0f * (q.Y * q.Z - q.X * q.W);
        float z = 2.0f * (q.Z * q.Z + q.W * q.W) - 1.0f;

        Vector3 normal = new (x, y, z);

        // Normalize the vector
        normal = Vector3.Normalize(normal);

        // Apply w-sign handling as per CryEngine's GetN()
        if (q.W < 0.0f)
            normal = -normal;

        return normal;
    }

    public static Vector3 GetNormalFromQTangentExact(this Quaternion q)
    {
        // Direct port of CryEngine's GetColumn2/GetN with no normalization
        float x = 2.0f * (q.X * q.Z + q.Y * q.W);
        float y = 2.0f * (q.Y * q.Z - q.X * q.W);
        float z = 2.0f * (q.Z * q.Z + q.W * q.W) - 1.0f;

        Vector3 normal = new(x, y, z);

        if (q.W < 0.0f)
            normal = -normal;

        return normal;
    }

    public static Vector3 GetNormalFromQTangentPreNormalized(this Quaternion q)
    {
        // Normalize quaternion first (in case input isn't normalized)
        float length = (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
        Quaternion normalized = new(q.X / length, q.Y / length, q.Z / length, q.W / length);

        // Then apply the same formula
        float x = 2.0f * (normalized.X * normalized.Z + normalized.Y * normalized.W);
        float y = 2.0f * (normalized.Y * normalized.Z - normalized.X * normalized.W);
        float z = 2.0f * (normalized.Z * normalized.Z + normalized.W * normalized.W) - 1.0f;

        Vector3 normal = new(x, y, z);

        if (normalized.W < 0.0f)
            normal = -normal;

        return normal;
    }

    public static List<float> ToGltfList(this Quaternion q) => [q.X, q.Y, q.Z, q.W];
}
