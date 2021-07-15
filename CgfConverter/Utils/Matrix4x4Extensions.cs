using CgfConverter.Structs;
using System.Numerics;

namespace Extensions
{
    public static class Matrix4x4Extensions    // Extensions for System.Numerics Matrix4x4
    {
        /// <summary>
        /// Gets the Rotation portion of a Transform Matrix4x4 (upper left).
        /// </summary>
        /// <returns>New Matrix33 with the rotation component.</returns>
        public static Matrix3x3 GetRotation(this Matrix4x4 matrix)
        {
            return new Matrix3x3()
            {
                M11 = matrix.M11,
                M12 = matrix.M12,
                M13 = matrix.M13,
                M21 = matrix.M21,
                M22 = matrix.M22,
                M23 = matrix.M23,
                M31 = matrix.M31,
                M32 = matrix.M32,
                M33 = matrix.M33
            };
        }

        public static Matrix4x4 CreateFromRotationMatrix(this Matrix4x4 m4, Matrix3x3 m3)
        {
            return new Matrix4x4()
            {
                M11 = m3.M11,
                M12 = m3.M12,
                M13 = m3.M13,
                M14 = 0,
                M21 = m3.M21,
                M22 = m3.M22,
                M23 = m3.M23,
                M24 = 0,
                M31 = m3.M31,
                M32 = m3.M32,
                M33 = m3.M33,
                M34 = 0,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };
        }

        public static Matrix4x4 CreateTransformFromParts(Vector3 translation, Matrix3x3 rotation)
        {
            return new Matrix4x4
            {
                M11 = rotation.M11,
                M12 = rotation.M12,
                M13 = rotation.M13,
                M14 = 0,
                M21 = rotation.M21,
                M22 = rotation.M22,
                M23 = rotation.M23,
                M24 = 0,
                M31 = rotation.M31,
                M32 = rotation.M32,
                M33 = rotation.M33,
                M34 = 0,
                M41 = translation.X,
                M42 = translation.Y,
                M43 = translation.Z,
                M44 = 1
            };
        }

        public static Matrix4x4 CreateDefaultRootNodeMatrix()
        {
            return new Matrix4x4()
            {
                M11 = 1.0f,
                M12 = 0.0f,
                M13 = 0.0f,
                M14 = 1.0f,
                M21 = 0.0f,
                M22 = 1.0f,
                M23 = 0.0f,
                M24 = 1.0f,
                M31 = 0.0f,
                M32 = 0.0f,
                M33 = 1.0f,
                M34 = 1.0f,
                M41 = 0.0f,
                M42 = 0.0f,
                M43 = 0.0f,
                M44 = 1.0f
            };
        }

        public static Vector3 GetScale(this Matrix4x4 matrix)
        {
            return new Vector3
            {
                X = matrix.M14,
                Y = matrix.M24,
                Z = matrix.M34
            };
        }

        public static Vector3 GetTranslation(this Matrix4x4 m)
        {
            return new Vector3
            {
                X = m.M14,
                Y = m.M24,
                Z = m.M34
            };
        }

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

    }
}
