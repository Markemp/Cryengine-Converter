using System;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfAnimationChannelTarget
{
    /// <summary>
    /// The index of the node to animate. When undefined, the animated object **MAY** be defined by an extension.
    /// </summary>
    [JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
    public int? Node;

    /// <summary>
    /// The name of the node's TRS property to animate, or the `"weights"` of the Morph Targets it instantiates.
    /// </summary>
    /// <remarks>
    /// For the `"translation"` property, the values that are provided by the sampler are the translation along the
    /// X, Y, and Z axes.
    /// For the `"rotation"` property, the values are a quaternion in the order (x, y, z, w), where w is the scalar.
    /// For the `"scale"` property, the values are the scaling factors along the X, Y, and Z axes.
    /// </remarks>
    [JsonIgnore] public GltfAnimationChannelTargetPath Path;

    [JsonProperty("path")]
    public string PathString
    {
        get => Path switch
        {
            GltfAnimationChannelTargetPath.Translation => "translation",
            GltfAnimationChannelTargetPath.Rotation => "rotation",
            GltfAnimationChannelTargetPath.Scale => "scale",
            GltfAnimationChannelTargetPath.Weights => "weights",
            _ => throw new ArgumentOutOfRangeException(),
        };
        set => Path = value switch
        {
            "translation" => GltfAnimationChannelTargetPath.Translation,
            "rotation" => GltfAnimationChannelTargetPath.Rotation,
            "scale" => GltfAnimationChannelTargetPath.Scale,
            "weights" => GltfAnimationChannelTargetPath.Weights,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}