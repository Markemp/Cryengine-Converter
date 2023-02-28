using System;
using System.IO;
using System.Numerics;
using BCnEncoder.Shared;
using CgfConverter.Renderers.Gltf.Models;
using SixLabors.ImageSharp.PixelFormats;

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

    internal static Rgba32[] ToRgba32(this ColorRgba32[] raw)
    {
        var res = new Rgba32[raw.Length];
        for (var i = 0; i < raw.Length; i++)
        {
            res[i].R = raw[i].r;
            res[i].G = raw[i].g;
            res[i].B = raw[i].b;
            res[i].A = raw[i].a;
        }

        return res;
    }

    internal static Rgb24[] ToRgb24(this ColorRgba32[] raw)
    {
        var res = new Rgb24[raw.Length];
        for (var i = 0; i < raw.Length; i++)
        {
            res[i].R = raw[i].r;
            res[i].G = raw[i].g;
            res[i].B = raw[i].b;
        }

        return res;
    }
}