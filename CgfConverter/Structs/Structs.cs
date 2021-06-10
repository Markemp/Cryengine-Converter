using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.LinearAlgebra;

namespace CgfConverter
{
    public struct RangeEntity
    {
        public string Name { get; set; } // String32!  32 byte char array.
        public int Start { get; set; }
        public int End { get; set; }
    } // String32 Name, int Start, int End - complete

    public struct Vector3
    {
        public double x { get; set; }
        public double y {get; set;}
        public double z { get; set; }
        public double w { get; set; } // Currently Unused

        public Vector3(double x, double y, double z) : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void ReadVector3(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            return;
        }

        public Vector3 Add(Vector3 vector)
        {
            Vector3 result = new Vector3
            {
                x = vector.x + x,
                y = vector.y + y,
                z = vector.z + z
            };
            return result;
        }

        public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3()
            {
                x = lhs.x + rhs.x,
                y = lhs.y + rhs.y,
                z = lhs.z + rhs.z
            };
        }

        public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3()
            {
                x = lhs.x - rhs.x,
                y = lhs.y - rhs.y,
                z = lhs.z - rhs.z
            };
        }

        public Vector4 ToVector4()
        {
            Vector4 result = new Vector4();
            result.x = x;
            result.y = y;
            result.z = z;
            result.w = 1;
            return result;
        }

        internal Vector<double> ToMathVector3()
        {
            Vector<double> result = Vector<double>.Build.Dense(3);
            result[0] = this.x;
            result[1] = this.y;
            result[2] = this.z;
            return result;
        }

        public Vector3 GetVector3(Vector<double> vector)
        {
            if (vector == null)
                throw new ArgumentNullException(nameof(vector));

            Vector3 result = new Vector3
            {
                x = vector[0],
                y = vector[1],
                z = vector[2]
            };
            return result;
        }

        public override string ToString()
        {
            return $@"x: {x:F8}, y: {y:F8}, z: {z:F8}";
        }
    }  // Vector in 3D space {x,y,z}

    public struct Vector4
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public double w { get; set; }

        public Vector4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector3 ToVector3()
        {
            Vector3 result = new Vector3();
            if (w == 0 || w == 1)
            {
                result.x = x;
                result.y = y;
                result.z = z;
            }
            else
            {
                result.x = x / w;
                result.y = y / w;
                result.z = z / w;
            }
            return result;
        }
    }

    public struct Matrix33    // a 3x3 transformation matrix
    {
        public double m11;
        public double m12;
        public double m13;
        public double m21;
        public double m22;
        public double m23;
        public double m31;
        public double m32;
        public double m33;

        public void ReadMatrix33(BinaryReader reader)
        {
            // Reads a Matrix33 structure
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            m11 = reader.ReadSingle();
            m12 = reader.ReadSingle();
            m13 = reader.ReadSingle();
            m21 = reader.ReadSingle();
            m22 = reader.ReadSingle();
            m23 = reader.ReadSingle();
            m31 = reader.ReadSingle();
            m32 = reader.ReadSingle();
            m33 = reader.ReadSingle();
        }

        public bool IsIdentity()
        {
            if (System.Math.Abs(m11 - 1.0) > 0.00001) { return false; }
            if (System.Math.Abs(m12) > 0.00001) { return false; }
            if (System.Math.Abs(m13) > 0.00001) { return false; }
            if (System.Math.Abs(m21) > 0.00001) { return false; }
            if (System.Math.Abs(m22 - 1.0) > 0.00001) { return false; }
            if (System.Math.Abs(m23) > 0.00001) { return false; }
            if (System.Math.Abs(m31) > 0.00001) { return false; }
            if (System.Math.Abs(m32) > 0.00001) { return false; }
            if (System.Math.Abs(m33 - 1.0) > 0.00001) { return false; }
            return true;
        }  // returns true if this is an identy matrix

        public Matrix33 GetCopy()    // returns a copy of the matrix33
        {
            Matrix33 mat = new Matrix33
            {
                m11 = m11,
                m12 = m12,
                m13 = m13,
                m21 = m21,
                m22 = m22,
                m23 = m23,
                m31 = m31,
                m32 = m32,
                m33 = m33
            };
            return mat;
        }

        public double GetDeterminant()
        {
            return (m11 * m22 * m33
                  + m12 * m23 * m31
                  + m13 * m21 * m32
                  - m31 * m22 * m13
                  - m21 * m12 * m33
                  - m11 * m32 * m23);
        }

        public Matrix33 GetTranspose()    // returns a copy of the matrix33
        {
            Matrix33 mat = new Matrix33
            {
                m11 = m11,
                m12 = m21,
                m13 = m31,
                m21 = m12,
                m22 = m22,
                m23 = m32,
                m31 = m13,
                m32 = m23,
                m33 = m33
            };
            return mat;
        }

        public Matrix33 Mult(Matrix33 mat)
        {
            Matrix33 mat2 = new Matrix33
            {
                m11 = (m11 * mat.m11) + (m12 * mat.m21) + (m13 * mat.m31),
                m12 = (m11 * mat.m12) + (m12 * mat.m22) + (m13 * mat.m32),
                m13 = (m11 * mat.m13) + (m12 * mat.m23) + (m13 * mat.m33),
                m21 = (m21 * mat.m11) + (m22 * mat.m21) + (m23 * mat.m31),
                m22 = (m21 * mat.m12) + (m22 * mat.m22) + (m23 * mat.m32),
                m23 = (m21 * mat.m13) + (m22 * mat.m23) + (m23 * mat.m33),
                m31 = (m31 * mat.m11) + (m32 * mat.m21) + (m33 * mat.m31),
                m32 = (m31 * mat.m12) + (m32 * mat.m22) + (m33 * mat.m32),
                m33 = (m31 * mat.m13) + (m32 * mat.m23) + (m33 * mat.m33)
            };
            return mat2;
        }

        public static Matrix33 operator *(Matrix33 lhs, Matrix33 rhs)
        {
            return lhs.Mult(rhs);
        }

        public Vector3 Mult3x1(Vector3 vector)
        {
            // Multiply the 3x3 matrix by a Vector 3 to get the rotation
            Vector3 result = new Vector3
            {
                x = (vector.x * m11) + (vector.y * m21) + (vector.z * m31),
                y = (vector.x * m12) + (vector.y * m22) + (vector.z * m32),
                z = (vector.x * m13) + (vector.y * m23) + (vector.z * m33)
            };
            return result;
        }

        public static Vector3 operator *(Matrix33 rhs, Vector3 lhs)
        {
            return rhs.Mult3x1(lhs);
        }

        public bool IsScaleRotation() // Returns true if the matrix decomposes nicely into scale * rotation\
        {
            Matrix33 self_transpose, mat = new Matrix33();
            self_transpose = GetTranspose();
            mat = Mult(self_transpose);
            if (Math.Abs(mat.m12) + Math.Abs(mat.m13)
                + Math.Abs(mat.m21) + Math.Abs(mat.m23)
                + Math.Abs(mat.m31) + Math.Abs(mat.m32) > 0.01)
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
            Matrix33 mat = Mult(GetTranspose());
            Vector3 scale = new Vector3();
            scale.x = (Double)Math.Pow(mat.m11, 0.5);
            scale.y = (Double)Math.Pow(mat.m22, 0.5);
            scale.z = (Double)Math.Pow(mat.m33, 0.5);
            if (GetDeterminant() < 0)
            {
                scale.x = 0 - scale.x;
                scale.y = 0 - scale.y;
                scale.z = 0 - scale.z;
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
            if (Math.Abs(scale.x - 1.0) > 0.01 || Math.Abs(scale.y - 1.0) > 0.01 || Math.Abs(scale.z - 1.0) > 0.1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a Math.Net matrix from a Cryengine Matrix33 object)
        /// </summary>
        /// <returns>New Math.Net matrix.</returns>
        public Matrix<double> ToMathMatrix()
        {
            Matrix<double> result = Matrix<double>.Build.Dense(3, 3);
            result[0, 0] = m11;
            result[0, 1] = m12;
            result[0, 2] = m13;
            result[1, 0] = m21;
            result[1, 1] = m22;
            result[1, 2] = m23;
            result[2, 0] = m31;
            result[2, 1] = m32;
            result[2, 2] = m33;
            return result;
        }

        public double Determinant()
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(3, 3);
            matrix = ToMathMatrix();
            return matrix.Determinant();
        }

        public Matrix33 Inverse()
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(3, 3);
            matrix = ToMathMatrix().Inverse();
            return GetMatrix33(matrix);
        }

        public Matrix33 Conjugate()
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(3, 3);
            matrix = ToMathMatrix().Conjugate();
            return GetMatrix33(matrix);
        }

        public Matrix33 ConjugateTranspose()
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(3, 3);
            matrix = ToMathMatrix().ConjugateTranspose();
            return GetMatrix33(matrix);
        }

        public Matrix33 ConjugateTransposeThisAndMultiply(Matrix33 inputMatrix)
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(3, 3);
            Matrix<double> matrix2 = Matrix<double>.Build.Dense(3, 3);
            matrix2 = inputMatrix.ToMathMatrix();
            matrix = ToMathMatrix().ConjugateTransposeThisAndMultiply(matrix2);
            return GetMatrix33(matrix);
        }

        public Vector3 Diagonal()
        {
            Vector3 result = new Vector3();
            Vector<double> vector = Vector<double>.Build.Dense(3);
            vector = this.ToMathMatrix().Diagonal();
            result = result.GetVector3(vector);
            return result;
        }

        public Matrix33 GetMatrix33(Matrix<double> matrix)
        {
            Matrix33 result = new Matrix33
            {
                m11 = matrix[0, 0],
                m12 = matrix[0, 1],
                m13 = matrix[0, 2],
                m21 = matrix[1, 0],
                m22 = matrix[1, 1],
                m23 = matrix[1, 2],
                m31 = matrix[2, 0],
                m32 = matrix[2, 1],
                m33 = matrix[2, 2]
            };
            return result;
        }

        public bool Equals(Matrix33 matrix)
        {
            if (
                matrix.m11 == m11 &&
                matrix.m12 == m12 &&
                matrix.m13 == m13 &&
                matrix.m21 == m21 &&
                matrix.m22 == m22 &&
                matrix.m23 == m23 &&
                matrix.m31 == m31 &&
                matrix.m32 == m32 &&
                matrix.m33 == m33)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// A 4x4 Transformation matrix.  These are row major matrices (m13 is first row, 3rd column).
    /// </summary>
    public struct Matrix44    // a 4x4 transformation matrix.  first value is row, second is column.
    {
        public double m11;
        public double m12;
        public double m13;
        public double m14;
        public double m21;
        public double m22;
        public double m23;
        public double m24;
        public double m31;
        public double m32;
        public double m33;
        public double m34;
        public double m41;
        public double m42;
        public double m43;
        public double m44;

        public Matrix44(Vector4 firstRow, Vector4 secondRow, Vector4 thirdRow, Vector4 fourthRow)
        {
            m11 = firstRow.x;
            m12 = secondRow.x;
            m13 = thirdRow.x;
            m14 = fourthRow.x;

            m21 = firstRow.y;
            m22 = secondRow.y;
            m23 = thirdRow.y;
            m24 = fourthRow.y;

            m31 = firstRow.z;
            m32 = secondRow.z;
            m33 = thirdRow.z;
            m34 = fourthRow.z;

            m41 = firstRow.w;
            m42 = secondRow.w;
            m43 = thirdRow.w;
            m44 = fourthRow.w;
        }
        public Vector4 Mult4x1(Vector4 vector)
        {
            // Pass the matrix a Vector4 (4x1) vector to get the transform of the vector
            Vector4 result = new Vector4
            {
                x = (m11 * vector.x) + (m21 * vector.y) + (m31 * vector.z) + (m41 * vector.w),
                y = (m12 * vector.x) + (m22 * vector.y) + (m32 * vector.z) + (m42 * vector.w),
                z = (m13 * vector.x) + (m23 * vector.y) + (m33 * vector.z) + (m43 * vector.w),
                w = (m14 * vector.x) + (m24 * vector.y) + (m34 * vector.z) + (m44 * vector.w)
            };

            return result;
        }

        public void ReadMatrix44(BinaryReader reader)
        {
            m11 = reader.ReadSingle();
            m12 = reader.ReadSingle();
            m13 = reader.ReadSingle();
            m14 = reader.ReadSingle();
            m21 = reader.ReadSingle();
            m22 = reader.ReadSingle();
            m23 = reader.ReadSingle();
            m24 = reader.ReadSingle();
            m31 = reader.ReadSingle();
            m32 = reader.ReadSingle();
            m33 = reader.ReadSingle();
            m34 = reader.ReadSingle();
            m41 = reader.ReadSingle();
            m42 = reader.ReadSingle();
            m43 = reader.ReadSingle();
            m44 = reader.ReadSingle();
        }

        public static Vector4 operator *(Matrix44 lhs, Vector4 vector)
        {
            Vector4 result = new Vector4();
            result.x = (lhs.m11 * vector.x) + (lhs.m21 * vector.y) + (lhs.m31 * vector.z) + (lhs.m41 * vector.w);
            result.y = (lhs.m12 * vector.x) + (lhs.m22 * vector.y) + (lhs.m32 * vector.z) + (lhs.m42 * vector.w);
            result.z = (lhs.m13 * vector.x) + (lhs.m23 * vector.y) + (lhs.m33 * vector.z) + (lhs.m43 * vector.w);
            result.w = (lhs.m14 * vector.x) + (lhs.m24 * vector.y) + (lhs.m34 * vector.z) + (lhs.m44 * vector.w);
            
            return result;
        }

        public static Vector3 operator *(Matrix44 lhs, Vector3 vector)
        {
            Vector3 result = new Vector3();
            result.x = (lhs.m11 * vector.x) + (lhs.m21 * vector.y) + (lhs.m31 * vector.z) + lhs.m41;
            result.y = (lhs.m12 * vector.x) + (lhs.m22 * vector.y) + (lhs.m32 * vector.z) + lhs.m42;
            result.z = (lhs.m13 * vector.x) + (lhs.m23 * vector.y) + (lhs.m33 * vector.z) + lhs.m43;
            result.w = (lhs.m14 * vector.x) + (lhs.m24 * vector.y) + (lhs.m34 * vector.z) + lhs.m44;

            return result;
        }

        public static Matrix44 operator *(Matrix44 lhs, Matrix44 rhs)
        {
            Matrix44 result = new Matrix44();
            result.m11 = (lhs.m11 * rhs.m11) + (lhs.m12 * rhs.m21) + (lhs.m13 * rhs.m31) + (lhs.m14 * rhs.m41);  // First row
            result.m12 = (lhs.m11 * rhs.m12) + (lhs.m12 * rhs.m22) + (lhs.m13 * rhs.m32) + (lhs.m14 * rhs.m42);
            result.m13 = (lhs.m11 * rhs.m13) + (lhs.m12 * rhs.m23) + (lhs.m13 * rhs.m33) + (lhs.m14 * rhs.m43);
            result.m14 = (lhs.m11 * rhs.m14) + (lhs.m12 * rhs.m24) + (lhs.m13 * rhs.m34) + (lhs.m14 * rhs.m44);

            result.m21 = (lhs.m21 * rhs.m11) + (lhs.m22 * rhs.m21) + (lhs.m23 * rhs.m31) + (lhs.m24 * rhs.m41);  // second row
            result.m22 = (lhs.m21 * rhs.m12) + (lhs.m22 * rhs.m22) + (lhs.m23 * rhs.m32) + (lhs.m24 * rhs.m42);
            result.m23 = (lhs.m21 * rhs.m13) + (lhs.m22 * rhs.m23) + (lhs.m23 * rhs.m33) + (lhs.m24 * rhs.m43);
            result.m24 = (lhs.m21 * rhs.m14) + (lhs.m22 * rhs.m24) + (lhs.m23 * rhs.m34) + (lhs.m24 * rhs.m44);

            result.m31 = (lhs.m31 * rhs.m11) + (lhs.m32 * rhs.m21) + (lhs.m33 * rhs.m31) + (lhs.m34 * rhs.m41);  // third row
            result.m32 = (lhs.m31 * rhs.m12) + (lhs.m32 * rhs.m22) + (lhs.m33 * rhs.m32) + (lhs.m34 * rhs.m42);
            result.m33 = (lhs.m31 * rhs.m13) + (lhs.m32 * rhs.m23) + (lhs.m33 * rhs.m33) + (lhs.m34 * rhs.m43);
            result.m34 = (lhs.m31 * rhs.m14) + (lhs.m32 * rhs.m24) + (lhs.m33 * rhs.m34) + (lhs.m34 * rhs.m44);

            result.m41 = (lhs.m41 * rhs.m11) + (lhs.m42 * rhs.m21) + (lhs.m43 * rhs.m31) + (lhs.m44 * rhs.m41);  // fourth row
            result.m42 = (lhs.m41 * rhs.m12) + (lhs.m42 * rhs.m22) + (lhs.m43 * rhs.m32) + (lhs.m44 * rhs.m42);
            result.m43 = (lhs.m41 * rhs.m13) + (lhs.m42 * rhs.m23) + (lhs.m43 * rhs.m33) + (lhs.m44 * rhs.m43);
            result.m44 = (lhs.m41 * rhs.m14) + (lhs.m42 * rhs.m24) + (lhs.m43 * rhs.m34) + (lhs.m44 * rhs.m44);

            return result;
        }

        public Vector3 GetTranslation()
        {
            return new Vector3
            {
                x = m14,
                y = m24,
                z = m34
            };
        }

        /// <summary>
        /// Gets the Rotation portion of a Transform Matrix44 (upper left).
        /// </summary>
        /// <returns>New Matrix33 with the rotation component.</returns>
        public Matrix33 GetRotation()
        {
            return new Matrix33()
            {
                m11 = this.m11,
                m12 = this.m12,
                m13 = this.m13,
                m21 = this.m21,
                m22 = this.m22,
                m23 = this.m23,
                m31 = this.m31,
                m32 = this.m32,
                m33 = this.m33,
            };
        }

        public Vector3 GetScale()
        {
            return new Vector3
            {
                x = m41,
                y = m42,
                z = m43 
            };
        }

        public Vector3 GetBoneTranslation()
        {
            return new Vector3
            {
                x = m14,
                y = m24,
                z = m34
            };
        }

        public double[,] ConvertTo4x4Array()
        {
            double[,] result = new double[4, 4];
            result[0, 0] = this.m11;
            result[0, 1] = this.m12;
            result[0, 2] = this.m13;
            result[0, 3] = this.m14;
            result[1, 0] = this.m21;
            result[1, 1] = this.m22;
            result[1, 2] = this.m23;
            result[1, 3] = this.m24;
            result[2, 0] = this.m31;
            result[2, 1] = this.m32;
            result[2, 2] = this.m33;
            result[2, 3] = this.m34;
            result[3, 0] = this.m41;
            result[3, 1] = this.m42;
            result[3, 2] = this.m43;
            result[3, 3] = this.m44;

            return result;
        }

        public Matrix44 Inverse()
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(4, 4);
            matrix = ToMathMatrix().Inverse();
            return GetMatrix44(matrix);
        }

        public Matrix44 GetTransformFromParts(Vector3 localTranslation, Matrix33 localRotation)
        {
            var defaultScale = new Vector3
            {
                x = 0.0f,
                y = 0.0f,
                z = 0.0f
            };
            return GetTransformFromParts(localTranslation, localRotation, defaultScale);
        }

        public Matrix44 GetTransformFromParts(Vector3 localTranslation, Matrix33 localRotation, Vector3 localScale)
        {
            Matrix44 transform = new Matrix44
            {
                // For Node Chunks, the translation appears to be along the bottom of the matrix, and scale on right side.
                // Translation part
                m41 = localTranslation.x,
                m42 = localTranslation.y,
                m43 = localTranslation.z,
                // Rotation part.  Invert this matrix, which results in proper rotation in Blender.
                m11 = localRotation.m11,
                m12 = localRotation.m21,
                m13 = localRotation.m31,
                m21 = localRotation.m12,
                m22 = localRotation.m22,
                m23 = localRotation.m32,
                m31 = localRotation.m13,
                m32 = localRotation.m23,
                m33 = localRotation.m33,
                // Scale part
                m14 = localScale.x,
                m24 = localScale.y,
                m34 = localScale.z,
                // Set final row
                m44 = 1
            };
            return transform;
        }

        public static Matrix44 Identity()
        {
            return new Matrix44()
            {
                m11 = 1,
                m12 = 0,
                m13 = 0,
                m14 = 0,
                m21 = 0,
                m22 = 1,
                m23 = 0,
                m24 = 0,
                m31 = 0,
                m32 = 0,
                m33 = 1,
                m34 = 0,
                m41 = 0,
                m42 = 0,
                m43 = 0,
                m44 = 1
            };
        }

        public static Matrix44 CreateDefaultRootNodeMatrix()
        {
            return new Matrix44()
            {
                m11 = 1,
                m12 = 0,
                m13 = 0,
                m14 = 0,
                m21 = 0,
                m22 = 1,
                m23 = 0,
                m24 = 0,
                m31 = 0,
                m32 = 0,
                m33 = 1,
                m34 = 0,
                m41 = 1,
                m42 = 1,
                m43 = 1,
                m44 = 0
            };
        }

        public Matrix<double> ToMathMatrix()
        {
            Matrix<double> result = Matrix<double>.Build.Dense(4, 4);
            result[0, 0] = this.m11;
            result[0, 1] = this.m12;
            result[0, 2] = this.m13;
            result[0, 3] = this.m14;
            result[1, 0] = this.m21;
            result[1, 1] = this.m22;
            result[1, 2] = this.m23;
            result[1, 3] = this.m24;
            result[2, 0] = this.m31;
            result[2, 1] = this.m32;
            result[2, 2] = this.m33;
            result[2, 3] = this.m34;
            result[3, 0] = this.m41;
            result[3, 1] = this.m42;
            result[3, 2] = this.m43;
            result[3, 3] = this.m44;
            return result;
        }

        public Matrix44 GetMatrix44(Matrix<double> matrix)
        {
            Matrix44 result = new Matrix44
            {
                m11 = matrix[0, 0],
                m12 = matrix[0, 1],
                m13 = matrix[0, 2],
                m14 = matrix[0, 3],
                m21 = matrix[1, 0],
                m22 = matrix[1, 1],
                m23 = matrix[1, 2],
                m24 = matrix[1, 3],
                m31 = matrix[2, 0],
                m32 = matrix[2, 1],
                m33 = matrix[2, 2],
                m34 = matrix[2, 3],
                m41 = matrix[3, 0],
                m42 = matrix[3, 1],
                m43 = matrix[3, 2],
                m44 = matrix[3, 3],
            };
            return result;
        }
    }

    /// <summary>
    /// A quaternion (x,y,z,w)
    /// </summary>
    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Matrix33 ConvertToRotationalMatrix()
        {
            // https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
            var rotationalMatrix = new Matrix33();
            double sqw = w * w;
            double sqx = x * x;
            double sqy = y * y;
            double sqz = z * z;

            // invs (inverse square length) is only required if quaternion is not already normalised
            double invs = 1 / (sqx + sqy + sqz + sqw);
            rotationalMatrix.m11 = (sqx - sqy - sqz + sqw) * invs; // since sqw + sqx + sqy + sqz =1/invs*invs
            rotationalMatrix.m22 = (-sqx + sqy - sqz + sqw) * invs;
            rotationalMatrix.m33 = (-sqx - sqy + sqz + sqw) * invs;

            double tmp1 = x * y;
            double tmp2 = z * w;
            rotationalMatrix.m21 = 2.0 * (tmp1 + tmp2) * invs;
            rotationalMatrix.m12 = 2.0 * (tmp1 - tmp2) * invs;

            tmp1 = x * z;
            tmp2 = y * w;
            rotationalMatrix.m31 = 2.0 * (tmp1 - tmp2) * invs;
            rotationalMatrix.m13 = 2.0 * (tmp1 + tmp2) * invs;
            tmp1 = y * z;
            tmp2 = x * w;
            rotationalMatrix.m32 = 2.0 * (tmp1 + tmp2) * invs;
            rotationalMatrix.m23 = 2.0 * (tmp1 - tmp2) * invs;

            return rotationalMatrix;
        }

        public override string ToString()
        {
            return $@"x: {x}, y: {y}, z: {z}, w: {w}";
        }
    }

    /// <summary>
    /// Vertex with position p(Vector3) and normal n(Vector3)
    /// </summary>
    public struct Vertex
    {
        public Vector3 p;  // position
        public Vector3 n;  // normal
    }

    public struct Face        // mesh face (3 vertex, Material index, smoothing group.  All ints)
    {
        public int v0; // first vertex
        public int v1; // second vertex
        public int v2; // third vertex
        public int Material; // Material Index
        public int SmGroup; //smoothing group
    }

    public struct MeshSubset
    {
        public int FirstIndex;
        public int NumIndices;
        public int FirstVertex;
        public int NumVertices;
        public int MatID;
        public float Radius;
        public Vector3 Center;
    }  // Contains data about the parts of a mesh, such as vertices, radius and center.

    public struct Key
    {
        public int Time; // Time in ticks
        public Vector3 AbsPos; // absolute position
        public Vector3 RelPos; // relative position
        public Quaternion RelQuat; //Relative Quaternion if ARG==1?
        public Vector3 Unknown1; // If ARG==6 or 10?
        public Double[] Unknown2; // If ARG==9?  array length = 2
    }

    public struct UV
    {
        public Double U;
        public Double V;
    }

    public struct UVFace
    {
        public int t0; // first vertex index
        public int t1; // second vertex index
        public int t2; // third vertex index
    }

    public struct ControllerInfo
    {
        public uint ControllerID;
        public uint PosKeyTimeTrack;
        public uint PosTrack;
        public uint RotKeyTimeTrack;
        public uint RotTrack;
    }

    /*public struct TextureMap
    {

    }*/
    // Fill this in later.  line 369 in cgf.xml.

    public struct IRGB
    {
        public byte r; // red
        public byte g; // green
        public byte b; // blue

        public IRGB Read(BinaryReader b)
        {
            return new IRGB
            {
                r = b.ReadByte(),
                g = b.ReadByte(),
                b = b.ReadByte()
            };
        }
    }

    public struct IRGBA
    {
        public byte r; // red
        public byte g; // green
        public byte b; // blue
        public byte a; // alpha
        public IRGBA Read(BinaryReader b)
        {
            return new IRGBA
            {
                r = b.ReadByte(),
                g = b.ReadByte(),
                b = b.ReadByte(),
                a = b.ReadByte()
            };
        }
    }   // May also be known as ColorB.

    public struct FRGB
    {
        public double r; // Double Red
        public double g; // Double green
        public double b; // Double blue
    }

    public struct AABB
    {
        Vector3 min;
        Vector3 max;
    }

    public struct Tangent
    {
        // Tangents.  Divide each component by 32767 to get the actual value
        public double x;
        public double y;
        public double z;
        public double w;  // Handness?  Either 32767 (+1.0) or -32767 (-1.0)
    }

    public struct SkinVertex
    {
        public int Volumetric;
        public int[] Index;     // Array of 4 ints
        public float[] w;       // Array of 4 floats
        public Matrix33 M;
    }

    /// <summary> WORLDTOBONE is also the Bind Pose Matrix (BPM) </summary>
    public struct WORLDTOBONE
    {
        public double[,] worldToBone;   //  4x3 structure

        public WORLDTOBONE(Matrix33 worldRotation, Vector3 worldTransform) : this()
        {
            worldToBone = new double[3, 4];
            worldToBone[0, 0] = worldRotation.m11;
            worldToBone[0, 1] = worldRotation.m12;
            worldToBone[0, 2] = worldRotation.m13;
            worldToBone[1, 0] = worldRotation.m21;
            worldToBone[1, 1] = worldRotation.m22;
            worldToBone[1, 2] = worldRotation.m23;
            worldToBone[2, 0] = worldRotation.m31;
            worldToBone[2, 1] = worldRotation.m32;
            worldToBone[2, 2] = worldRotation.m33;
            worldToBone[0, 3] = worldTransform.x;
            worldToBone[1, 3] = worldTransform.y;
            worldToBone[2, 3] = worldTransform.z;
        }

        public void GetWorldToBone(BinaryReader b)
        {
            worldToBone = new double[3, 4];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    worldToBone[i, j] = b.ReadSingle();  // this might have to be switched to [j,i].  Who knows???
                }
            }
            return;
        }

        public Matrix44 GetMatrix44()
        {
            Matrix44 matrix = new Matrix44
            {
                m11 = worldToBone[0, 0],
                m12 = worldToBone[0, 1],
                m13 = worldToBone[0, 2],
                m14 = worldToBone[0, 3],
                m21 = worldToBone[1, 0],
                m22 = worldToBone[1, 1],
                m23 = worldToBone[1, 2],
                m24 = worldToBone[1, 3],
                m31 = worldToBone[2, 0],
                m32 = worldToBone[2, 1],
                m33 = worldToBone[2, 2],
                m34 = worldToBone[2, 3],
                m41 = 0,
                m42 = 0,
                m43 = 0,
                m44 = 1
            };
            return matrix;

        }

        internal Matrix33 GetWorldToBoneRotationMatrix()
        {
            Matrix33 result = new Matrix33
            {
                m11 = worldToBone[0, 0],
                m12 = worldToBone[0, 1],
                m13 = worldToBone[0, 2],
                m21 = worldToBone[1, 0],
                m22 = worldToBone[1, 1],
                m23 = worldToBone[1, 2],
                m31 = worldToBone[2, 0],
                m32 = worldToBone[2, 1],
                m33 = worldToBone[2, 2]
            };
            return result;
        }

        internal Vector3 GetWorldToBoneTranslationVector()
        {
            Vector3 result = new Vector3
            {
                x = worldToBone[0, 3],
                y = worldToBone[1, 3],
                z = worldToBone[2, 3]
            };
            return result;
        }
    }

    /// <summary> BONETOWORLD contains the world space location/rotation of a bone. </summary>
    public struct BONETOWORLD
    {
        public double[,] boneToWorld;   //  4x3 structure

        public BONETOWORLD(Matrix33 matrix33, Vector3 relativeTransform) : this()
        {
            boneToWorld = new double[3, 4];
            boneToWorld[0, 0] = matrix33.m11;
            boneToWorld[0, 1] = matrix33.m12;
            boneToWorld[0, 2] = matrix33.m13;
            boneToWorld[1, 0] = matrix33.m21;
            boneToWorld[1, 1] = matrix33.m22;
            boneToWorld[1, 2] = matrix33.m23;
            boneToWorld[2, 0] = matrix33.m31;
            boneToWorld[2, 1] = matrix33.m32;
            boneToWorld[2, 2] = matrix33.m33;
            boneToWorld[0, 3] = relativeTransform.x;
            boneToWorld[1, 3] = relativeTransform.y;
            boneToWorld[2, 3] = relativeTransform.z;
        }

        public void ReadBoneToWorld(BinaryReader b)
        {
            boneToWorld = new double[3, 4];
            //Utils.Log(LogLevelEnum.Debug, "GetBoneToWorld");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    boneToWorld[i, j] = b.ReadSingle();  // this might have to be switched to [j,i].  Who knows???
                    //Utils.Log(LogLevelEnum.Debug, "boneToWorld: {0:F7}", boneToWorld[i, j]);
                }
            }
            return;
        }

        /// <summary> Returns the world space rotational matrix in a Math.net 3x3 matrix. </summary>
        /// <returns>Matrix33</returns>
        public Matrix33 GetBoneToWorldRotationMatrix()
        {
            Matrix33 result = new Matrix33
            {
                m11 = boneToWorld[0, 0],
                m12 = boneToWorld[0, 1],
                m13 = boneToWorld[0, 2],
                m21 = boneToWorld[1, 0],
                m22 = boneToWorld[1, 1],
                m23 = boneToWorld[1, 2],
                m31 = boneToWorld[2, 0],
                m32 = boneToWorld[2, 1],
                m33 = boneToWorld[2, 2]
            };
            return result;
        }

        public Vector3 GetBoneToWorldTranslationVector()
        {
            Vector3 result = new Vector3
            {
                x = boneToWorld[0, 3],
                y = boneToWorld[1, 3],
                z = boneToWorld[2, 3]
            };
            return result;
        }
    }

    public struct PhysicsGeometry
    {
        public uint physicsGeom;
        public uint flags;              // 0x0C ?
        public Vector3 min;
        public Vector3 max;
        public Vector3 spring_angle;
        public Vector3 spring_tension;
        public Vector3 damping;
        public Matrix33 framemtx;

        public void ReadPhysicsGeometry(BinaryReader b)      // Read a PhysicsGeometry structure
        {
            physicsGeom = b.ReadUInt32();
            flags = b.ReadUInt32();
            min.ReadVector3(b);
            max.ReadVector3(b);
            spring_angle.ReadVector3(b);
            spring_tension.ReadVector3(b);
            damping.ReadVector3(b);
            framemtx.ReadMatrix33(b);
            return;
        }
        public void WritePhysicsGeometry()
        {
            Utils.Log(LogLevelEnum.Verbose, "WritePhysicsGeometry");
        }
    }

    public class CompiledPhysicalBone
    {
        public uint BoneIndex;
        public uint ParentOffset;
        public uint NumChildren;
        public uint ControllerID;
        public char[] prop;
        public PhysicsGeometry PhysicsGeometry;

        // Calculated values
        public long offset;
        public uint parentID;                       // ControllerID of parent
        public List<uint> childIDs;                 // Not part of read struct.  Contains the controllerIDs of the children to this bone.

        public CompiledBone GetBonePartner()
        {
            return null;
        }

        public void ReadCompiledPhysicalBone(BinaryReader b)
        {
            // Reads just a single 584 byte entry of a bone. At the end the seek position will be advanced, so keep that in mind.
            BoneIndex = b.ReadUInt32();                 // unique id of bone (generated from bone name)
            ParentOffset = b.ReadUInt32();
            NumChildren = b.ReadUInt32();
            ControllerID = b.ReadUInt32();
            prop = b.ReadChars(32);                    // Not sure what this is used for.
            PhysicsGeometry.ReadPhysicsGeometry(b);

            childIDs = new List<uint>();                    // Calculated
        }
    }

    public struct InitialPosMatrix
    {
        // A bone initial position matrix.
        Matrix33 Rotation;              // type="Matrix33">
        Vector3 Position;                // type="Vector3">
    }

    public struct BoneLink
    {
        public int BoneID;
        public Vector3 offset;
        public float Blending;
    }


    public class DirectionalBlends
    {
        public string AnimToken;
        public uint AnimTokenCRC32;
        public string ParaJointName;
        public short ParaJointIndex;
        public short RotParaJointIndex;
        public string StartJointName;
        public short StartJointIndex;
        public short RotStartJointIndex;
        public string ReferenceJointName;
        public short ReferenceJointIndex;

        public DirectionalBlends()
        {
            AnimToken = string.Empty;
            AnimTokenCRC32 = 0;
            ParaJointName = string.Empty;
            ParaJointIndex = -1;
            RotParaJointIndex = -1;
            StartJointName = string.Empty;
            StartJointIndex = -1;
            RotStartJointIndex = -1;
            ReferenceJointName = string.Empty;
            ReferenceJointIndex = 1;  //by default we use the Pelvis
        }
    };

    #region Skinning Structures

    public struct BoneEntity
    {
        readonly int Bone_Id;                 //" type="int">Bone identifier.</add>
        readonly int Parent_Id;               //" type="int">Parent identifier.</add>
        readonly int Num_Children;            //" type="uint" />
        readonly uint Bone_Name_CRC32;         //" type="uint">CRC32 of bone name as listed in the BoneNameListChunk.  In Python this can be calculated using zlib.crc32(name)</add>
        readonly string Properties;            //" type="String32" />
        BonePhysics Physics;            //" type="BonePhysics" />
    }

    public struct BonePhysics           // 26 total words = 104 total bytes
    {
        readonly uint Geometry;                //" type="Ref" template="BoneMeshChunk">Geometry of a separate mesh for this bone.</add>
                                                 //<!-- joint parameters -->

        readonly uint Flags;                   //" type="uint" />
        Vector3 Min;                   //" type="Vector3" />
        Vector3 Max;                   //" type="Vector3" />
        Vector3 Spring_Angle;          //" type="Vector3" />
        Vector3 Spring_Tension;        //" type="Vector3" />
        Vector3 Damping;               //" type="Vector3" />
        Matrix33 Frame_Matrix;        //" type="Matrix33" />
    }

    public struct MeshBoneMapping
    {
        // 4 bones, 4 weights for each vertex mapping.
        public int[] BoneIndex;
        public int[] Weight;                    // Byte / 256?
    }

    public struct MeshPhysicalProxyHeader
    {
        public uint ChunkID;
        public uint NumPoints;
        public uint NumIndices;
        public uint NumMaterials;
    }

    public struct MeshMorphTargetHeader
    {
        public uint MeshID;
        public uint NameLength;
        public uint NumIntVertices;
        public uint NumExtVertices;
    }

    public struct MeshMorphTargetVertex
    {
        public uint VertexID;
        public Vector3 Vertex;

        public static MeshMorphTargetVertex Read(BinaryReader b)
        {
            MeshMorphTargetVertex vertex = new MeshMorphTargetVertex();
            vertex.VertexID = b.ReadUInt32();
            vertex.Vertex.ReadVector3(b);
            return vertex;
        }
    }

    public struct MorphTargets
    {
        readonly uint MeshID;
        readonly string Name;
        readonly List<MeshMorphTargetVertex> IntMorph;
        readonly List<MeshMorphTargetVertex> ExtMorph;
    }

    public struct TFace
    {
        public UInt16 I0 { get; set; }
        public UInt16 I1 { get; set; }
        public UInt16 I2 { get; set; }

        //public static bool operator =(TFace face)
        //{
        //    if (face.i0 == i0 && face.i1 == i1 && face.i2 == i2)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }

    public class MeshCollisionInfo
    {
        // AABB AABB;       // Bounding box structures?
        // OBB OBB;         // Has an m33, h and c value.
        public Vector3 Position;
        public List<short> Indices;
        public int BoneID;

        public MeshCollisionInfo()
        {

        }
    }

    public struct IntSkinVertex
    {
        public Vector3 Obsolete0;
        public Vector3 Position;
        public Vector3 Obsolete2;
        public ushort[] BoneIDs;     // 4 bone IDs
        public float[] Weights;     // Should be 4 of these
        public IRGBA Color;
    }

    public struct SpeedChunk
    {
        public float Speed;
        public float Distance;
        public float Slope;
        public int AnimFlags;
        public float[] MoveDir;
        public Quaternion StartPosition;
    }

    public struct PhysicalProxy
    {
        public uint ID;             // Chunk ID (although not technically a chunk
        public uint FirstIndex;
        public uint NumIndices;
        public uint FirstVertex;
        public uint NumVertices;
        public uint Material;     // Size of the weird data at the end of the hitbox structure.
        public Vector3[] Vertices;    // Array of vertices (x,y,z) length NumVertices
        public UInt16[] Indices;      // Array of indices
    }

    public struct PhysicalProxyStub
    {
        readonly uint ChunkID;
        readonly List<Vector3> Points;
        readonly List<short> Indices;
        readonly List<string> Materials;
    }

    #endregion

    public struct PhysicsCube
    {
        public PhysicsStruct1 Unknown14;
        public PhysicsStruct1 Unknown15;
        public int Unknown16;
    }

    public struct PhysicsPolyhedron
    {
        public uint NumVertices;
        public uint NumTriangles;
        public int Unknown17;
        public int Unknown18;
        public byte HasVertexMap;
        public ushort[] VertexMap; // Array length NumVertices.  If the (non-physics) mesh has say 200 vertices, then the first 200
                                   //entries of this map give a mapping identifying the unique vertices.
                                   //The meaning of the extra entries is unknown.
        public byte UseDatasStream;
        public Vector3[] Vertices; // Array Length NumVertices
        public ushort[] Triangles; // Array length NumTriangles
        public byte Unknown210;
        public byte[] TriangleFlags; // Array length NumTriangles
        public ushort[] TriangleMap; // Array length NumTriangles
        public byte[] Unknown45; // Array length 16
        public int Unknown461;  //0
        public int Unknown462;  //0
        public float Unknown463; // 0.02
        public float Unknown464;
        // There is more.  See cgf.xml for the rest, but probably not really needed
    }

    public struct PhysicsCylinder
    {
        public float[] Unknown1;  // array length 8
        public int Unknown2;
        public PhysicsDataType2 Unknown3;
    }

    public struct PhysicsShape6
    {
        public float[] Unknown1; // array length 8
        public int Unknown2;
        public PhysicsDataType2 Unknown3;
    }

    public struct PhysicsDataType0
    {
        public int NumData;
        public PhysicsStruct2[] Data; // Array length NumData
        public int[] Unknown33; // array length 3
        public float Unknown80;
    }

    public struct PhysicsDataType1
    {
        public uint NumData1;  // usually 4294967295
        public PhysicsStruct50[] Data1; // Array length NumData1
        public int NumData2;
        public PhysicsStruct50[] Data2; // Array length NumData2
        public float[] Unknown60; // array length 6
        public Matrix33 Unknown61; // Rotation matrix?
        public int[] Unknown70; //Array length 3
        public float Unknown80;
    }

    public struct PhysicsDataType2
    {
        public Matrix33 Unknown1;
        public int Unknown;
        public float[] Unknown3; // array length 6
        public int Unknown4;
    }

    public struct PhysicsStruct1
    {
        public Matrix33 Unknown1;
        public int Unknown2;
        public float[] Unknown3; // array length 6
    }

    public struct PhysicsStruct2
    {
        public Matrix33 Unknown1;
        public float[] Unknown2;  // array length 6
        public int[] Unknown3; // array length 3
    }

    public struct PhysicsStruct50
    {
        public short Unknown11;
        public short Unknown12;
        public short Unknown21;
        public short Unknown22;
        public short Unknown23;
        public short Unknown24;
    }
}