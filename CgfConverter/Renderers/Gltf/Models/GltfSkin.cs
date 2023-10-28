using System.Collections.Generic;
using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfSkin
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    /// <summary>
    /// The index of the accessor containing the floating-point 4x4 inverse-bind matrices.
    /// </summary>
    /// <remarks>
    /// Its `accessor.count` property **MUST** be greater than or equal to the number of elements of the `joints`
    /// array. When undefined, each matrix is a 4x4 identity matrix.
    /// </remarks>
    [JsonProperty("inverseBindMatrices")]
    public int? InverseBindMatrices;

    [JsonProperty("joints")]
    public List<int> Joints = new();

    [JsonProperty("skeleton", NullValueHandling = NullValueHandling.Ignore)]
    public int? Skeleton;
}
