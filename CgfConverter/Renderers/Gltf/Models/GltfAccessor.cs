using System;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfAccessor
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("bufferView")] 
    public int BufferView;

    [JsonProperty("byteOffset")] 
    public long ByteOffset;

    [JsonProperty("componentType")] 
    public GltfAccessorComponentTypes ComponentType;

    [JsonProperty("count")] 
    public int Count;

    [JsonIgnore] 
    public GltfAccessorTypes Type;

    [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
    public object? Min;

    [JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
    public object? Max;

    [JsonProperty("type")]
    public string TypeString
    {
        get => Type switch
        {
            GltfAccessorTypes.Scalar => "SCALAR",
            GltfAccessorTypes.Vec2 => "VEC2",
            GltfAccessorTypes.Vec3 => "VEC3",
            GltfAccessorTypes.Vec4 => "VEC4",
            GltfAccessorTypes.Mat2 => "MAT2",
            GltfAccessorTypes.Mat3 => "MAT3",
            GltfAccessorTypes.Mat4 => "MAT4",
            _ => throw new ArgumentOutOfRangeException(nameof(Type)),
        };
        set => Type = value switch
        {
            "SCALAR" => GltfAccessorTypes.Scalar,
            "VEC2" => GltfAccessorTypes.Vec2,
            "VEC3" => GltfAccessorTypes.Vec3,
            "VEC4" => GltfAccessorTypes.Vec4,
            "MAT2" => GltfAccessorTypes.Mat2,
            "MAT3" => GltfAccessorTypes.Mat3,
            "MAT4" => GltfAccessorTypes.Mat4,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    public override string ToString() => 
        $"{Name}: {BufferView}, {ComponentType}, {TypeString}";
}