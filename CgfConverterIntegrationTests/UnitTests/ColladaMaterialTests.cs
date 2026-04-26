using CgfConverter;
using CgfConverter.Models.Materials;
using CgfConverter.PackFileSystem;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Effects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class ColladaMaterialTests
{
    private string tempDir = string.Empty;
    private ColladaModelRenderer renderer = null!;

    [TestInitialize]
    public void Initialize()
    {
        tempDir = Path.Combine(Path.GetTempPath(), "ColladaMaterialTests_" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        var args = new Args { NoTextures = false, OutputDir = tempDir };
        var packFs = new RealFileSystem(tempDir);
        var fakeInput = Path.Combine(tempDir, "fake.cgf");
        File.WriteAllBytes(fakeInput, []);

        var cryData = new CryEngine(fakeInput, packFs);
        renderer = new ColladaModelRenderer(args, cryData);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, recursive: true);
    }

    private static Material BuildSubMaterial(string name, params (string mapString, string file)[] textures)
    {
        var mat = new Material { Name = name };
        mat.Textures = new Texture[textures.Length];
        for (int i = 0; i < textures.Length; i++)
            mat.Textures[i] = new Texture { MapString = textures[i].mapString, File = textures[i].file };
        return mat;
    }

    private static string? GetPhongDiffuseTextureRef(ColladaEffect effect)
        => effect.Profile_COMMON?[0].Technique?.Phong?.Diffuse?.Texture?.Texture;

    private static string? GetPhongSpecularTextureRef(ColladaEffect effect)
        => effect.Profile_COMMON?[0].Technique?.Phong?.Specular?.Texture?.Texture;

    [TestMethod]
    public void CreateColladaEffect_TexSlot1Only_BindsTexSlot1AsDiffuse()
    {
        // Star Citizen #ivo materials: Map="TexSlot1" is the diffuse slot.
        // Without the fix, no diffuse texture is bound and Blender shows a flat color.
        var subMat = BuildSubMaterial("pom_decals",
            ("TexSlot1", "argo_pom_diff.dds"),
            ("Bumpmap", "argo_pom_ddna.dds"),
            ("TexSlot8", "argo_pom_displ.dds"));

        var effect = renderer.CreateColladaEffect("argo_mole_interior.mtl", subMat);

        Assert.IsNotNull(effect);
        var diffuseRef = GetPhongDiffuseTextureRef(effect);
        Assert.IsNotNull(diffuseRef, "Expected phong diffuse to be bound to a texture sampler when only TexSlot1 is present");
        StringAssert.EndsWith(diffuseRef, "_TexSlot1-sampler");
    }

    [TestMethod]
    public void CreateColladaEffect_DiffuseAndTexSlot1_PrefersExplicitDiffuse()
    {
        // Regression guard for materials like rubber_flooring that have BOTH a literal
        // Diffuse entry and a TexSlot1 entry. Explicit Diffuse must win.
        var subMat = BuildSubMaterial("rubber_flooring",
            ("TexSlot1", "rubber_worn_diff.dds"),
            ("Bumpmap", "rubber_worn_ddna.dds"),
            ("Diffuse", "rubber_grip_diff.dds"),
            ("TexSlot11", "rubber_grip_displ.dds"));

        var effect = renderer.CreateColladaEffect("argo_mole_interior.mtl", subMat);

        Assert.IsNotNull(effect);
        var diffuseRef = GetPhongDiffuseTextureRef(effect);
        Assert.IsNotNull(diffuseRef);
        StringAssert.EndsWith(diffuseRef, "_Diffuse-sampler",
            "Explicit Diffuse map should take precedence over TexSlot1");
    }

    [TestMethod]
    public void CreateColladaEffect_TexSlot10Only_BindsTexSlot10AsSpecular()
    {
        // TexSlot10 is the #ivo specular slot. TexSlot4 is already remapped to Specular at parse,
        // but TexSlot10 is not — verify the renderer falls back to it.
        var subMat = BuildSubMaterial("metal_dark",
            ("TexSlot1", "metal_dark_diff.dds"),
            ("TexSlot10", "metal_dark_spec.dds"));

        var effect = renderer.CreateColladaEffect("argo_mole_interior.mtl", subMat);

        Assert.IsNotNull(effect);
        var specularRef = GetPhongSpecularTextureRef(effect);
        Assert.IsNotNull(specularRef, "Expected phong specular to be bound when only TexSlot10 is present");
        StringAssert.EndsWith(specularRef, "_TexSlot10-sampler");
    }

    [TestMethod]
    public void CreateColladaEffect_NoDiffuseTexture_FallsBackToColor()
    {
        // Sanity: when no diffuse-eligible texture exists, phong falls back to a color, not a texture.
        var subMat = BuildSubMaterial("cables");
        subMat.Diffuse = "1 1 1";

        var effect = renderer.CreateColladaEffect("argo_mole_interior.mtl", subMat);

        Assert.IsNotNull(effect);
        var phong = effect.Profile_COMMON?[0].Technique?.Phong;
        Assert.IsNotNull(phong);
        Assert.IsNull(phong.Diffuse?.Texture, "Expected no diffuse texture when no eligible textures present");
        Assert.IsNotNull(phong.Diffuse?.Color, "Expected diffuse color fallback when no textures present");
    }
}
