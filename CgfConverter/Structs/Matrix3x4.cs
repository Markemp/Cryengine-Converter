using System;
using System.Numerics;

namespace CgfConverter.Structs
{
    /// <summary>
    /// WorldToBone and BoneToWorld objects in Cryengine files.  Inspiration/code based from
    /// https://referencesource.microsoft.com/#System.Numerics/System/Numerics/Matrix4x4.cs,48ce53b7e55d0436
    /// </summary>
    public struct Matrix3x4 : IEquatable<Matrix3x4>
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;

        public Vector3 Translation { 
            get { return new Vector3(M14, M24, M34); }
            set 
            {
                M14 = value.X;
                M24 = value.Y;
                M34 = value.Z;
            }
        }

        public Matrix3x3 Rotation
        {
            get {
                return new Matrix3x3(M11, M12, M13, M21, M22, M23, M31, M32, M33);
            }
        }

        /// <summary>
        /// Creates a rotation matrix from the given Quaternion rotation value.
        /// </summary>
        /// <param name="quaternion">The source Quaternion.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix3x4 CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix3x4 result;

            float xx = quaternion.X * quaternion.X;
            float yy = quaternion.Y * quaternion.Y;
            float zz = quaternion.Z * quaternion.Z;

            float xy = quaternion.X * quaternion.Y;
            float wz = quaternion.Z * quaternion.W;
            float xz = quaternion.Z * quaternion.X;
            float wy = quaternion.Y * quaternion.W;
            float yz = quaternion.Y * quaternion.Z;
            float wx = quaternion.X * quaternion.W;

            result.M11 = 1.0f - 2.0f * (yy + zz);
            result.M12 = 2.0f * (xy + wz);
            result.M13 = 2.0f * (xz - wy);
            result.M14 = 0.0f;
            result.M21 = 2.0f * (xy - wz);
            result.M22 = 1.0f - 2.0f * (zz + xx);
            result.M23 = 2.0f * (yz + wx);
            result.M24 = 0.0f;
            result.M31 = 2.0f * (xz + wy);
            result.M32 = 2.0f * (yz - wx);
            result.M33 = 1.0f - 2.0f * (yy + xx);
            result.M34 = 0.0f;

            return result;
        }

        public Matrix4x4 ConvertToTransformMatrix()
        {
            var m = new Matrix4x4
            {
                M11 = M11,
                M12 = M12,
                M13 = M13,
                M14 = M14,
                M21 = M21,
                M22 = M22,
                M23 = M23,
                M24 = M24,
                M31 = M31,
                M32 = M32,
                M33 = M33,
                M34 = M34,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };
            return m;
        }

        /// <summary>
        /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
        /// </summary>
        /// <param name="other">The matrix to compare this instance to.</param>
        /// <returns>True if the matrices are equal; False otherwise.</returns>
        public bool Equals(Matrix3x4 other)
        {
            return (M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && // Check diagonal element first for early out.
                    M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
                    M21 == other.M21 && M23 == other.M23 && M24 == other.M24 &&
                    M31 == other.M31 && M32 == other.M32 && M34 == other.M34);
        }
    }
}
