using Newtonsoft.Json;

namespace CgfConverter.Renderers.Gltf.Models;

public class GltfExtensions
{
    [JsonProperty("KHR_materials_specular", NullValueHandling = NullValueHandling.Ignore)]
    public GltfExtensionKhrMaterialsSpecular? KhrMaterialsSpecular;

    [JsonProperty("KHR_materials_pbrSpecularGlossiness", NullValueHandling = NullValueHandling.Ignore)]
    public GltfExtensionKhrMaterialsPbrSpecularGlossiness? KhrMaterialsPbrSpecularGlossiness;

    [JsonProperty("KHR_materials_emissive_strength", NullValueHandling = NullValueHandling.Ignore)]
    public GltfExtensionMaterialsEmissiveStrength? KhrMaterialsEmissiveStrength;

    [JsonProperty("MSFT_texture_dds", NullValueHandling = NullValueHandling.Ignore)]
    public GltfExtensionMsftTextureDds? MsftTextureDds;
}
