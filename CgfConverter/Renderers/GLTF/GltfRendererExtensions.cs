using System;
using System.IO;
using System.Numerics;
using CgfConverter.Renderers.Gltf.Models;

namespace CgfConverter.Renderers.Gltf;

internal static class GltfRendererExtensions
{
    internal static int GetScalarCount(this GltfAccessorTypes t) => t switch
    {
        GltfAccessorTypes.Scalar => 1,
        GltfAccessorTypes.Vec2 => 2,
        GltfAccessorTypes.Vec3 => 3,
        GltfAccessorTypes.Vec4 => 4,
        GltfAccessorTypes.Mat2 => 4,
        GltfAccessorTypes.Mat3 => 9,
        GltfAccessorTypes.Mat4 => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(t)),
    };

    internal static int GetSize(this GltfAccessorComponentTypes t) => t switch
    {
        GltfAccessorComponentTypes.s8 => 1,
        GltfAccessorComponentTypes.u8 => 1,
        GltfAccessorComponentTypes.s16 => 2,
        GltfAccessorComponentTypes.u16 => 2,
        GltfAccessorComponentTypes.u32 => 4,
        GltfAccessorComponentTypes.f32 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(t)),
    };

    internal static void Write(this BinaryWriter bw, Vector3 v)
    {
        bw.Write(v.X);
        bw.Write(v.Y);
        bw.Write(v.Z);
    }

    internal static void Write(this BinaryWriter bw, Quaternion v)
    {
        bw.Write(v.X);
        bw.Write(v.Y);
        bw.Write(v.Z);
        bw.Write(v.W);
    }

    internal static void Write(this BinaryWriter bw, Tangent v)
    {
        bw.Write(v.x / 32767f);
        bw.Write(v.y / 32767f);
        bw.Write(v.z / 32767f);
        bw.Write(v.w / 32767f);
    }

    internal static void Write(this BinaryWriter bw, IRGBA v)
    {
        bw.Write(v.r / 255f);
        bw.Write(v.g / 255f);
        bw.Write(v.b / 255f);
        bw.Write(v.a / 255f);
    }

    internal static void Write(this BinaryWriter bw, UV v)
    {
        bw.Write(v.U);
        bw.Write(v.V);
    }

    internal static void Write(this BinaryWriter bw, Matrix4x4 v)
    {
        bw.Write(v.M11);
        bw.Write(v.M12);
        bw.Write(v.M13);
        bw.Write(v.M14);
        bw.Write(v.M21);
        bw.Write(v.M22);
        bw.Write(v.M23);
        bw.Write(v.M24);
        bw.Write(v.M31);
        bw.Write(v.M32);
        bw.Write(v.M33);
        bw.Write(v.M34);
        bw.Write(v.M41);
        bw.Write(v.M42);
        bw.Write(v.M43);
        bw.Write(v.M44);
    }
}