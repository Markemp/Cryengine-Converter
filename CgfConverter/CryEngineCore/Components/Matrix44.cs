using System.Numerics;

namespace CgfConverter.CryEngineCore.Components
{
    public struct Matrix4x4    // Extensions for System.Numerics Matrix4x4
    {
        //public double m11;
        //public double m12;
        //public double m13;
        //public double m14;
        //public double m21;
        //public double m22;
        //public double m23;
        //public double m24;
        //public double m31;
        //public double m32;
        //public double m33;
        //public double m34;
        //public double m41;
        //public double m42;
        //public double m43;
        //public double m44;

        //public Matrix44(Vector4 firstRow, Vector4 secondRow, Vector4 thirdRow, Vector4 fourthRow)
        //{
        //    m11 = firstRow.x;
        //    m12 = secondRow.x;
        //    m13 = thirdRow.x;
        //    m14 = fourthRow.x;

        //    m21 = firstRow.y;
        //    m22 = secondRow.y;
        //    m23 = thirdRow.y;
        //    m24 = fourthRow.y;

        //    m31 = firstRow.z;
        //    m32 = secondRow.z;
        //    m33 = thirdRow.z;
        //    m34 = fourthRow.z;

        //    m41 = firstRow.w;
        //    m42 = secondRow.w;
        //    m43 = thirdRow.w;
        //    m44 = fourthRow.w;
        //}
        //public Vector4 Mult4x1(Vector4 vector)
        //{
        //    // Pass the matrix a Vector4 (4x1) vector to get the transform of the vector
        //    var result = new Vector4
        //    {
        //        x = (m11 * vector.x) + (m21 * vector.y) + (m31 * vector.z) + (m41 * vector.w),
        //        y = (m12 * vector.x) + (m22 * vector.y) + (m32 * vector.z) + (m42 * vector.w),
        //        z = (m13 * vector.x) + (m23 * vector.y) + (m33 * vector.z) + (m43 * vector.w),
        //        w = (m14 * vector.x) + (m24 * vector.y) + (m34 * vector.z) + (m44 * vector.w)
        //    };

        //    return result;
        //}

        //public void ReadMatrix44(BinaryReader reader)
        //{
        //    m11 = reader.ReadSingle();
        //    m12 = reader.ReadSingle();
        //    m13 = reader.ReadSingle();
        //    m14 = reader.ReadSingle();
        //    m21 = reader.ReadSingle();
        //    m22 = reader.ReadSingle();
        //    m23 = reader.ReadSingle();
        //    m24 = reader.ReadSingle();
        //    m31 = reader.ReadSingle();
        //    m32 = reader.ReadSingle();
        //    m33 = reader.ReadSingle();
        //    m34 = reader.ReadSingle();
        //    m41 = reader.ReadSingle();
        //    m42 = reader.ReadSingle();
        //    m43 = reader.ReadSingle();
        //    m44 = reader.ReadSingle();
        //}

        //public static Vector4 operator *(Matrix44 lhs, Vector4 vector)
        //{
        //    Vector4 result = new Vector4();
        //    result.x = (lhs.m11 * vector.x) + (lhs.m21 * vector.y) + (lhs.m31 * vector.z) + (lhs.m41 * vector.w);
        //    result.y = (lhs.m12 * vector.x) + (lhs.m22 * vector.y) + (lhs.m32 * vector.z) + (lhs.m42 * vector.w);
        //    result.z = (lhs.m13 * vector.x) + (lhs.m23 * vector.y) + (lhs.m33 * vector.z) + (lhs.m43 * vector.w);
        //    result.w = (lhs.m14 * vector.x) + (lhs.m24 * vector.y) + (lhs.m34 * vector.z) + (lhs.m44 * vector.w);

        //    return result;
        //}

        //public static Vector3 operator *(Matrix44 lhs, Vector3 vector)
        //{
        //    Vector3 result = new Vector3();
        //    result.x = (lhs.m11 * vector.x) + (lhs.m21 * vector.y) + (lhs.m31 * vector.z) + lhs.m41;
        //    result.y = (lhs.m12 * vector.x) + (lhs.m22 * vector.y) + (lhs.m32 * vector.z) + lhs.m42;
        //    result.z = (lhs.m13 * vector.x) + (lhs.m23 * vector.y) + (lhs.m33 * vector.z) + lhs.m43;
        //    result.w = (lhs.m14 * vector.x) + (lhs.m24 * vector.y) + (lhs.m34 * vector.z) + lhs.m44;

        //    return result;
        //}

