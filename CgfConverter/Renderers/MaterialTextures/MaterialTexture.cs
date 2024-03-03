using System;
using System.IO;
using BCnEncoder.Shared;
using Extensions;

namespace CgfConverter.Renderers.MaterialTextures;

public class MaterialTexture : IDisposable
{
    public MaterialTexture(IMaterialTextureKey key, ColorRgba32[] data, int width, int height, int numChannels)
    {
        Key = key;
        Width = width;
        Height = height;
        Data = data;
        NumChannels = numChannels;
    }

    public int NumChannels { get; }

    public IMaterialTextureKey Key { get; }

    public int Width { get; }

    public int Height { get; }

    public ColorRgba32[] Data { get; }

    public void Dispose()
    {
    }
}
