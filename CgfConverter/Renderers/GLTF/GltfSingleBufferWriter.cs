using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;
using CgfConverter.Renderers.Gltf.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CgfConverter.Renderers.Gltf;

public class GltfSingleBufferWriter
{
    private readonly List<byte[]> _bytesList = new();
    private readonly GltfRoot _root = new();

    private int _currentOffset;

    public GltfSingleBufferWriter()
    {
        _root.Buffers.Add(new GltfBuffer());
    }

    public int Scene
    {
        get => _root.Scene;
        set => _root.Scene = value;
    }

    public HashSet<string> ExtensionsUsed => _root.ExtensionsUsed;
    public List<GltfScene> Scenes => _root.Scenes;
    public List<GltfSampler> Samplers => _root.Samplers;
    public List<GltfNode> Nodes => _root.Nodes;
    public List<GltfSkin> Skins => _root.Skins;
    public List<GltfMesh> Meshes => _root.Meshes;
    public List<GltfMaterial> Materials => _root.Materials;

    public Tuple<byte[], GltfRoot> Compile()
    {
        var bin = new byte[_bytesList.Select(x => x.Length).Sum()];
        for (int i = 0, j = 0; i < _bytesList.Count; j += _bytesList[i].Length, i += 1)
            Buffer.BlockCopy(_bytesList[i], 0, bin, j, _bytesList[i].Length);
        _root.Buffers[0].ByteLength = bin.Length;
        return Tuple.Create(bin, _root);
    }

    public int Add(GltfScene scene)
    {
        _root.Scenes.Add(scene);
        return _root.Scenes.Count - 1;
    }

    public int Add(GltfSampler sampler)
    {
        _root.Samplers.Add(sampler);
        return _root.Samplers.Count - 1;
    }

    public int Add(GltfNode node)
    {
        _root.Nodes.Add(node);
        return _root.Nodes.Count - 1;
    }

    public int Add(GltfAnimation animation)
    {
        _root.Animations.Add(animation);
        return _root.Animations.Count - 1;
    }

    public int Add(GltfSkin skin)
    {
        _root.Skins.Add(skin);
        return _root.Skins.Count - 1;
    }

    public int Add(GltfMesh mesh)
    {
        _root.Meshes.Add(mesh);
        return _root.Meshes.Count - 1;
    }

    public int Add(GltfMaterial material)
    {
        _root.Materials.Add(material);
        return _root.Materials.Count - 1;
    }

    public unsafe int AddBufferView<T>(T[] data) where T : unmanaged
    {
        var rawSize = Marshal.SizeOf(data[0]) * data.Length;
        var paddedSize = (rawSize + 3) / 4 * 4;
        var target = new byte[paddedSize];

        fixed (void* src = data)
        fixed (void* dst = target)
            Buffer.MemoryCopy(src, dst, paddedSize, rawSize);

        _root.BufferViews.Add(new GltfBufferView
        {
            ByteOffset = _currentOffset,
            ByteLength = rawSize,
        });
        _bytesList.Add(target);
        _currentOffset += paddedSize;

        return _root.BufferViews.Count - 1;
    }

    public int AddBufferView(IRGBA[] data)
    {
        var buffer = new float[data.Length * 4];
        for (int i = 0, j = 0; i < buffer.Length; j++)
        {
            buffer[i++] = data[j].r;
            buffer[i++] = data[j].g;
            buffer[i++] = data[j].b;
            buffer[i++] = data[j].a;
        }

        return AddBufferView(buffer);
    }

