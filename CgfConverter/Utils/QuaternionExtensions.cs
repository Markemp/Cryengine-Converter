using CgfConverter.Structs;
using System.Numerics;

namespace Extensions;

public static class QuaternionExtensions
{
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

    public static Vector3 RotateVectorByQuaternion(this Quaternion q, Vector3 v)
    {
        // https://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/transforms/index.htm
        // q*v*q^-1
        var qv = new Quaternion(v.X, v.Y, v.Z, 0);
        var qvq = q * qv * Quaternion.Conjugate(q);
        return new Vector3(qvq.X, qvq.Y, qvq.Z);
    }

    public static Vector3 FastRotateVectorByQuaternion(this Quaternion q, Vector3 v)
    {
        Vector3 result;
        float x2 = q.X * 2.0f;
        float y2 = q.Y * 2.0f;
        float z2 = q.Z * 2.0f;
        float xx2 = q.X * x2;
        float yy2 = q.Y * y2;
        float zz2 = q.Z * z2;
        float xy2 = q.X * y2;
        float xz2 = q.X * z2;
        float yz2 = q.Y * z2;
        float wx2 = q.W * x2;
        float wy2 = q.W * y2;
        float wz2 = q.W * z2;

        result.X = (1.0f - (yy2 + zz2)) * v.X + (xy2 - wz2) * v.Y + (xz2 + wy2) * v.Z;
        result.Y = (xy2 + wz2) * v.X + (1.0f - (xx2 + zz2)) * v.Y + (yz2 - wx2) * v.Z;
        result.Z = (xz2 - wy2) * v.X + (yz2 + wx2) * v.Y + (1.0f - (xx2 + yy2)) * v.Z;

        return result;
    }

    
}

