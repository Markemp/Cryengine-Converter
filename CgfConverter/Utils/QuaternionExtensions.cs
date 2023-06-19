﻿using System;
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

    public static List<float> ToGltfList(this Quaternion q, bool isGltfCoordinateSystem = false)
    {
        //if (isGltfCoordinateSystem)
        //    return new List<float>() { q.X, q.Z, -q.Y, q.W };
        //else
            return new List<float>() { q.X, q.Y, q.Z, q.W };
    }
}