    public int AddAccessor<T>(string name, int bufferView, T[] data, int start = 0, int end = int.MaxValue)
        where T : unmanaged
    {
        if (bufferView == -1)
            bufferView = AddBufferView(data);
        if (end == int.MaxValue)
            end = data.Length;

        GltfAccessorComponentTypes componentType;
        if (typeof(T) == typeof(byte))
            componentType = GltfAccessorComponentTypes.u8;
        else if (typeof(T) == typeof(sbyte))
            componentType = GltfAccessorComponentTypes.s8;
        else if (typeof(T) == typeof(ushort))
            componentType = GltfAccessorComponentTypes.u16;
        else if (typeof(T) == typeof(short))
            componentType = GltfAccessorComponentTypes.s16;
        else if (typeof(T) == typeof(uint))
            componentType = GltfAccessorComponentTypes.u32;
        else if (typeof(T) == typeof(float))
            componentType = GltfAccessorComponentTypes.f32;
        else
            throw new NotImplementedException();

        _root.Accessors.Add(new GltfAccessor
        {
            Name = name,
            ByteOffset = start * Marshal.SizeOf(data[0]),
            BufferView = bufferView,
            ComponentType = componentType,
            Count = end - start,
            Type = GltfAccessorTypes.Scalar,
            Min = new List<T> {data.Skip(start).Take(end - start).Min()},
            Max = new List<T> {data.Skip(start).Take(end - start).Max()},
        });
        return _root.Accessors.Count - 1;
    }

    public int AddAccessor<T>(string name, int bufferView, TypedVec4<T>[] data, int start = 0, int end = int.MaxValue)
        where T : unmanaged
    {
        if (bufferView == -1)
            bufferView = AddBufferView(data);
        if (end == int.MaxValue)
            end = data.Length;

        GltfAccessorComponentTypes componentType;
        if (typeof(T) == typeof(byte))
            componentType = GltfAccessorComponentTypes.u8;
        else if (typeof(T) == typeof(sbyte))
            componentType = GltfAccessorComponentTypes.s8;
        else if (typeof(T) == typeof(ushort))
            componentType = GltfAccessorComponentTypes.u16;
        else if (typeof(T) == typeof(short))
            componentType = GltfAccessorComponentTypes.s16;
        else if (typeof(T) == typeof(uint))
            componentType = GltfAccessorComponentTypes.u32;
        else if (typeof(T) == typeof(float))
            componentType = GltfAccessorComponentTypes.f32;
        else
            throw new NotImplementedException();

        _root.Accessors.Add(new GltfAccessor
        {
            Name = name,
            ByteOffset = start * Marshal.SizeOf(data[0]),
            BufferView = bufferView,
            ComponentType = componentType,
            Count = end - start,
            Type = GltfAccessorTypes.Vec4,
            Min = new List<T>
            {
                data.Skip(start).Take(end - start).Min(x => x.V1),
                data.Skip(start).Take(end - start).Min(x => x.V2),
                data.Skip(start).Take(end - start).Min(x => x.V3),
                data.Skip(start).Take(end - start).Min(x => x.V4),
            },
            Max = new List<T>
            {
                data.Skip(start).Take(end - start).Max(x => x.V1),
                data.Skip(start).Take(end - start).Max(x => x.V2),
                data.Skip(start).Take(end - start).Max(x => x.V3),
                data.Skip(start).Take(end - start).Max(x => x.V4),
            },
        });
        return _root.Accessors.Count - 1;
    }

    public int AddAccessor(string name, int bufferView, UV[] data, int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(data);
        if (end == int.MaxValue)
            end = data.Length;
        _root.Accessors.Add(new GltfAccessor
        {
            Name = name,
            ByteOffset = start * Marshal.SizeOf(data[0]),
            BufferView = bufferView,
            ComponentType = GltfAccessorComponentTypes.f32,
            Count = end - start,
            Type = GltfAccessorTypes.Vec2,
            Min = new List<float>
            {
                data.Skip(start).Take(end - start).Min(x => x.U),
                data.Skip(start).Take(end - start).Min(x => x.V),
            },
            Max = new List<float>
            {
                data.Skip(start).Take(end - start).Max(x => x.U),
                data.Skip(start).Take(end - start).Max(x => x.V),
            },
        });
        return _root.Accessors.Count - 1;
    }