        //public static Matrix44 operator *(Matrix44 lhs, Matrix44 rhs)
        //{
        //    Matrix44 result = new Matrix44();
        //    result.m11 = (lhs.m11 * rhs.m11) + (lhs.m12 * rhs.m21) + (lhs.m13 * rhs.m31) + (lhs.m14 * rhs.m41);  // First row
        //    result.m12 = (lhs.m11 * rhs.m12) + (lhs.m12 * rhs.m22) + (lhs.m13 * rhs.m32) + (lhs.m14 * rhs.m42);
        //    result.m13 = (lhs.m11 * rhs.m13) + (lhs.m12 * rhs.m23) + (lhs.m13 * rhs.m33) + (lhs.m14 * rhs.m43);
        //    result.m14 = (lhs.m11 * rhs.m14) + (lhs.m12 * rhs.m24) + (lhs.m13 * rhs.m34) + (lhs.m14 * rhs.m44);

        //    result.m21 = (lhs.m21 * rhs.m11) + (lhs.m22 * rhs.m21) + (lhs.m23 * rhs.m31) + (lhs.m24 * rhs.m41);  // second row
        //    result.m22 = (lhs.m21 * rhs.m12) + (lhs.m22 * rhs.m22) + (lhs.m23 * rhs.m32) + (lhs.m24 * rhs.m42);
        //    result.m23 = (lhs.m21 * rhs.m13) + (lhs.m22 * rhs.m23) + (lhs.m23 * rhs.m33) + (lhs.m24 * rhs.m43);
        //    result.m24 = (lhs.m21 * rhs.m14) + (lhs.m22 * rhs.m24) + (lhs.m23 * rhs.m34) + (lhs.m24 * rhs.m44);

        //    result.m31 = (lhs.m31 * rhs.m11) + (lhs.m32 * rhs.m21) + (lhs.m33 * rhs.m31) + (lhs.m34 * rhs.m41);  // third row
        //    result.m32 = (lhs.m31 * rhs.m12) + (lhs.m32 * rhs.m22) + (lhs.m33 * rhs.m32) + (lhs.m34 * rhs.m42);
        //    result.m33 = (lhs.m31 * rhs.m13) + (lhs.m32 * rhs.m23) + (lhs.m33 * rhs.m33) + (lhs.m34 * rhs.m43);
        //    result.m34 = (lhs.m31 * rhs.m14) + (lhs.m32 * rhs.m24) + (lhs.m33 * rhs.m34) + (lhs.m34 * rhs.m44);

        //    result.m41 = (lhs.m41 * rhs.m11) + (lhs.m42 * rhs.m21) + (lhs.m43 * rhs.m31) + (lhs.m44 * rhs.m41);  // fourth row
        //    result.m42 = (lhs.m41 * rhs.m12) + (lhs.m42 * rhs.m22) + (lhs.m43 * rhs.m32) + (lhs.m44 * rhs.m42);
        //    result.m43 = (lhs.m41 * rhs.m13) + (lhs.m42 * rhs.m23) + (lhs.m43 * rhs.m33) + (lhs.m44 * rhs.m43);
        //    result.m44 = (lhs.m41 * rhs.m14) + (lhs.m42 * rhs.m24) + (lhs.m43 * rhs.m34) + (lhs.m44 * rhs.m44);

        //    return result;
        //}

        //public Vector3 GetTranslation()
        //{
        //    return new Vector3
        //    {
        //        x = m14,
        //        y = m24,
        //        z = m34
        //    };
        //}

        ///// <summary>
        ///// Gets the Rotation portion of a Transform Matrix44 (upper left).
        ///// </summary>
        ///// <returns>New Matrix33 with the rotation component.</returns>
        //public Matrix33 GetRotation()
        //{
        //    return new Matrix33()
        //    {
        //        m11 = this.m11,
        //        m12 = this.m12,
        //        m13 = this.m13,
        //        m21 = this.m21,
        //        m22 = this.m22,
        //        m23 = this.m23,
        //        m31 = this.m31,
        //        m32 = this.m32,
        //        m33 = this.m33,
        //    };
        //}

        //public Vector3 GetScale()
        //{
        //    return new Vector3
        //    {
        //        x = m41,
        //        y = m42,
        //        z = m43
        //    };
        //}

        //public Vector3 GetBoneTranslation()
        //{
        //    return new Vector3
        //    {
        //        x = m14,
        //        y = m24,
        //        z = m34
        //    };
        //}

        //public double[,] ConvertTo4x4Array()
        //{
        //    double[,] result = new double[4, 4];
        //    result[0, 0] = this.m11;
        //    result[0, 1] = this.m12;
        //    result[0, 2] = this.m13;
        //    result[0, 3] = this.m14;
        //    result[1, 0] = this.m21;
        //    result[1, 1] = this.m22;
        //    result[1, 2] = this.m23;
        //    result[1, 3] = this.m24;
        //    result[2, 0] = this.m31;
        //    result[2, 1] = this.m32;
        //    result[2, 2] = this.m33;
        //    result[2, 3] = this.m34;
        //    result[3, 0] = this.m41;
        //    result[3, 1] = this.m42;
        //    result[3, 2] = this.m43;
        //    result[3, 3] = this.m44;

