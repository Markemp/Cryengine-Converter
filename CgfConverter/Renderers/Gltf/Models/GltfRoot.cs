using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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

    public bool ShouldSerializeExtensionsUsed() => ExtensionsUsed.Any();
    public bool ShouldSerializeScenes() => Scenes.Any();
    public bool ShouldSerializeNodes() => Nodes.Any();
    public bool ShouldSerializeAnimations() => Animations.Any();
    public bool ShouldSerializeMaterials() => Materials.Any();
    public bool ShouldSerializeMeshes() => Meshes.Any();
    public bool ShouldSerializeTextures() => Textures.Any();
    public bool ShouldSerializeImages() => Images.Any();
    public bool ShouldSerializeSkins() => Skins.Any();
    public bool ShouldSerializeAccessors() => Accessors.Any();
    public bool ShouldSerializeBufferViews() => BufferViews.Any();
    public bool ShouldSerializeSamplers() => Samplers.Any();
    public bool ShouldSerializeBuffers() => Buffers.Any();
}
