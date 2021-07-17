using System;
using System.Numerics;

namespace CgfConverter.Structs
{
    /// <summary>
    /// 3x3 Rotation matrices in Cryengine files.  Inspiration/code based from
    /// https://referencesource.microsoft.com/#System.Numerics/System/Numerics/Matrix4x4.cs,48ce53b7e55d0436
    /// </summary>
    public struct Matrix3x3 : IEquatable<Matrix3x3>
    {
        public float M11;
        public float M12;
        public float M13;
        public float M21;
        public float M22;
        public float M23;
        public float M31;
        public float M32;
        public float M33;

        public Matrix3x3(float m11, float m12, float m13,
                         float m21, float m22, float m23,
                         float m31, float m32, float m33)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;

            M21 = m21;
            M22 = m22;
            M23 = m23;

            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        private static readonly Matrix3x3 _identity = new Matrix3x3
        (
            1f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f
        );

        public static Matrix3x3 Identity
        {
            get { return _identity; }
        }

        /// <summary>
        /// Transposes the rows and columns of a matrix.
        /// </summary>
        /// <param name="matrix">The source matrix.</param>
        /// <returns>The transposed matrix.</returns>
        public static Matrix3x3 Transpose(Matrix3x3 matrix)
        {
            Matrix3x3 result;

            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;

            return result;
        }

        public double GetDeterminant()
        {
            return (M11 * M22 * M33
                  + M12 * M23 * M31
                  + M13 * M21 * M32
                  - M31 * M22 * M13
                  - M21 * M12 * M33
                  - M11 * M32 * M23);
        }

        public Matrix3x3 GetTranspose()    // returns a copy of the matrix33
        {
            Matrix3x3 mat = new Matrix3x3
            {
                M11 = M11,
                M12 = M21,
                M13 = M31,
                M21 = M12,
                M22 = M22,
                M23 = M32,
                M31 = M13,
                M32 = M23,
                M33 = M33
            };
            return mat;
        }

        public Matrix3x3 Mult(Matrix3x3 mat)
        {
            Matrix3x3 mat2 = new Matrix3x3
            {
                M11 = (M11 * mat.M11) + (M12 * mat.M21) + (M13 * mat.M31),
                M12 = (M11 * mat.M12) + (M12 * mat.M22) + (M13 * mat.M32),
                M13 = (M11 * mat.M13) + (M12 * mat.M23) + (M13 * mat.M33),
                M21 = (M21 * mat.M11) + (M22 * mat.M21) + (M23 * mat.M31),
                M22 = (M21 * mat.M12) + (M22 * mat.M22) + (M23 * mat.M32),
                M23 = (M21 * mat.M13) + (M22 * mat.M23) + (M23 * mat.M33),
                M31 = (M31 * mat.M11) + (M32 * mat.M21) + (M33 * mat.M31),
                M32 = (M31 * mat.M12) + (M32 * mat.M22) + (M33 * mat.M32),
                M33 = (M31 * mat.M13) + (M32 * mat.M23) + (M33 * mat.M33)
            };
            return mat2;
        }

        internal Matrix3x3 ConjugateTransposeThisAndMultiply(Matrix3x3 matrix3x3)
        {
            throw new NotImplementedException();
        }

        public static Matrix3x3 operator *(Matrix3x3 lhs, Matrix3x3 rhs)
        {
            return lhs.Mult(rhs);
        }

        public Vector3 Mult3x1(Vector3 vector)
        {
            // Multiply the 3x3 matrix by a Vector 3 to get the rotation
            Vector3 result = new Vector3
            {
                X = ((vector.X * M11) + (vector.Y * M21) + (vector.Z * M31)),
                Y = ((vector.X * M12) + (vector.Y * M22) + (vector.Z * M32)),
                Z = ((vector.X * M13) + (vector.Y * M23) + (vector.Z * M33))
            };
            return result;
        }

        public static Vector3 operator *(Matrix3x3 rhs, Vector3 lhs)
        {
            return rhs.Mult3x1(lhs);
        }

        public bool IsScaleRotation() // Returns true if the matrix decomposes nicely into scale * rotation\
        {
            Matrix3x3 self_transpose, mat = new Matrix3x3();
            self_transpose = GetTranspose();
            mat = Mult(self_transpose);
            if (Math.Abs(mat.M12) + Math.Abs(mat.M13)
                + Math.Abs(mat.M21) + Math.Abs(mat.M23)
                + Math.Abs(mat.M31) + Math.Abs(mat.M32) > 0.01)
            {
                Utils.Log(LogLevelEnum.Debug, " is a Scale_Rot matrix");
                return false;
            }
            Utils.Log(LogLevelEnum.Debug, " is not a Scale_Rot matrix");
            return true;
        }

        public Vector3 GetScale()
        {
            // Get the scale, assuming is_scale_rotation is true
            Matrix3x3 mat = Mult(GetTranspose());
            Vector3 scale = new Vector3
            {
                X = (float)Math.Pow(mat.M11, 0.5),
                Y = (float)Math.Pow(mat.M22, 0.5),
                Z = (float)Math.Pow(mat.M33, 0.5)
            };
            if (GetDeterminant() < 0)
            {
                scale.X = 0 - scale.X;
                scale.Y = 0 - scale.Y;
                scale.Z = 0 - scale.Z;
                return scale;
            }
            else
            {
                return scale;
            }
        }

        public Vector3 GetScaleRotation()   // Gets the scale.  this should also return the rotation matrix, but..eh...
        {
            Vector3 scale = GetScale();
            return scale;
        }

        public bool IsRotation()
        {
            // NOTE: 0.01 instead of CgfFormat.EPSILON to work around bad files
            if (!IsScaleRotation()) { return false; }
            Vector3 scale = GetScale();
            if (Math.Abs(scale.X - 1.0) > 0.01 || Math.Abs(scale.Y - 1.0) > 0.01 || Math.Abs(scale.Z - 1.0) > 0.1)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return $"[[{M11:F4}, {M12:F4}, {M13:F4}], [{M21:F4}, {M22:F4}, {M23:F4}], [{M31:F4}, {M32:F4}, {M33:F4}]]";
        }

        /// <summary>
        /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
        /// </summary>
        /// <param name="other">The matrix to compare this instance to.</param>
        /// <returns>True if the matrices are equal; False otherwise.</returns>
        public bool Equals(Matrix3x3 other)
        {
            return (M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && // Check diagonal element first for early out.
                    M12 == other.M12 && M13 == other.M13 &&
                    M21 == other.M21 && M23 == other.M23 &&
                    M31 == other.M31 && M32 == other.M32);
        }
    }
}
