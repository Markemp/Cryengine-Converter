using System;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfAnimationSampler
{
    /// <summary>
    /// The index of an accessor containing keyframe timestamps.
    /// </summary>
    /// <remarks>
    /// The accessor **MUST** be of scalar type with floating-point components.
    /// The values represent time in seconds with `time[0] >= 0.0`, and strictly increasing values,
    /// i.e., `time[n + 1] > time[n]`.
    /// </remarks>
    [JsonProperty("input")] public int Input;

    /// <summary>
    /// The index of an accessor, containing keyframe output values.
    /// </summary>
    [JsonProperty("output")] public int Output;

    /// <summary>
    /// Interpolation algorithm.
    /// </summary>
    [JsonIgnore] public GltfAnimationSamplerInterpolation Interpolation;

    [JsonProperty("interpolation")]
    public string InterpolationString
    {
        get => Interpolation switch
        {
            GltfAnimationSamplerInterpolation.Linear => "LINEAR",
            GltfAnimationSamplerInterpolation.Step => "STEP",
            GltfAnimationSamplerInterpolation.CubicSpline => "CUBICSPLINE",
            _ => throw new ArgumentOutOfRangeException(),
        };
        set => Interpolation = value switch
        {
            "LINEAR" => GltfAnimationSamplerInterpolation.Linear,
            "STEP" => GltfAnimationSamplerInterpolation.Step,
            "CUBICSPLINE" => GltfAnimationSamplerInterpolation.CubicSpline,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}