using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfMeshPrimitiveAttributes
{
    /// <summary>
    /// Unitless XYZ vertex positions.
    /// </summary>
    [JsonProperty("POSITION", NullValueHandling = NullValueHandling.Ignore)]
    public int? Position;

    /// <summary>
    /// Normalized XYZ vertex normals.
    /// </summary>
    [JsonProperty("NORMAL", NullValueHandling = NullValueHandling.Ignore)]
    public int? Normal;

    /// <summary>
    /// ST(UV) texture coordinates.
    /// </summary>
    [JsonProperty("TEXCOORD_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? TexCoord0;

    /// <summary>
    /// Second UV channel texture coordinates.
    /// </summary>
    [JsonProperty("TEXCOORD_1", NullValueHandling = NullValueHandling.Ignore)]
    public int? TexCoord1;

    /// <summary>
    /// XYZW tangent vectors where XYZ is the tangent direction and W is the handedness (±1).
    /// </summary>
    [JsonProperty("TANGENT", NullValueHandling = NullValueHandling.Ignore)]
    public int? Tangent;

    /// <summary>
    /// RGB or RGBA vertex color linear multiplier.
    /// </summary>
    [JsonProperty("COLOR_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? Color0;

    /// <summary>
    /// Indices of the joints from the corresponding `skin.joints` array that affect the vertex.
    /// </summary>
    [JsonProperty("JOINTS_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? Joints0;

    /// <summary>
    /// Weights indicating how strongly the joint influences the vertex.
    /// </summary>
    [JsonProperty("WEIGHTS_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? Weights0;
}