        //    return result;
        //}

        //public Matrix44 Inverse()
        //{
        //    Matrix<double> matrix = Matrix<double>.Build.Dense(4, 4);
        //    matrix = ToMathMatrix().Inverse();
        //    return GetMatrix44(matrix);
        //}

        //public Matrix44 GetTransformFromParts(Vector3 localTranslation, Matrix33 localRotation)
        //{
        //    var defaultScale = new Vector3
        //    {
        //        x = 0.0f,
        //        y = 0.0f,
        //        z = 0.0f
        //    };
        //    return GetTransformFromParts(localTranslation, localRotation, defaultScale);
        //}

        //public Matrix44 GetTransformFromParts(Vector3 localTranslation, Matrix33 localRotation, Vector3 localScale)
        //{
        //    Matrix44 transform = new Matrix44
        //    {
        //        // For Node Chunks, the translation appears to be along the bottom of the matrix, and scale on right side.
        //        // Translation part
        //        m41 = localTranslation.x,
        //        m42 = localTranslation.y,
        //        m43 = localTranslation.z,
        //        // Rotation part.  Invert this matrix, which results in proper rotation in Blender.
        //        m11 = localRotation.m11,
        //        m12 = localRotation.m21,
        //        m13 = localRotation.m31,
        //        m21 = localRotation.m12,
        //        m22 = localRotation.m22,
        //        m23 = localRotation.m32,
        //        m31 = localRotation.m13,
        //        m32 = localRotation.m23,
        //        m33 = localRotation.m33,
        //        // Scale part
        //        m14 = localScale.x,
        //        m24 = localScale.y,
        //        m34 = localScale.z,
        //        // Set final row
        //        m44 = 1
        //    };
        //    return transform;
        //}

        //public static Matrix44 Identity()
        //{
        //    return new Matrix44()
        //    {
        //        m11 = 1,
        //        m12 = 0,
        //        m13 = 0,
        //        m14 = 0,
        //        m21 = 0,
        //        m22 = 1,
        //        m23 = 0,
        //        m24 = 0,
        //        m31 = 0,
        //        m32 = 0,
        //        m33 = 1,
        //        m34 = 0,
        //        m41 = 0,
        //        m42 = 0,
        //        m43 = 0,
        //        m44 = 1
        //    };
        //}

        //public static Matrix44 CreateDefaultRootNodeMatrix()
        //{
        //    return new Matrix44()
        //    {
        //        m11 = 1,
        //        m12 = 0,
        //        m13 = 0,
        //        m14 = 0,
        //        m21 = 0,
        //        m22 = 1,
        //        m23 = 0,
        //        m24 = 0,
        //        m31 = 0,
        //        m32 = 0,
        //        m33 = 1,
        //        m34 = 0,
        //        m41 = 1,
        //        m42 = 1,
        //        m43 = 1,
        //        m44 = 0
        //    };
        //}

        //public Matrix<double> ToMathMatrix()
        //{
        //    Matrix<double> result = Matrix<double>.Build.Dense(4, 4);
        //    result[0, 0] = this.m11;
        //    result[0, 1] = this.m12;
        //    result[0, 2] = this.m13;
        //    result[0, 3] = this.m14;
        //    result[1, 0] = this.m21;
        //    result[1, 1] = this.m22;
        //    result[1, 2] = this.m23;
        //    result[1, 3] = this.m24;
        //    result[2, 0] = this.m31;
        //    result[2, 1] = this.m32;
        //    result[2, 2] = this.m33;
        //    result[2, 3] = this.m34;
        //    result[3, 0] = this.m41;
        //    result[3, 1] = this.m42;
        //    result[3, 2] = this.m43;
        //    result[3, 3] = this.m44;
        //    return result;
        //}

        //public Matrix44 GetMatrix44(Matrix<double> matrix)
        //{
        //    Matrix44 result = new Matrix44
        //    {
        //        m11 = matrix[0, 0],
        //        m12 = matrix[0, 1],
        //        m13 = matrix[0, 2],
        //        m14 = matrix[0, 3],
        //        m21 = matrix[1, 0],
        //        m22 = matrix[1, 1],
        //        m23 = matrix[1, 2],
        //        m24 = matrix[1, 3],
        //        m31 = matrix[2, 0],
        //        m32 = matrix[2, 1],
        //        m33 = matrix[2, 2],
        //        m34 = matrix[2, 3],
        //        m41 = matrix[3, 0],
        //        m42 = matrix[3, 1],
        //        m43 = matrix[3, 2],
        //        m44 = matrix[3, 3],
        //    };
        //    return result;
        //}
    }
}
