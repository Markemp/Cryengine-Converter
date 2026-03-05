using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada;
using CgfConverter.Renderers.Gltf;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Renderers.USD;
using CgfConverter.Renderers.USD.Models;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class ArcheAgeTests
{
    // Archeage objectdir is at the game directory level (d:\depot\archeage), not the objects subdirectory.
    // Material files reference paths relative to this root (e.g., game\objects\...).
    private readonly TestUtils testUtils = new();
    private readonly string objectDir = @"d:\depot\archeage";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
    }

    #region Shared Helpers

    private CryEngine ProcessCryEngineFile(string file, string? matFile = null, bool includeAnimations = false)
    {
        var args = new string[] { file, "-objectdir", objectDir };
        testUtils.argsHandler.ProcessArgs(args);

        var options = new CryEngineOptions(matFile, objectDir, includeAnimations);
        var cryData = new CryEngine(file, testUtils.argsHandler.Args.PackFileSystem, options);
        cryData.ProcessCryengineFiles();
        return cryData;
    }

    private ColladaDoc GenerateCollada(CryEngine cryData, bool validate = true)
    {
        var colladaRenderer = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaRenderer.GenerateDaeObject();

        if (validate)
            testUtils.ValidateColladaXml(colladaRenderer);

        return colladaRenderer.DaeObject;
    }

    private GltfRoot GenerateGltf(CryEngine cryData)
    {
        var gltfRenderer = new GltfModelRenderer(testUtils.argsHandler.Args, cryData);
        return gltfRenderer.GenerateGltfObject();
    }

    private string GenerateUsdString(CryEngine cryData)
    {
        var usdRenderer = new UsdRenderer(testUtils.argsHandler.Args, cryData);
        var usdDoc = usdRenderer.GenerateUsdObject();

        var serializer = new UsdSerializer();
        using var writer = new StringWriter();
        serializer.Serialize(usdDoc, writer);
        return writer.ToString();
    }

    #endregion

    #region Bird .chr

    [TestMethod]
    public void Bird_Collada_SkeletonAndMaterials()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\characters\animals\bird\bird_a.chr", "bird_a.mtl");
        var dae = GenerateCollada(cryData);

        Assert.AreEqual(2, dae.Library_Materials.Material.Length);

        // Controller check
        var controller = dae.Library_Controllers.Controller;
        var expectedBones = "Bone04 Bone05 Bone01 Bone06 Bone06(mirrored) Bone02 Bone07 Bone08 Bone07(mirrored) Bone08(mirrored)";
        Assert.IsTrue(controller[0].Skin.Source[0].Name_Array.Value_Pre_Parse.StartsWith(expectedBones));
        var expectedBpm = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 0 0 0.100000 0 1 0 0 0 0 1 0 0 0 0 1 -0 0 -1 -0 -0.999108 0.042216 0 -0.071707 0.042216 0.999108 0 0.018175 0 0 0 1 0.967510 -0.183784 -0.173634 0.007351 0.186228 0.982504 -0.002255 -0.008423 0.171010 -0.030154 0.984808 0.026689";
        Assert.IsTrue(controller[0].Skin.Source[1].Float_Array.Value_As_String.StartsWith(expectedBpm));
        Assert.AreEqual(160, controller[0].Skin.Source[1].Float_Array.Count);

        // Visual scene
        Assert.AreEqual("Scene", dae.Scene.Visual_Scene.Name);
        Assert.AreEqual("#Scene", dae.Scene.Visual_Scene.URL);
        Assert.AreEqual(1, dae.Library_Visual_Scene.Visual_Scene.Length);
        Assert.AreEqual("Scene", dae.Library_Visual_Scene.Visual_Scene[0].ID);
        Assert.AreEqual(2, dae.Library_Visual_Scene.Visual_Scene[0].Node.Length);

        // Armature root bone
        var node = dae.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("Bone04", node.ID);
        Assert.AreEqual("Bone04", node.sID);
        Assert.AreEqual("Bone04", node.Name);
        Assert.AreEqual("JOINT", node.Type.ToString());
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", node.Matrix[0].Value_As_String);

        var locatorBone = node.node[0];
        Assert.AreEqual("Bone05", locatorBone.ID);
        Assert.AreEqual("Bone05", locatorBone.Name);
        Assert.AreEqual("Bone05", locatorBone.sID);
        Assert.AreEqual("JOINT", locatorBone.Type.ToString());
        Assert.AreEqual("1 0 0 0.100000 0 1 0 0 0 0 1 0 0 0 0 1", locatorBone.Matrix[0].Value_As_String);
    }

    [TestMethod]
    public void Bird_Collada_MaterialAutoDiscovery()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\characters\animals\bird\bird_a.chr");
        var dae = GenerateCollada(cryData);

        Assert.AreEqual(2, dae.Library_Materials.Material.Length);
    }

    [TestMethod]
    public void Bird_Gltf_SkeletonAndMaterials()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\characters\animals\bird\bird_a.chr", "bird_a.mtl");
        var gltf = GenerateGltf(cryData);

        Assert.AreEqual(2, gltf.Materials.Count);
        Assert.AreEqual(1, gltf.Meshes.Count);

        // Skeleton: 10 bones (Bone04..Bone08 + mirrored variants)
        Assert.AreEqual(1, gltf.Skins.Count);
        Assert.AreEqual(10, gltf.Skins[0].Joints.Count);

        // Root bone
        Assert.AreEqual("Bone04", gltf.Nodes[0].Name);
    }

    [TestMethod]
    public void Bird_Usd_SkeletonAndMaterials()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\characters\animals\bird\bird_a.chr", "bird_a.mtl");
        var usdOutput = GenerateUsdString(cryData);

        Assert.IsTrue(usdOutput.Contains("def Skeleton \"Skeleton\""), "Should have Skeleton prim");
        Assert.IsTrue(usdOutput.Contains("Bone04"), "Should contain root bone Bone04");
        Assert.IsTrue(usdOutput.Contains("Bone05"), "Should contain child bone Bone05");
        Assert.IsTrue(usdOutput.Contains("point3f[] points"), "Should have geometry points");
        Assert.IsTrue(usdOutput.Contains("normal3f[] normals"), "Should have normals");
        Assert.IsTrue(usdOutput.Contains("primvars:skel:jointIndices"), "Should have joint indices");
        Assert.IsTrue(usdOutput.Contains("primvars:skel:jointWeights"), "Should have joint weights");
    }

    #endregion

    #region DrugBoy .chr (camera controller edge case)

    [TestMethod]
    public void DrugBoy_Collada_CameraControllerEdgeCase()
    {
        // Camera has controller id 0xffffffff just like parent Bip01 — verify it doesn't throw.
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\characters\people\drug_boy01\face\drug_boy01_face01\drug_boy01_face01.chr");
        GenerateCollada(cryData);
    }

    [TestMethod]
    public void DrugBoy_Gltf_CameraControllerEdgeCase()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\characters\people\drug_boy01\face\drug_boy01_face01\drug_boy01_face01.chr");
        GenerateGltf(cryData);
    }

    [TestMethod]
    public void DrugBoy_Usd_CameraControllerEdgeCase()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\characters\people\drug_boy01\face\drug_boy01_face01\drug_boy01_face01.chr");
        GenerateUsdString(cryData);
    }

    #endregion

    #region BasketMix .cga (multi-material)

    [TestMethod]
    public void BasketMix_Collada_MultiMaterial()
    {
        // 2 material files used in this model. The textures for the basket mtl file are missing.
        var cryData = ProcessCryEngineFile(
            @"D:\depot\archeage\game\objects\env\01_nuia\001_housing\01_tools\basket_mix_ani.cga",
            "basket_mix.mtl,tool_farm_d.mtl");
        var dae = GenerateCollada(cryData);

        // Material library
        Assert.AreEqual(5, dae.Library_Materials.Material.Length);
        Assert.AreEqual("basket_mix_mtl_basket_mix-material", dae.Library_Materials.Material[0].ID);
        Assert.AreEqual("#basket_mix_mtl_basket_mix-effect", dae.Library_Materials.Material[0].Instance_Effect.URL);

        // Image library
        Assert.AreEqual(7, dae.Library_Images.Image.Length);
        Assert.AreEqual("tool_farm_d_mtl_wood_Diffuse", dae.Library_Images.Image[3].Name);

        // Effect library
        Assert.AreEqual(5, dae.Library_Effects.Effect.Length);
        Assert.AreEqual("basket_mix_mtl_basket_mix-effect", dae.Library_Effects.Effect[0].ID);
        Assert.AreEqual("basket_mix_mtl_basket_mix_Diffuse-sampler", dae.Library_Effects.Effect[0].Profile_COMMON[0].Technique.Phong.Diffuse.Texture.Texture);
        Assert.AreEqual(6, dae.Library_Effects.Effect[0].Profile_COMMON[0].New_Param.Length);

        // Visual scene
        var rootNode = dae.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("basket_mix_ani", rootNode.Name);
        Assert.AreEqual("basket_mix_ani", rootNode.Instance_Geometry[0].Name);
        Assert.AreEqual("#basket_mix_ani-mesh", rootNode.Instance_Geometry[0].URL);
        Assert.AreEqual("#basket_mix_mtl_basket_mix-material", rootNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("#basket_mix_mtl_proxy-material", rootNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[1].Target);
    }

    [TestMethod]
    public void BasketMix_Collada_AutoDiscoverMaterials()
    {
        // Without an explicit mtl, both material files should still be auto-discovered.
        var cryData = ProcessCryEngineFile(
            @"D:\depot\archeage\game\objects\env\01_nuia\001_housing\01_tools\basket_mix_ani.cga");
        var dae = GenerateCollada(cryData);

        // Material library
        Assert.AreEqual(5, dae.Library_Materials.Material.Length);
        Assert.AreEqual("basket_mix_mtl_basket_mix-material", dae.Library_Materials.Material[0].ID);
        Assert.AreEqual("#basket_mix_mtl_basket_mix-effect", dae.Library_Materials.Material[0].Instance_Effect.URL);

        // Image library
        Assert.AreEqual(7, dae.Library_Images.Image.Length);
        Assert.AreEqual("tool_farm_d_mtl_wood_Diffuse", dae.Library_Images.Image[3].Name);

        // Effect library
        Assert.AreEqual(5, dae.Library_Effects.Effect.Length);
        Assert.AreEqual("basket_mix_mtl_basket_mix-effect", dae.Library_Effects.Effect[0].ID);
        Assert.AreEqual("basket_mix_mtl_basket_mix_Diffuse-sampler", dae.Library_Effects.Effect[0].Profile_COMMON[0].Technique.Phong.Diffuse.Texture.Texture);
        Assert.AreEqual(6, dae.Library_Effects.Effect[0].Profile_COMMON[0].New_Param.Length);

        // Visual scene
        var rootNode = dae.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("basket_mix_ani", rootNode.Name);
        Assert.AreEqual("basket_mix_ani", rootNode.Instance_Geometry[0].Name);
        Assert.AreEqual("#basket_mix_ani-mesh", rootNode.Instance_Geometry[0].URL);
        Assert.AreEqual("#basket_mix_mtl_basket_mix-material", rootNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("#basket_mix_mtl_proxy-material", rootNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[1].Target);
    }

    [TestMethod]
    public void BasketMix_Gltf_MultiMaterial()
    {
        var cryData = ProcessCryEngineFile(
            @"D:\depot\archeage\game\objects\env\01_nuia\001_housing\01_tools\basket_mix_ani.cga",
            "basket_mix.mtl,tool_farm_d.mtl");
        var gltf = GenerateGltf(cryData);

        Assert.AreEqual(5, gltf.Materials.Count);
        Assert.AreEqual(1, gltf.Meshes.Count);
        Assert.AreEqual("basket_mix_ani", gltf.Nodes[0].Name);
    }

    [TestMethod]
    public void BasketMix_Usd_MultiMaterial()
    {
        var cryData = ProcessCryEngineFile(
            @"D:\depot\archeage\game\objects\env\01_nuia\001_housing\01_tools\basket_mix_ani.cga",
            "basket_mix.mtl,tool_farm_d.mtl");
        var usdOutput = GenerateUsdString(cryData);

        Assert.IsTrue(usdOutput.Contains("basket_mix"), "Should contain basket_mix material");
        Assert.IsTrue(usdOutput.Contains("tool_farm_d"), "Should contain tool_farm_d material");
        Assert.IsTrue(usdOutput.Contains("point3f[] points"), "Should have geometry points");
        Assert.IsTrue(usdOutput.Contains("elementType = \"face\""), "GeomSubsets should use face element type");
    }

    #endregion

    #region Fishboat .chr (0x828 controller edge case)

    [TestMethod]
    public void Fishboat_Collada_SkeletonAndMaterials()
    {
        // fishboat.cal references CAF files with 0x828 controller chunks — verify no exception is thrown.
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\env\06_unit\01_ship\fishboat\fishboat.chr",
            "fishboat.mtl");
        var dae = GenerateCollada(cryData);

        Assert.IsNotNull(dae.Library_Visual_Scene);
        Assert.IsTrue(dae.Library_Visual_Scene.Visual_Scene[0].Node.Length > 0);
    }

    [TestMethod]
    public void Fishboat_Gltf_SkeletonAndMaterials()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\env\06_unit\01_ship\fishboat\fishboat.chr",
            "fishboat.mtl");
        var gltf = GenerateGltf(cryData);

        Assert.IsTrue(gltf.Meshes.Count > 0);
        Assert.AreEqual("Bip01", gltf.Nodes[0].Name);
    }

    [TestMethod]
    public void Fishboat_Usd_SkeletonAndMaterials()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\game\objects\env\06_unit\01_ship\fishboat\fishboat.chr",
            "fishboat.mtl");
        var usdOutput = GenerateUsdString(cryData);

        Assert.IsTrue(usdOutput.Contains("point3f[] points"), "Should have geometry points");
        Assert.IsTrue(usdOutput.Contains("fishboat"), "Should contain fishboat prim");
    }

    #endregion
}
