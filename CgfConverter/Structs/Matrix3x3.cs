using System;
using System.Numerics;

namespace CgfConverter.Structs
{
    public struct Matrix3x3    // a 3x3 transformation matrix
    {
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; }
        public float M21 { get; set; }
        public float M22 { get; set; }
        public float M23 { get; set; }
        public float M31 { get; set; }
        public float M32 { get; set; }
        public float M33 { get; set; }

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
    }
}
