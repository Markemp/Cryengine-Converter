using System.Numerics;

namespace CgfConverter.Structs
{
    public class Matrix3x4
    {
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; }
        public float M14 { get; set; }
        public float M21 { get; set; }
        public float M22 { get; set; }
        public float M23 { get; set; }
        public float M24 { get; set; }
        public float M31 { get; set; }
        public float M32 { get; set; }
        public float M33 { get; set; }
        public float M34 { get; set; }

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
    }
}
