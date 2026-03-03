using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Gltf;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Renderers.USD;
using CgfConverter.Renderers.USD.Models;
using CgfConverterIntegrationTests.Extensions;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class MWOIntegrationTests
{
    private readonly TestUtils testUtils = new();
    private readonly string objectDir = @"d:\depot\mwo";

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

    private (string Output, UsdDoc Doc) GenerateUsd(CryEngine cryData)
    {
        var usdRenderer = new UsdRenderer(testUtils.argsHandler.Args, cryData);
        var usdDoc = usdRenderer.GenerateUsdObject();

        var serializer = new UsdSerializer();
        using var writer = new StringWriter();
        serializer.Serialize(usdDoc, writer);
        return (writer.ToString(), usdDoc);
    }

    private static UsdSkeleton? FindSkeleton(UsdPrim prim)
    {
        if (prim is UsdSkeleton skel)
            return skel;

        foreach (var child in prim.Children)
        {
            var found = FindSkeleton(child);
            if (found is not null)
                return found;
        }

        return null;
    }

    #endregion

    #region .cgf Static Geometry

    [TestMethod]
    public void Box_Collada_GeometryAndMaterials()
    {
        var cryData = ProcessCryEngineFile($@"{objectDir}\Objects\default\box.cgf");
        var dae = GenerateCollada(cryData);

        // Geometry
        Assert.AreEqual(1, dae.Library_Geometries.Geometry.Length);
        Assert.AreEqual("box-mesh", dae.Library_Geometries.Geometry[0].ID);
        Assert.AreEqual(4, dae.Library_Geometries.Geometry[0].Mesh.Source.Length);
        Assert.AreEqual(2, dae.Library_Geometries.Geometry[0].Mesh.Triangles.Length);
        Assert.AreEqual(12, dae.Library_Geometries.Geometry[0].Mesh.Triangles[0].Count);

        // Materials (resolved via model-name fallback: box.mtl loaded for library "helper")
        Assert.AreEqual(2, dae.Library_Materials.Material.Length);
        Assert.AreEqual("helper_mtl_Material_#25", dae.Library_Materials.Material[0].Name);

        // Visual scene node
        var boxNode = dae.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("box", boxNode.ID);
        Assert.AreEqual(ColladaNodeType.NODE, boxNode.Type);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0", boxNode.Matrix[0].Value_As_String);

        // Material binding
        Assert.AreEqual("#helper_mtl_Material_#25-material", boxNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("helper_mtl_Material_#25-material", boxNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
    }

    [TestMethod]
    public void Box_Gltf_GeometryAndMaterials()
    {
        var cryData = ProcessCryEngineFile($@"{objectDir}\Objects\default\box.cgf");
        var gltf = GenerateGltf(cryData);

        Assert.AreEqual(2, gltf.Materials.Count);
        Assert.AreEqual(1, gltf.Meshes.Count);
        Assert.AreEqual(1, gltf.Meshes[0].Primitives.Count); // Second subset has 0 indices/vertices
        Assert.AreEqual("box", gltf.Nodes[0].Name);
    }

    [TestMethod]
    public void Box_Usd_MaterialProperties()
    {
        var matFile = $@"{objectDir}\Objects\default\box.mtl";
        var cryData = ProcessCryEngineFile($@"{objectDir}\Objects\default\box.cgf", matFile);
        var usdOutput = GenerateUsdString(cryData);

        // Material shader properties
        Assert.IsTrue(usdOutput.Contains("inputs:diffuseColor"), "Should have diffuseColor");
        Assert.IsTrue(usdOutput.Contains("inputs:opacity"), "Should have opacity");
        Assert.IsTrue(usdOutput.Contains("inputs:roughness"), "Should have roughness");
        Assert.IsTrue(usdOutput.Contains("inputs:metallic"), "Should have metallic");

        // Color format uses parentheses
        Assert.IsTrue(usdOutput.Contains("inputs:diffuseColor = ("), "Color values should use parentheses");

        // Face-based GeomSubsets
        Assert.IsTrue(usdOutput.Contains("elementType = \"face\""), "GeomSubset should use face element type");

        // Material names cleaned
        Assert.IsTrue(usdOutput.Contains("Material__25"), "Material #25 should be cleaned");
        Assert.IsTrue(usdOutput.Contains("Material__26"), "Material #26 should be cleaned");
    }

    [TestMethod]
    public void WetLamp_Collada_MissingMaterialFile()
    {
        var cryData = ProcessCryEngineFile($@"{objectDir}\Objects\environments\frontend\mechlab_a\lights\industrial_wetlamp_a.cgf");
        var dae = GenerateCollada(cryData);

        Assert.IsTrue(dae.Library_Materials.Material.Length >= 1, "Should have at least 1 material");
    }

    [TestMethod]
    public void MechFactory_Crates_Gltf_ExplicitMtl()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\objects\environments\mech_factory\mf_crates\mechfactory_cratesa.cgf",
            "mf_crates.mtl");
        var gltf = GenerateGltf(cryData);

        Assert.AreEqual(1, gltf.Textures.Count);
        Assert.AreEqual(2, gltf.Materials.Count);
    }

    #endregion

    #region .cga Animated Geometry

    [TestMethod]
    public void AdrRightTorso_Collada_DefaultMaterials()
    {
        var cryData = ProcessCryEngineFile($@"{objectDir}\objects\mechs\adder\body\adr_right_torso_uac20_bh1.cga");
        var dae = GenerateCollada(cryData);

        // Default materials (no mtl file)
        Assert.AreEqual(0, dae.Library_Images.Image.Length);
        Assert.AreEqual(22, dae.Library_Materials.Material.Length);
        Assert.AreEqual("mechDefault_mtl_material0", dae.Library_Materials.Material[0].Name);

        // Geometry (multiple meshes from node hierarchy)
        Assert.IsTrue(dae.Library_Geometries.Geometry.Length >= 3, "Should have at least 3 geometries");

        // Root visual scene node
        var rootNode = dae.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("adr_right_torso_uac20_bh1", rootNode.Name);

        // Animation node has no geometry
        Assert.AreEqual("animation068", rootNode.node[0].Name);
        Assert.IsNull(rootNode.node[0].Instance_Geometry);
    }

    [TestMethod]
    public void AdrRightTorso_Collada_ProvidedMtlFile()
    {
        var matFile = $@"{objectDir}\Objects\mechs\adder\body\adder_body.mtl";
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\objects\mechs\adder\body\adr_right_torso_uac20_bh1.cga", matFile);
        var dae = GenerateCollada(cryData);

        Assert.AreEqual(28, dae.Library_Images.Image.Length);
        Assert.AreEqual(5, dae.Library_Materials.Material.Length);
        Assert.AreEqual("adder_body_mtl_adder_body", dae.Library_Materials.Material[0].Name);

        // Material binding on root node
        var rootNode = dae.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("adder_body_mtl_generic-material",
            rootNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
        Assert.AreEqual("#adder_body_mtl_generic-material",
            rootNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
    }

    [TestMethod]
    public void ClanBanner_Collada_MaterialAutoDiscovery()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\Objects\purchasable\cockpit_mounted\clanbanner\clanbanner_a_adder.cga");

        // Verify MtlName chunk metadata
        var mtlChunks = cryData.Chunks.Where(a => a.ChunkType == ChunkType.MtlName).ToList();
        Assert.AreEqual(1, (int)((ChunkMtlName)mtlChunks[0]).NumChildren);
        Assert.AreEqual(MtlNameType.Library, ((ChunkMtlName)mtlChunks[0]).MatType);

        var dae = GenerateCollada(cryData);

        Assert.AreEqual(1, dae.Library_Materials.Material.Length);
        Assert.AreEqual(1, dae.Library_Effects.Effect.Length);
        Assert.AreEqual(2, dae.Library_Images.Image.Length);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a", dae.Library_Materials.Material[0].Name);
    }

    [TestMethod]
    public void ClanBanner_Collada_ExplicitMtlFile()
    {
        var matFile = $@"{objectDir}\Objects\purchasable\cockpit_mounted\clanbanner\clanbanner_a.mtl";
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\Objects\purchasable\cockpit_mounted\clanbanner\clanbanner_a_adder.cga", matFile);
        var dae = GenerateCollada(cryData);

        // Should produce same results as auto-discovery
        Assert.AreEqual(1, dae.Library_Materials.Material.Length);
        Assert.AreEqual(1, dae.Library_Effects.Effect.Length);
        Assert.AreEqual(2, dae.Library_Images.Image.Length);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a", dae.Library_Materials.Material[0].Name);
    }

    [TestMethod]
    public void HulaGirl_Collada_ChildNodesAndTransforms()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\objects\purchasable\cockpit_standing\hulagirl\hulagirl__gold_a.cga");
        var dae = GenerateCollada(cryData);

        Assert.AreEqual(1, dae.Library_Materials.Material.Length);
        Assert.AreEqual(2, dae.Library_Geometries.Geometry.Length);

        // Root node has children but no geometry itself
        var rootNode = dae.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("hulagirl_a", rootNode.ID);
        Assert.AreEqual(2, rootNode.node.Length);
        Assert.IsNull(rootNode.Instance_Geometry);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0", rootNode.Matrix[0].Value_As_String);
    }

    [TestMethod]
    public void HulaGirl_Gltf_ChildNodesAndTransforms()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\Objects\purchasable\cockpit_standing\hulagirl\hulagirl__gold_a.cga");
        var gltf = GenerateGltf(cryData);

        Assert.AreEqual(1, gltf.Materials.Count);
        Assert.AreEqual(2, gltf.Meshes.Count);
        Assert.AreEqual(3, gltf.Nodes.Count);

        // Node names
        Assert.AreEqual("HulaGirl_UpperBody", gltf.Nodes[0].Name);
        Assert.AreEqual("HulaGirl_LowerBody", gltf.Nodes[1].Name);
        Assert.AreEqual("hulagirl_a", gltf.Nodes[2].Name);

        // Root children count
        Assert.AreEqual(2, gltf.Nodes[2].Children.Count);

        // Rotation/translation spot-checks on UpperBody
        AssertExtensions.AreEqual(
            [-0.10248467f, 0.00384537f, -0.04688235744833946f, 0.9936217f],
            gltf.Nodes[0].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual(
            [-0.000101296f, 0.0640777f, 0f],
            gltf.Nodes[0].Translation, TestUtils.delta);
    }

    [TestMethod]
    public void AtlasBodyPart_Collada_MultiSubmaterial()
    {
        var matFile = $@"{objectDir}\Objects\mechs\atlas\body\atlas_body.mtl";
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\Objects\mechs\atlas\body\as7_centre_torso.cga", matFile);
        var dae = GenerateCollada(cryData);

        Assert.AreEqual(5, dae.Library_Materials.Material.Length);
        Assert.AreEqual(5, dae.Library_Effects.Effect.Length);
        Assert.AreEqual(31, dae.Library_Images.Image.Length);
        Assert.AreEqual("atlas_body_mtl_atlas_body", dae.Library_Materials.Material[0].Name);
    }

    #endregion

    #region .chr Skeleton and Skinning

    [TestMethod]
    public void Adder_Collada_SkeletonAndAnimations()
    {
        var cryData = ProcessCryEngineFile($@"{objectDir}\objects\mechs\adder\body\adder.chr", includeAnimations: true);
        var dae = GenerateCollada(cryData);

        // Materials (default, no textures)
        Assert.AreEqual(11, dae.Library_Materials.Material.Length);
        Assert.AreEqual(0, dae.Library_Images.Image.Length);

        // Geometry
        Assert.AreEqual(1, dae.Library_Geometries.Geometry.Length);
        Assert.AreEqual("adder-mesh", dae.Library_Geometries.Geometry[0].ID);

        // Controller (skinning)
        Assert.AreEqual(1, dae.Library_Controllers.Controller.Length);
        var skin = dae.Library_Controllers.Controller[0].Skin;
        var jointsSource = skin.Source.First(s => s.ID == "Controller-joints");
        Assert.AreEqual(73, jointsSource.Name_Array.Count);
        Assert.AreEqual("Bip01", jointsSource.Name_Array.Value()[0]);

        // Bind poses
        var bindPosesSource = skin.Source.First(s => s.ID == "Controller-bind_poses");
        Assert.AreEqual(1168, bindPosesSource.Float_Array.Count); // 73 joints * 16 floats

        // Armature root node
        var rootNode = dae.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("Bip01", rootNode.ID);
        Assert.AreEqual(ColladaNodeType.JOINT, rootNode.Type);

        // Pelvis has 3 children (L_Hip, Pitch, R_Hip)
        var pelvisNode = rootNode.node[0];
        Assert.AreEqual("Bip01_Pelvis", pelvisNode.ID);
        Assert.AreEqual(3, pelvisNode.node.Length);
        Assert.IsNotNull(pelvisNode.node.FirstOrDefault(n => n.ID == "Bip01_L_Hip"));
        Assert.IsNotNull(pelvisNode.node.FirstOrDefault(n => n.ID == "Bip01_Pitch"));
        Assert.IsNotNull(pelvisNode.node.FirstOrDefault(n => n.ID == "Bip01_R_Hip"));

        // Geometry node uses Instance_Controller, not Instance_Geometry
        var geometryNode = dae.Library_Visual_Scene.Visual_Scene[0].Node[1];
        Assert.AreEqual("adder", geometryNode.ID);
        Assert.IsNull(geometryNode.Instance_Geometry);
        Assert.AreEqual(1, geometryNode.Instance_Controller.Length);
        Assert.AreEqual("#Controller", geometryNode.Instance_Controller[0].URL);

        // Animations loaded at data level (exported to separate files for Blender compatibility)
        Assert.IsTrue(cryData.Animations.Count > 0, "Should have animations from DBA files");
    }

    [TestMethod]
    public void Adder_Gltf_SkeletonAndAnimations()
    {
        var cryData = ProcessCryEngineFile($@"{objectDir}\objects\mechs\adder\body\adder.chr", includeAnimations: true);
        var gltf = GenerateGltf(cryData);

        Assert.AreEqual(11, gltf.Materials.Count);
        Assert.AreEqual(1, gltf.Meshes.Count);
        Assert.IsTrue(gltf.Nodes.Count >= 73, "Should have at least 73 bone nodes");

        // Root bone
        Assert.AreEqual("Bip01", gltf.Nodes[0].Name);
        AssertExtensions.AreEqual(
            [0.0f, 0.70710677f, 0.0f, 0.70710677f],
            gltf.Nodes[0].Rotation, 1e-5f);

        // Skin
        Assert.AreEqual(1, gltf.Skins.Count);
        Assert.AreEqual(73, gltf.Skins[0].Joints.Count);

        // Animations
        Assert.IsTrue(gltf.Animations.Count > 0, "Should have animations from DBA files");
    }

    [TestMethod]
    public void Adder_Usd_SkeletonAndAnimations()
    {
        var cryData = ProcessCryEngineFile($@"{objectDir}\objects\mechs\adder\body\adder.chr");
        var usdOutput = GenerateUsdString(cryData);

        // Root and skeleton prims
        Assert.IsTrue(usdOutput.Contains("def Xform \"adder\"") || usdOutput.Contains("def SkelRoot"), "Should have root prim");
        Assert.IsTrue(usdOutput.Contains("def Skeleton \"Skeleton\""), "Should have skeleton");

        // Key bone names
        Assert.IsTrue(usdOutput.Contains("Bip01"), "Should include Bip01");
        Assert.IsTrue(usdOutput.Contains("Bip01_Pelvis"), "Should include Bip01_Pelvis");

        // Geometry present
        Assert.IsTrue(usdOutput.Contains("point3f[] points"), "Should have geometry points");
        Assert.IsTrue(usdOutput.Contains("normal3f[] normals"), "Should have normals");
    }

    [TestMethod]
    public void FiftyCal_Collada_SmallSkeleton()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr");
        var dae = GenerateCollada(cryData);

        Assert.AreEqual(1, dae.Library_Materials.Material.Length);
        Assert.AreEqual(1, dae.Library_Effects.Effect.Length);
        Assert.AreEqual(4, dae.Library_Images.Image.Length);
    }

    [TestMethod]
    public void FiftyCal_Gltf_SmallSkeleton()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr");
        var gltf = GenerateGltf(cryData);

        Assert.AreEqual(1, gltf.Materials.Count);
        Assert.AreEqual(1, gltf.Meshes.Count);
        Assert.AreEqual(9, gltf.Nodes.Count);

        // Root bone
        Assert.AreEqual("Bip01", gltf.Nodes[0].Name);
        AssertExtensions.AreEqual(
            [-0.4963841f, -0.5035906f, 0.491474152f, 0.5083822f],
            gltf.Nodes[0].Rotation, TestUtils.delta);

        // Skin
        Assert.AreEqual(1, gltf.Skins.Count);
        Assert.AreEqual(8, gltf.Skins[0].Joints.Count);

        // Accessors
        Assert.AreEqual(7, gltf.Accessors.Count);
    }

    [TestMethod]
    public void Mechanic_Usd_SkeletonAndSkinning()
    {
        var cryData = ProcessCryEngineFile($@"{objectDir}\Objects\characters\mechanic\mechanic.chr");

        // Verify skinning info at CryEngine level
        Assert.IsNotNull(cryData.SkinningInfo, "SkinningInfo should not be null");
        Assert.IsTrue(cryData.SkinningInfo.HasSkinningInfo, "Should have skinning info");
        Assert.IsTrue(cryData.SkinningInfo.CompiledBones.Count > 0, "Should have bones");

        // Key bones exist
        var bip01 = cryData.SkinningInfo.CompiledBones.FirstOrDefault(b => b.BoneName == "Bip01");
        var pelvis = cryData.SkinningInfo.CompiledBones.FirstOrDefault(b => b.BoneName == "bip_01_Pelvis");
        Assert.IsNotNull(bip01, "Should find Bip01 bone");
        Assert.IsNotNull(pelvis, "Should find bip_01_Pelvis bone");

        // Pelvis world position
        Assert.AreEqual(0.950611, pelvis.WorldTransformMatrix.M34, 0.001, "Pelvis Z should be ~0.951");

        // Generate USD and verify structure
        var (usdOutput, usdDoc) = GenerateUsd(cryData);

        Assert.IsTrue(usdOutput.Contains("def SkelRoot \"Armature\""), "Should have SkelRoot");
        Assert.IsTrue(usdOutput.Contains("def Skeleton \"Skeleton\""), "Should have Skeleton");
        Assert.IsTrue(usdOutput.Contains("uniform token[] joints"), "Should have joints array");
        Assert.IsTrue(usdOutput.Contains("uniform matrix4d[] bindTransforms"), "Should have bindTransforms");
        Assert.IsTrue(usdOutput.Contains("uniform matrix4d[] restTransforms"), "Should have restTransforms");
        Assert.IsTrue(usdOutput.Contains("primvars:skel:jointIndices"), "Should have jointIndices");
        Assert.IsTrue(usdOutput.Contains("primvars:skel:jointWeights"), "Should have jointWeights");
        Assert.IsTrue(usdOutput.Contains("Bip01"), "Should contain Bip01");

        // Find Skeleton prim and verify restTransforms
        UsdSkeleton? skeleton = null;
        foreach (var prim in usdDoc.Prims)
        {
            skeleton = FindSkeleton(prim);
            if (skeleton is not null) break;
        }
        Assert.IsNotNull(skeleton, "Skeleton prim should be findable");

        var restTransforms = skeleton.Attributes.FirstOrDefault(a => a.Name == "restTransforms") as UsdMatrix4dArray;
        Assert.IsNotNull(restTransforms, "Should have restTransforms attribute");
        Assert.IsTrue(restTransforms.Matrices.Count >= 3, "Should have at least 3 bones");
    }

    #endregion

    #region Supplementary Renderer Coverage

    [TestMethod]
    public void HulaGirl_Usd_NodeHierarchy()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\Objects\purchasable\cockpit_standing\hulagirl\hulagirl__gold_a.cga");
        var usdOutput = GenerateUsdString(cryData);

        Assert.IsNotNull(usdOutput);
        Assert.IsTrue(usdOutput.Contains("hulagirl"), "Should contain hulagirl");
        Assert.IsTrue(usdOutput.Contains("HulaGirl_UpperBody"), "Should contain UpperBody node");
        Assert.IsTrue(usdOutput.Contains("HulaGirl_LowerBody"), "Should contain LowerBody node");
    }

    [TestMethod]
    public void AdrRightTorso_Usd_DefaultMaterials()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\objects\mechs\adder\body\adr_right_torso_uac20_bh1.cga");
        var usdOutput = GenerateUsdString(cryData);

        Assert.IsNotNull(usdOutput);
        Assert.IsTrue(usdOutput.Contains("mechDefault"), "Should have default materials");
        Assert.IsTrue(usdOutput.Contains("point3f[] points"), "Should have geometry points");
    }

    [TestMethod]
    public void FiftyCal_Usd_SmallSkeleton()
    {
        var cryData = ProcessCryEngineFile(
            $@"{objectDir}\objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr");
        var usdOutput = GenerateUsdString(cryData);

        Assert.IsTrue(usdOutput.Contains("Skeleton"), "Should contain Skeleton");
        Assert.IsTrue(usdOutput.Contains("Bip01"), "Should contain Bip01");
    }

    #endregion
}
