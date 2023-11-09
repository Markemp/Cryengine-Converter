using System;
using CgfConverter.Structs;
using System.Numerics;
using System.Collections.Generic;

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
        //Vector3 axis = Vector3.Normalize(new Vector3(quaternion.X, quaternion.Y, quaternion.Z));
        //float angle = 2.0f * MathF.Acos(quaternion.W);
        //return new Vector4(axis, angle);
        // Handle the case where the quaternion is the identity quaternion.
        // This condition checks if the quaternion is close enough to the identity quaternion.
        if (MathF.Abs(quaternion.W) >= 1.0f - float.Epsilon)
            return new Vector4(1, 0, 0, 0); // Arbitrary axis with 0 angle.

        Vector3 axis = new(-quaternion.X, -quaternion.Y, -quaternion.Z);
        float scale = axis.Length();

        // Avoid division by zero when normalizing the axis.
        if (scale < float.Epsilon)
            scale = 1;

        axis /= scale;
        float angle = 2.0f * MathF.Acos(quaternion.W);

        // Convert the angle to degrees.
        angle *= (180.0f / MathF.PI);

        return new Vector4(axis, angle);
    }

    public static Vector4 ToAxisAngle2(this Quaternion quaternion)
    {
        if (quaternion.W > 1)
            quaternion = Quaternion.Normalize(quaternion);

        float angle = 2.0f * MathF.Acos(quaternion.W) * (180.0f / MathF.PI);
        float s = MathF.Sqrt(1 - quaternion.W * quaternion.W);
        if (s < 0.001f)
            return new Vector4(-quaternion.X, -quaternion.Y, -quaternion.Z, angle); // angle is 0 and axis is arbitrary
        else
            return new Vector4(-quaternion.X / s, -quaternion.Y / s, -quaternion.Z / s, angle);
    }

    public static List<float> ToGltfList(this Quaternion q) => new List<float>() { q.X, q.Y, q.Z, q.W };
}
