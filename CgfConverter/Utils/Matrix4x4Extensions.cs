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

        public static Vector3 GetScale(this Matrix4x4 matrix)
        {
            return new Vector3
            {
                X = matrix.M41,
                Y = matrix.M42,
                Z = matrix.M43
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

        public static Matrix4x4 CreateFromMatrix3x4(Matrix3x4 matrix)
        {
            return new Matrix4x4()
            {
                M11 = matrix.M11,
                M12 = matrix.M12,
                M13 = matrix.M13,
                M14 = matrix.M14,
                M21 = matrix.M21,
                M22 = matrix.M22,
                M23 = matrix.M23,
                M24 = matrix.M24,
                M31 = matrix.M31,
                M32 = matrix.M32,
                M33 = matrix.M33,
                M34 = matrix.M34,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };
        }

        public static Matrix4x4 CreateFromRotationMatrix(Matrix3x3 m3)
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

        public static Matrix4x4 CreateLocalTransformFromB2W(Matrix4x4 parent, Matrix4x4 child)
        {
            var parentRot = parent.GetRotation();
            var parentTranslation = parent.GetTranslation();

            var childRot = child.GetRotation();
            var childTranslation = child.GetTranslation();

            var newRot = Matrix3x3.Transpose(parentRot) * childRot;
            var newTranslation = parent.GetRotation() * (childTranslation - parentTranslation);
            return CreateTransformFromParts(newTranslation, newRot);
        }

        public static Matrix4x4 CreateTransformFromParentAndChild(Matrix4x4 parent, Matrix4x4 child)
        {
            var parentRot = parent.GetRotation();
            var parentTranslation = parent.GetTranslation();

            var childRot = child.GetRotation();
            var childTranslation = child.GetTranslation();

            var newRot = Matrix3x3.Transpose(parentRot) * childRot;
            var newTranslation = parent.GetRotation() * (childTranslation - parentTranslation);
            return CreateTransformFromParts(newTranslation, newRot);
        }

        public static Matrix4x4 CreateTransformFromParts(Vector3 translation, Matrix3x3 rotation)
        {
            return new Matrix4x4
            {
                M11 = rotation.M11,
                M12 = rotation.M12,
                M13 = rotation.M13,
                M14 = translation.X,
                M21 = rotation.M21,
                M22 = rotation.M22,
                M23 = rotation.M23,
                M24 = translation.Y,
                M31 = rotation.M31,
                M32 = rotation.M32,
                M33 = rotation.M33,
                M34 = translation.Z,
                M41 = 0,
                M42 = 0,
                M43 = 0,
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
    }
}
