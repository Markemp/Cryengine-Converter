using CgfConverter.Models.Materials;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Renderers.MaterialTextures;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    private class WrittenMaterial
    {
        private static readonly float[] Float4Transparent = new float[4];

        public const int SkippedFromArgsIndex = -1;

        public WrittenMaterial(
            Material source,
            BaseGltfRenderer renderer,
            MaterialTextureManager mtm,
            ArgsHandler argsHandler)
        {
            CryMaterial = source;
            if (argsHandler.IsMaterialExcluded(CryMaterial))
            {
                Index = SkippedFromArgsIndex;
                return;
            }

            // TODO: use GltfMaterialAlphaMode.Blend depending on shader type

            if ((CryMaterial.MaterialFlags & MaterialFlags.NoDraw) != 0)
            {
                GltfMaterial = new GltfMaterial
                {
                    Name = CryMaterial.Name,
                    AlphaCutoff = 1f, // Fully transparent
                    AlphaMode = GltfMaterialAlphaMode.Mask,
                    PbrMetallicRoughness = new GltfMaterialPbrMetallicRoughness
                    {
                        BaseColorFactor = Float4Transparent,
                    },
                };
                GltfMaterial.AlphaMode = GltfMaterialAlphaMode.Mask;
                GltfMaterial.AlphaCutoff = 1f; // Fully transparent
                GltfMaterial.PbrMetallicRoughness.BaseColorFactor = Float4Transparent;
                return;
            }

            MaterialTextureSet textures = mtm.CreateSet(CryMaterial);
            GltfMaterial = new GltfMaterial
            {
                Name = CryMaterial.Name,
                AlphaCutoff = CryMaterial.AlphaTest == 0 ? null : (float) CryMaterial.AlphaTest,
                AlphaMode = CryMaterial.AlphaTest == 0
                    ? GltfMaterialAlphaMode.Opaque
                    : GltfMaterialAlphaMode.Mask,
                DoubleSided = CryMaterial.MaterialFlags.HasFlag(MaterialFlags.TwoSided),
                NormalTexture = renderer.AddTextureInfo($"{CryMaterial.Name}-normal", textures.Normal),
                PbrMetallicRoughness = new GltfMaterialPbrMetallicRoughness
                {
                    BaseColorTexture = renderer.AddTextureInfo($"{CryMaterial.Name}-diffuse", textures.Diffuse),
                    BaseColorFactor = new[]
                    {
                        CryMaterial.DiffuseValue?.Red ?? 0f,
                        CryMaterial.DiffuseValue?.Green ?? 0f,
                        CryMaterial.DiffuseValue?.Blue ?? 0f,
                        float.Clamp(CryMaterial.OpacityValue ?? 1f, 0f, 1f),
                    },
                    MetallicFactor = float.Clamp(CryMaterial.PublicParams?.Metalness ?? 0, 0, 1),
                    RoughnessFactor = float.Clamp((255 - (float) CryMaterial.Shininess) / 255f, 0, 1),
                    MetallicRoughnessTexture = renderer.AddTextureInfo(
                        $"{CryMaterial.Name}-metallicroughness",
                        textures.MetallicRoughness),
                },
                Extensions = new GltfExtensions
                {
                    KhrMaterialsPbrSpecularGlossiness = new GltfExtensionKhrMaterialsPbrSpecularGlossiness
                    {
                        DiffuseFactor = new[]
                        {
                            CryMaterial.DiffuseValue?.Red ?? 0f,
                            CryMaterial.DiffuseValue?.Green ?? 0f,
                            CryMaterial.DiffuseValue?.Blue ?? 0f,
                            float.Clamp(CryMaterial.OpacityValue ?? 0f, 0f, 1f),
                        },
                        DiffuseTexture = renderer.AddTextureInfo($"{CryMaterial.Name}-diffuse", textures.Diffuse),
                        SpecularFactor = new[]
                        {
                            CryMaterial.SpecularValue?.Red ?? 0f,
                            CryMaterial.SpecularValue?.Green ?? 0f,
                            CryMaterial.SpecularValue?.Blue ?? 0f,
                        },
                        GlossinessFactor = float.Clamp((float) CryMaterial.Shininess / 255f, 0f, 1f),
                        SpecularGlossinessTexture = renderer.AddTextureInfo(
                            $"{CryMaterial.Name}-specularglossiness",
                            textures.SpecularGlossiness),
                    },
                    KhrMaterialsEmissiveStrength = CryMaterial.GlowAmount <= 0
                        ? null
                        : new GltfExtensionMaterialsEmissiveStrength
                        {
                            EmissiveStrength = (float) CryMaterial.GlowAmount,
                        },
                },
            };

            Index = renderer.AddMaterial(GltfMaterial);
        }

        public bool IsSkippedFromArgs => Index == SkippedFromArgsIndex;

        public int Index { get; }

        public Material CryMaterial { get; }

        public GltfMaterial? GltfMaterial { get; }
    }

    protected void WriteMaterial(string materialFile, Material material)
    {
        foreach (Material submat in material.SubMaterials!)
        {
            (string MaterialFile, string SubMaterialName) key = (materialFile, submat.Name!);
            if (_materialMap.ContainsKey(key))
                continue;
            _materialMap[key] = new WrittenMaterial(
                submat,
                this,
                _materialTextureManager,
                Args);
        }
    }
}
