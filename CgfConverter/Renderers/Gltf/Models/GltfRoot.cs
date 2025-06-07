using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CgfConverter.Renderers.Gltf.Models;

/// <summary>
/// Base object for a GLTF file. This gets serialized to the gltf/glb file.
/// </summary>
public class GltfRoot
{
    [JsonProperty("asset")]
    public GltfAsset Asset = new();

    [JsonProperty("extensionsUsed")]
    public HashSet<string> ExtensionsUsed = [];

    [JsonProperty("scene")]
    public int Scene;

    [JsonProperty("scenes")]
    public List<GltfScene> Scenes = [];

    [JsonProperty("nodes")]
    public List<GltfNode> Nodes = [];

    [JsonProperty("animations")]
    public List<GltfAnimation> Animations = [];

    [JsonProperty("materials")]
    public List<GltfMaterial> Materials = [];

    [JsonProperty("meshes")]
    public List<GltfMesh> Meshes = [];

    [JsonProperty("textures")]
    public List<GltfTexture> Textures = [];

    [JsonProperty("images")]
    public List<GltfImage> Images = [];

    [JsonProperty("skins")]
    public List<GltfSkin> Skins = [];

    [JsonProperty("accessors")]
    public List<GltfAccessor> Accessors = [];

    [JsonProperty("bufferViews")]
    public List<GltfBufferView> BufferViews = [];

    [JsonProperty("samplers")]
    public List<GltfSampler> Samplers = [];

    [JsonProperty("buffers")]
    public List<GltfBuffer> Buffers = [];

    public bool ShouldSerializeExtensionsUsed() => ExtensionsUsed.Count != 0;
    public bool ShouldSerializeScenes() => Scenes.Count != 0;
    public bool ShouldSerializeNodes() => Nodes.Count != 0;
    public bool ShouldSerializeAnimations() => Animations.Count != 0;
    public bool ShouldSerializeMaterials() => Materials.Count != 0;
    public bool ShouldSerializeMeshes() => Meshes.Count != 0;
    public bool ShouldSerializeTextures() => Textures.Count != 0;
    public bool ShouldSerializeImages() => Images.Count != 0;
    public bool ShouldSerializeSkins() => Skins.Count != 0;
    public bool ShouldSerializeAccessors() => Accessors.Count != 0;
    public bool ShouldSerializeBufferViews() => BufferViews.Count != 0;
    public bool ShouldSerializeSamplers() => Samplers.Count != 0;
    public bool ShouldSerializeBuffers() => Buffers.Count != 0;
}
