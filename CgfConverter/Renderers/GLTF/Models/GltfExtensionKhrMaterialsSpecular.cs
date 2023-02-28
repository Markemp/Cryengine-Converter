using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfExtensionKhrMaterialsSpecular
{
    [JsonProperty("specularColorTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfMaterialTextureSpecifier? SpecularColorTexture;
}