    public int AddAccessor(string name, int bufferView, Vector3[] data, int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(data);
        if (end == int.MaxValue)
            end = data.Length;
        _root.Accessors.Add(new GltfAccessor
        {
            Name = name,
            ByteOffset = start * Marshal.SizeOf(data[0]),
            BufferView = bufferView,
            ComponentType = GltfAccessorComponentTypes.f32,
            Count = end - start,
            Type = GltfAccessorTypes.Vec3,
            Min = new List<float>
            {
                data.Skip(start).Take(end - start).Min(x => x.X),
                data.Skip(start).Take(end - start).Min(x => x.Y),
                data.Skip(start).Take(end - start).Min(x => x.Z),
            },
            Max = new List<float>
            {
                data.Skip(start).Take(end - start).Max(x => x.X),
                data.Skip(start).Take(end - start).Max(x => x.Y),
                data.Skip(start).Take(end - start).Max(x => x.Z),
            },
        });
        return _root.Accessors.Count - 1;
    }

    public int AddAccessor(string name, int bufferView, Quaternion[] data, int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(data);
        if (end == int.MaxValue)
            end = data.Length;
        _root.Accessors.Add(new GltfAccessor
        {
            Name = name,
            ByteOffset = start * Marshal.SizeOf(data[0]),
            BufferView = bufferView,
            ComponentType = GltfAccessorComponentTypes.f32,
            Count = end - start,
            Type = GltfAccessorTypes.Vec4,
            Min = new List<float>
            {
                data.Skip(start).Take(end - start).Min(x => x.X),
                data.Skip(start).Take(end - start).Min(x => x.Y),
                data.Skip(start).Take(end - start).Min(x => x.Z),
                data.Skip(start).Take(end - start).Min(x => x.W),
            },
            Max = new List<float>
            {
                data.Skip(start).Take(end - start).Max(x => x.X),
                data.Skip(start).Take(end - start).Max(x => x.Y),
                data.Skip(start).Take(end - start).Max(x => x.Z),
                data.Skip(start).Take(end - start).Max(x => x.W),
            },
        });
        return _root.Accessors.Count - 1;
    }

    public int AddAccessor(string name, int bufferView, IRGBA[] data, int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(data);
        if (end == int.MaxValue)
            end = data.Length;
        _root.Accessors.Add(new GltfAccessor
        {
            Name = name,
            ByteOffset = start * Marshal.SizeOf(data[0]),
            BufferView = bufferView,
            ComponentType = GltfAccessorComponentTypes.f32,
            Count = end - start,
            Type = GltfAccessorTypes.Vec4,
            Min = new List<float>
            {
                data.Skip(start).Take(end - start).Min(x => x.r),
                data.Skip(start).Take(end - start).Min(x => x.g),
                data.Skip(start).Take(end - start).Min(x => x.b),
                data.Skip(start).Take(end - start).Min(x => x.a),
            },
            Max = new List<float>
            {
                data.Skip(start).Take(end - start).Max(x => x.r),
                data.Skip(start).Take(end - start).Max(x => x.g),
                data.Skip(start).Take(end - start).Max(x => x.b),
                data.Skip(start).Take(end - start).Max(x => x.a),
            },
        });
        return _root.Accessors.Count - 1;
    }

