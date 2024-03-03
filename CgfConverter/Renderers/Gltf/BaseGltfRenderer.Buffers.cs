using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Shared;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Renderers.MaterialTextures;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    protected GltfScene CurrentScene => Root.Scenes[Root.Scene];

    private void Compile()
    {
        if (_bytesList.Count <= 1)
            return;

        var bin = new byte[_bytesList.Select(x => x.Length).Sum()];
        for (int i = 0, j = 0; i < _bytesList.Count; j += _bytesList[i].Length, i += 1)
            Buffer.BlockCopy(_bytesList[i], 0, bin, j, _bytesList[i].Length);
        _bytesList.Clear();
        _bytesList.Add(bin);
    }

    private void CompileToPair(string binFileName, Stream gltfStream, Stream binStream)
    {
        Compile();

        var bytes = _bytesList.FirstOrDefault() ?? Array.Empty<byte>();
        binStream.Write(bytes);

        Root.Buffers = new List<GltfBuffer>
        {
            new()
            {
                ByteLength = bytes.Length,
                Uri = binFileName,
            }
        };
        gltfStream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Root)));
    }

    private void CompileToBinary(Stream stream)
    {
        Compile();

        var bytes = _bytesList.FirstOrDefault() ?? Array.Empty<byte>();

        Root.Buffers = new List<GltfBuffer>
        {
            new()
            {
                ByteLength = bytes.Length,
            }
        };

        var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Root));
        if (json.Length % 4 != 0)
        {
            var rv = new byte[(json.Length + 3) / 4 * 4];
            Buffer.BlockCopy(json, 0, rv, 0, json.Length);
            for (var i = json.Length; i < rv.Length; i++)
                rv[i] = 0x20; // space
            json = rv;
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(0x46546C67);
        writer.Write(2);
        writer.Write(12 + 8 + json.Length + 8 + bytes.Length);
        writer.Write(json.Length);
        writer.Write(0x4E4F534A);
        writer.Write(json);

        writer.Write(bytes.Length);
        writer.Write(0x004E4942);
        writer.Write(bytes);
    }

    protected int AddNode(GltfNode node)
    {
        Root.Nodes.Add(node);
        return Root.Nodes.Count - 1;
    }

    private int AddAnimation(GltfAnimation animation)
    {
        Root.Animations.Add(animation);
        return Root.Animations.Count - 1;
    }

    private int AddSkin(GltfSkin skin)
    {
        Root.Skins.Add(skin);
        return Root.Skins.Count - 1;
    }

    private int AddMesh(GltfMesh mesh)
    {
        Root.Meshes.Add(mesh);
        return Root.Meshes.Count - 1;
    }

    private int AddMaterial(GltfMaterial material)
    {
        Root.Materials.Add(material);
        return Root.Materials.Count - 1;
    }

    private int? GetBufferViewOrDefault(string baseName)
    {
        var name = $"{baseName}/bufferView";
        var index = Root.Accessors.FindIndex(x => x.Name == name);
        return index == -1 ? null : index;
    }

    private unsafe int AddBufferView<T>(
        string? baseName,
        T[] data,
        GltfBufferViewTarget? bufferViewTarget)
        where T : unmanaged
    {
        var rawSize = Marshal.SizeOf(data[0]) * data.Length;
        var paddedSize = (rawSize + 3) / 4 * 4;
        var target = new byte[paddedSize];

        fixed (void* src = data)
        fixed (void* dst = target)
            Buffer.MemoryCopy(src, dst, paddedSize, rawSize);

        Root.BufferViews.Add(new GltfBufferView
        {
            Name = baseName is null ? null : $"{baseName}/bufferView",
            ByteOffset = _currentOffset,
            ByteLength = rawSize,
            Target = bufferViewTarget,
        });
        _bytesList.Add(target);
        _currentOffset += paddedSize;

        return Root.BufferViews.Count - 1;
    }

    private int? GetAccessorOrDefault(string baseName, int start, int end)
    {
        var name = $"{baseName}/accessor[{start}:{end}]";
        var index = Root.Accessors.FindIndex(x => x.Name == name);
        return index == -1 ? null : index;
    }

    private int AddAccessor<T>(
        string? baseName,
        int bufferView,
        GltfBufferViewTarget? bufferViewTarget,
        T[] data,
        int start = 0, int end = int.MaxValue)
        where T : unmanaged
    {
        if (bufferView == -1)
            bufferView = AddBufferView(baseName, data, bufferViewTarget);
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

        Root.Accessors.Add(new GltfAccessor
        {
            Name = baseName is null ? null : $"{baseName}/accessor[{start}:{end}]",
            ByteOffset = start * Marshal.SizeOf(data[0]),
            BufferView = bufferView,
            ComponentType = componentType,
            Count = end - start,
            Type = GltfAccessorTypes.Scalar,
            Min = new List<T> {data.Skip(start).Take(end - start).Min()},
            Max = new List<T> {data.Skip(start).Take(end - start).Max()},
        });
        return Root.Accessors.Count - 1;
    }

    private int AddAccessor<T>(
        string? baseName,
        int bufferView,
        GltfBufferViewTarget? bufferViewTarget,
        TypedVec4<T>[] data,
        int start = 0, int end = int.MaxValue)
        where T : unmanaged
    {
        if (bufferView == -1)
            bufferView = AddBufferView(baseName, data, bufferViewTarget);
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

        Root.Accessors.Add(new GltfAccessor
        {
            Name = baseName is null ? null : $"{baseName}/accessor[{start}:{end}]",
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
        return Root.Accessors.Count - 1;
    }

    private int AddAccessor(
        string? baseName,
        int bufferView,
        GltfBufferViewTarget? bufferViewTarget,
        UV[] data,
        int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(baseName, data, bufferViewTarget);

        if (end == int.MaxValue)
            end = data.Length;

        Root.Accessors.Add(new GltfAccessor
        {
            Name = baseName is null ? null : $"{baseName}/accessor[{start}:{end}]",
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
        return Root.Accessors.Count - 1;
    }

    private int AddAccessor(
        string? baseName,
        int bufferView,
        GltfBufferViewTarget? bufferViewTarget,
        Vector3[] data,
        int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(baseName, data, bufferViewTarget);
        if (end == int.MaxValue)
            end = data.Length;
        Root.Accessors.Add(new GltfAccessor
        {
            Name = baseName is null ? null : $"{baseName}/accessor[{start}:{end}]",
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
        return Root.Accessors.Count - 1;
    }

    private int AddAccessor(
        string? baseName,
        int bufferView,
        GltfBufferViewTarget? bufferViewTarget,
        Vector4[] data,
        int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(baseName, data, bufferViewTarget);
        if (end == int.MaxValue)
            end = data.Length;
        Root.Accessors.Add(new GltfAccessor
        {
            Name = baseName is null ? null : $"{baseName}/accessor[{start}:{end}]",
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
        return Root.Accessors.Count - 1;
    }

    private int AddAccessor(
        string? baseName,
        int bufferView,
        GltfBufferViewTarget? bufferViewTarget,
        Quaternion[] data,
        int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(baseName, data, bufferViewTarget);
        if (end == int.MaxValue)
            end = data.Length;
        Root.Accessors.Add(new GltfAccessor
        {
            Name = baseName is null ? null : $"{baseName}/accessor[{start}:{end}]",
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
        return Root.Accessors.Count - 1;
    }

    private int AddAccessor(
        string? baseName,
        int bufferView,
        GltfBufferViewTarget? bufferViewTarget,
        Matrix4x4[] data,
        int start = 0, int end = int.MaxValue)
    {
        if (bufferView == -1)
            bufferView = AddBufferView(baseName, data, bufferViewTarget);
        if (end == int.MaxValue)
            end = data.Length;

        Root.Accessors.Add(new GltfAccessor
        {
            Name = baseName is null ? null : $"{baseName}/accessor[{start}:{end}]",
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
        return Root.Accessors.Count - 1;
    }

    private enum SourceAlphaModes
    {
        Disable,
        Enable,
        Automatic,
    }

    private GltfTextureInfo? AddTextureInfo(string name, MaterialTexture? materialTexture)
    {
        if (materialTexture is null)
            return null;

        int index = Root.Textures.FindIndex(x => x.Name == $"{name}/texture");
        if (index == -1)
        {
            index = AddTexture(
                name,
                materialTexture.Width,
                materialTexture.Height,
                materialTexture.Data,
                materialTexture.NumChannels == 4 ? SourceAlphaModes.Enable : SourceAlphaModes.Disable);
        }

        return new GltfTextureInfo { Index = index };
    }

    private int AddTexture(string? baseName, int width, int height, ColorRgba32[] raw, SourceAlphaModes sourceAlphaMode,
        float? newOpacity = null)
    {
        if (sourceAlphaMode == SourceAlphaModes.Automatic)
        {
            sourceAlphaMode = GltfRendererUtilities.HasMeaningfulAlphaChannel(raw)
                ? SourceAlphaModes.Enable
                : SourceAlphaModes.Disable;
        }

        using var ms = new MemoryStream();
        if (newOpacity is not null)
        {
            var buf = raw.ToRgba32();
            if (sourceAlphaMode == SourceAlphaModes.Enable)
            {
                for (var i = 0; i < buf.Length; i++)
                    buf[i].A = (byte) (buf[i].A * newOpacity);
            }
            else
            {
                var newAlphaByte = (byte) (255 * newOpacity.Value);
                for (var i = 0; i < buf.Length; i++)
                    buf[i].A = newAlphaByte;
            }

            using var image = Image.LoadPixelData(buf, width, height);
            image.SaveAsPng(ms);
        }
        else
        {
            if (sourceAlphaMode == SourceAlphaModes.Enable)
            {
                using var image = Image.LoadPixelData(raw.ToRgba32(), width, height);
                image.SaveAsPng(ms);
            }
            else
            {
                using var image = Image.LoadPixelData(raw.ToRgb24(), width, height);
                image.SaveAsPng(ms);
            }
        }

        return AddTexture(baseName, "image/png", ms.ToArray());
    }

    private int AddTexture(string? baseName, string mimeType, byte[] textureBytes)
    {
        int? bufferViewIndex = null;
        string? uri = null;
        if (Args.EmbedTextures)
        {
            bufferViewIndex = AddBufferView(baseName, textureBytes, null);
        }
        else
        {
            baseName ??= $"file{_filesList.Count}";
            baseName = "mat_ " + string.Join("_", baseName.Split(Path.GetInvalidFileNameChars())).TrimEnd('.');
            uri = Path.ChangeExtension(baseName, ".png");
            _filesList[uri] = textureBytes;
        }

        Root.Images.Add(new GltfImage
        {
            Name = baseName is null ? null : $"{baseName}/image",
            MimeType = mimeType,
            BufferView = bufferViewIndex,
            Uri = uri,
        });

        Root.Textures.Add(new GltfTexture
        {
            Name = baseName is null ? null : $"{baseName}/texture",
            Source = Root.Images.Count - 1,
        });

        return Root.Textures.Count - 1;
    }
}
