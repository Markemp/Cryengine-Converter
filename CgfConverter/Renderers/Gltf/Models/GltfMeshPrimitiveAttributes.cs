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
    /// XYZW vertex tangents where the XYZ portion is normalized,
    /// and the W component is a sign value (-1 or +1) indicating handedness of the tangent basis.
    /// </summary>
    [JsonProperty("TANGENT", NullValueHandling = NullValueHandling.Ignore)]
    public int? Tangent;

    /// <summary>
    /// ST(UV) texture coordinates.
    /// </summary>
    [JsonProperty("TEXCOORD_0", NullValueHandling = NullValueHandling.Ignore)]
    public int? TexCoord0;

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