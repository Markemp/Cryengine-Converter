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
    public HashSet<string> ExtensionsUsed = new();

    [JsonProperty("scene")] 
    public int Scene;

    [JsonProperty("scenes")] 
    public List<GltfScene> Scenes = new();

    [JsonProperty("nodes")] 
    public List<GltfNode> Nodes = new();

    [JsonProperty("animations")] 
    public List<GltfAnimation> Animations = new();

    [JsonProperty("materials")] 
    public List<GltfMaterial> Materials = new();

    [JsonProperty("meshes")] 
    public List<GltfMesh> Meshes = new();

    [JsonProperty("textures")] 
    public List<GltfTexture> Textures = new();

    [JsonProperty("images")] 
    public List<GltfImage> Images = new();

    [JsonProperty("skins")] 
    public List<GltfSkin> Skins = new();

    [JsonProperty("accessors")] 
    public List<GltfAccessor> Accessors = new();

    [JsonProperty("bufferViews")] 
    public List<GltfBufferView> BufferViews = new();

    [JsonProperty("samplers")] 
    public List<GltfSampler> Samplers = new();

    [JsonProperty("buffers")] 
    public List<GltfBuffer> Buffers = new();

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