    public int AddAccessor(string name, int bufferView, Matrix4x4[] data, int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(data);
        if (end == int.MaxValue)
            end = data.Length;
        _root.Accessors.Add(new GltfAccessor
        {
            Name = name,
            ByteOffset = start * Marshal.SizeOf(data[0]),
            BufferView = bufferView,
            ComponentType = GltfAccessorComponentTypes.f32,
            Count = end - start,
            Type = GltfAccessorTypes.Mat4,
            Min = new List<float>
            {
                data.Skip(start).Take(end - start).Min(x => x.M11),
                data.Skip(start).Take(end - start).Min(x => x.M12),
                data.Skip(start).Take(end - start).Min(x => x.M13),
                data.Skip(start).Take(end - start).Min(x => x.M14),
                data.Skip(start).Take(end - start).Min(x => x.M21),
                data.Skip(start).Take(end - start).Min(x => x.M22),
                data.Skip(start).Take(end - start).Min(x => x.M23),
                data.Skip(start).Take(end - start).Min(x => x.M24),
                data.Skip(start).Take(end - start).Min(x => x.M31),
                data.Skip(start).Take(end - start).Min(x => x.M32),
                data.Skip(start).Take(end - start).Min(x => x.M33),
                data.Skip(start).Take(end - start).Min(x => x.M34),
                data.Skip(start).Take(end - start).Min(x => x.M41),
                data.Skip(start).Take(end - start).Min(x => x.M42),
                data.Skip(start).Take(end - start).Min(x => x.M43),
                data.Skip(start).Take(end - start).Min(x => x.M44),
            },
            Max = new List<float>
            {
                data.Skip(start).Take(end - start).Max(x => x.M11),
                data.Skip(start).Take(end - start).Max(x => x.M12),
                data.Skip(start).Take(end - start).Max(x => x.M13),
                data.Skip(start).Take(end - start).Max(x => x.M14),
                data.Skip(start).Take(end - start).Max(x => x.M21),
                data.Skip(start).Take(end - start).Max(x => x.M22),
                data.Skip(start).Take(end - start).Max(x => x.M23),
                data.Skip(start).Take(end - start).Max(x => x.M24),
                data.Skip(start).Take(end - start).Max(x => x.M31),
                data.Skip(start).Take(end - start).Max(x => x.M32),
                data.Skip(start).Take(end - start).Max(x => x.M33),
                data.Skip(start).Take(end - start).Max(x => x.M34),
                data.Skip(start).Take(end - start).Max(x => x.M41),
                data.Skip(start).Take(end - start).Max(x => x.M42),
                data.Skip(start).Take(end - start).Max(x => x.M43),
                data.Skip(start).Take(end - start).Max(x => x.M44),
            },
        });
        return _root.Accessors.Count - 1;
    }

    public int AddTexture<TPixel>(string name, int width, int height, TPixel[] raw)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        using var ms = new MemoryStream();
        using (var image = Image.LoadPixelData(raw, width, height))
            image.SaveAsPng(ms);
        return AddTexture(name + ".png", "image/png", ms.ToArray());
    }

    public int AddTexture(string name, int width, int height, ColorRgba32[] raw, bool useAlpha)
    {
        using var ms = new MemoryStream();
        if (useAlpha)
        {
            var res = new Rgba32[raw.Length];
            for (var i = 0; i < raw.Length; i++)
            {
                res[i].R = raw[i].r;
                res[i].G = raw[i].g;
                res[i].B = raw[i].b;
                res[i].A = raw[i].a;
            }

            using var image = Image.LoadPixelData(res, width, height);
            image.SaveAsPng(ms);
        }
        else
        {
            var res = new Rgb24[raw.Length];
            for (var i = 0; i < raw.Length; i++)
            {
                res[i].R = raw[i].r;
                res[i].G = raw[i].g;
                res[i].B = raw[i].b;
            }

            using var image = Image.LoadPixelData(res, width, height);
            image.SaveAsPng(ms);
        }

        return AddTexture(name + ".png", "image/png", ms.ToArray());
    }

    public int AddTexture(string name, string mimeType, byte[] textureBytes)
    {
        var bufferViewIndex = AddBufferView(textureBytes);

        _root.Images.Add(new GltfImage
        {
            MimeType = mimeType,
            Name = name,
            BufferView = bufferViewIndex,
        });

        _root.Textures.Add(new GltfTexture
        {
            Name = name,
            Source = _root.Images.Count - 1,
        });

        return _root.Textures.Count - 1;
    }
}