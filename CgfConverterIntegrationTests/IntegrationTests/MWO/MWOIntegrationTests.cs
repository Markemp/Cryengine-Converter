using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada;
using CgfConverter.Renderers.Gltf;
using CgfConverterIntegrationTests.Extensions;
using CgfConverterTests.TestUtilities;
using Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class MWOIntegrationTests
{
    private readonly TestUtils testUtils = new();
    private readonly string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private readonly string objectDir = @"d:\depot\mwo\";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void ClanBanner_Adder_VerifyMaterials()
    {
        var matFile = @"D:\depot\mwo\Objects\purchasable\cockpit_mounted\clanbanner\clanbanner_a.mtl";
        var args = new string[] { $@"D:\depot\mwo\Objects\purchasable\cockpit_mounted\clanbanner\clanbanner_a_adder.cga", "-dds", "-dae", "-objectdir", objectDir, "-mtl", matFile };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, null, matFile);
        cryData.ProcessCryengineFiles();

        var mtlChunks = cryData.Chunks.Where(a => a.ChunkType == ChunkType.MtlName).ToList();
        Assert.AreEqual(1, (int)((ChunkMtlName)mtlChunks[0]).NumChildren);
        Assert.AreEqual(MtlNameType.Library, ((ChunkMtlName)mtlChunks[0]).MatType);
        Assert.AreEqual(MtlNameType.Child, ((ChunkMtlName)mtlChunks[1]).MatType);

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        Assert.AreEqual(1, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(1, daeObject.Library_Effects.Effect.Length);
        Assert.AreEqual(2, daeObject.Library_Images.Image.Length);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a", daeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a-material", daeObject.Library_Materials.Material[0].ID);
        Assert.AreEqual("#clanbanner_a_mtl_clanbanner_a-effect", daeObject.Library_Materials.Material[0].Instance_Effect.URL);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a_Diffuse", daeObject.Library_Images.Image[0].Name);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a_Diffuse", daeObject.Library_Images.Image[0].ID);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a_Normals", daeObject.Library_Images.Image[1].Name);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a_Normals", daeObject.Library_Images.Image[1].ID);
    }

    [TestMethod]
    public void ClanBanner_Adder_VerifyMaterialsWithNoMtlFileArg()
    {
        var args = new string[] { $@"D:\depot\mwo\Objects\purchasable\cockpit_mounted\clanbanner\clanbanner_a_adder.cga", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var mtlChunks = cryData.Chunks.Where(a => a.ChunkType == ChunkType.MtlName).ToList();
        Assert.AreEqual(1, (int)((ChunkMtlName)mtlChunks[0]).NumChildren);
        Assert.AreEqual(MtlNameType.Library, ((ChunkMtlName)mtlChunks[0]).MatType);
        Assert.AreEqual(MtlNameType.Child, ((ChunkMtlName)mtlChunks[1]).MatType);

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        Assert.AreEqual(1, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(1, daeObject.Library_Effects.Effect.Length);
        Assert.AreEqual(2, daeObject.Library_Images.Image.Length);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a", daeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a-material", daeObject.Library_Materials.Material[0].ID);
        Assert.AreEqual("#clanbanner_a_mtl_clanbanner_a-effect", daeObject.Library_Materials.Material[0].Instance_Effect.URL);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a_Diffuse", daeObject.Library_Images.Image[0].Name);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a_Diffuse", daeObject.Library_Images.Image[0].ID);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a_Normals", daeObject.Library_Images.Image[1].Name);
        Assert.AreEqual("clanbanner_a_mtl_clanbanner_a_Normals", daeObject.Library_Images.Image[1].ID);
    }

    [TestMethod]
    public void AtlasBodyPart_VerifyMaterials()
    {
        var matFile = @"D:\depot\mwo\Objects\mechs\atlas\body\atlas_body.mtl";
        var args = new string[] { $@"D:\depot\mwo\Objects\mechs\atlas\body\as7_centre_torso.cga", "-dds", "-dae", "-objectdir", objectDir, "-mtl", matFile };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, null, matFile);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        Assert.AreEqual(5, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(5, daeObject.Library_Effects.Effect.Length);
        Assert.AreEqual(31, daeObject.Library_Images.Image.Length);
        Assert.AreEqual("atlas_body_mtl_atlas_body", daeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual("atlas_body_mtl_atlas_body-material", daeObject.Library_Materials.Material[0].ID);
        Assert.AreEqual("#atlas_body_mtl_atlas_body-effect", daeObject.Library_Materials.Material[0].Instance_Effect.URL);
        Assert.AreEqual("atlas_body_mtl_atlas_body_Diffuse", daeObject.Library_Images.Image[0].Name);
        Assert.AreEqual("atlas_body_mtl_atlas_body_Diffuse", daeObject.Library_Images.Image[0].ID);
        Assert.AreEqual("atlas_body_mtl_atlas_body_Specular", daeObject.Library_Images.Image[1].Name);
        Assert.AreEqual("atlas_body_mtl_atlas_body_Specular", daeObject.Library_Images.Image[1].ID);
    }

    [TestMethod]
    public void Atlas_VerifyArmature()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\MWO\atlas.chr", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Atlas_VerifyArmature2()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\mechs\atlas\body\atlas.chr", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Adder_VerifyArmatureAndAnimations_Collada()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\mechs\adder\body\adder.chr", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Adder_VerifyArmatureAndAnimations_Gltf()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\mechs\adder\body\adder.chr", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
    }

    [TestMethod]
    public void HarnessCable_VerifyArmatureAndAnimations_Gltf()
    {
        var args = new string[] { @"D:\depot\MWO\Objects\environments\frontend\mechlab_a\mechbay_cables\harness_cable.chr", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();

    }

    [TestMethod]
    public void HarnessCable_VerifyArmatureAndAnimations_Collada()
    {
        var args = new string[] { @"D:\depot\MWO\Objects\environments\frontend\mechlab_a\mechbay_cables\harness_cable.chr",
            "-dds", "-dae",
            "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

    }

    [TestMethod]
    public void adr_right_torso_uac20_bh1()
    {
        // This model has 4 mtl files, all variations of mechdefault.  No mtl file provided, so it should create default materials only.
        var args = new string[] { $@"d:\depot\mwo\objects\mechs\adder\body\adr_right_torso_uac20_bh1.cga", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();
        var matNameChunks = cryData.Chunks.Where(c => c.ChunkType == ChunkType.MtlName).ToList();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        Assert.AreEqual(0, colladaData.DaeObject.Library_Images.Image.Length);
        Assert.AreEqual(22, colladaData.DaeObject.Library_Materials.Material.Length); // default materials
        Assert.AreEqual("mechDefault_mtl_material0", colladaData.DaeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual(0, colladaData.DaeObject.Library_Images.Image.Length);
        Assert.AreEqual("mechDefault_mtl_material0", colladaData.DaeObject.Library_Effects.Effect[0].Name);
        Assert.AreEqual("mechDefault_mtl_material0-effect", colladaData.DaeObject.Library_Effects.Effect[0].ID);

        var visualSceneLibrary = colladaData.DaeObject.Library_Visual_Scene.Visual_Scene[0];
        var imageLibrary = colladaData.DaeObject.Library_Images;

        Assert.AreEqual(0, colladaData.DaeObject.Library_Images.Image.Length);
        Assert.AreEqual("mechDefault_mtl_material0", colladaData.DaeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual("mechDefault_mtl_material0-material", colladaData.DaeObject.Library_Materials.Material[0].ID);
        Assert.AreEqual(22, colladaData.DaeObject.Library_Materials.Material.Length);
        // library_effects
        Assert.AreEqual("mechDefault_mtl_material0", colladaData.DaeObject.Library_Effects.Effect[0].Name);
        Assert.AreEqual("mechDefault_mtl_material1", colladaData.DaeObject.Library_Effects.Effect[1].Name);
        Assert.AreEqual("mechDefault_mtl_material0-effect", colladaData.DaeObject.Library_Effects.Effect[0].ID);

        // library_geometries
        Assert.AreEqual("mechDefault_mtl_material4-material", colladaData.DaeObject.Library_Geometries.Geometry[0].Mesh.Triangles[0].Material);
        Assert.AreEqual("05_-_Default_mtl_material4-material", colladaData.DaeObject.Library_Geometries.Geometry[1].Mesh.Triangles[0].Material);
        Assert.AreEqual("mechDefault_mtl_material2-material", colladaData.DaeObject.Library_Geometries.Geometry[2].Mesh.Triangles[0].Material);
        Assert.AreEqual("05_-_Default_mtl_material4-material", colladaData.DaeObject.Library_Geometries.Geometry[1].Mesh.Triangles[0].Material);
        Assert.AreEqual("mechDefault_mtl_material2-material", colladaData.DaeObject.Library_Geometries.Geometry[2].Mesh.Triangles[0].Material);

        // Verify visual scene material ids are set right
        // bh1
        Assert.AreEqual("adr_right_torso_uac20_bh1", visualSceneLibrary.Node[0].Name);
        Assert.AreEqual("mechDefault_mtl_material4-material", visualSceneLibrary.Node[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
        Assert.AreEqual("#mechDefault_mtl_material4-material", visualSceneLibrary.Node[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        // mount
        Assert.AreEqual("adr_right_torso_uac20_bh1_mount", visualSceneLibrary.Node[0].node[3].Name);
        Assert.AreEqual("mechDefault_mtl_material2-material", visualSceneLibrary.Node[0].node[3].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
        // barrel
        Assert.AreEqual("barrel013", visualSceneLibrary.Node[0].node[0].node[0].Name);
        Assert.AreEqual("05_-_Default_mtl_material4-material", visualSceneLibrary.Node[0].node[0].node[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
        // animations (no bind_mat)
        Assert.AreEqual("animation068", visualSceneLibrary.Node[0].node[0].Name);
        Assert.IsNull(visualSceneLibrary.Node[0].node[0].Instance_Geometry);
    }

    [TestMethod]
    public void adr_right_torso_uac20_bh1_ProvidedMtlFile()
    {
        // mtl file provided, so materials are created.
        var matFile = @"D:\depot\mwo\Objects\mechs\adder\body\adder_body.mtl";
        var args = new string[] { $@"d:\depot\mwo\objects\mechs\adder\body\adr_right_torso_uac20_bh1.cga", "-dds", "-dae", "-objectdir", objectDir, "-mtl", matFile };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, null, matFile);
        cryData.ProcessCryengineFiles();
        var matNameChunks = cryData.Chunks.Where(c => c.ChunkType == ChunkType.MtlName).ToList();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var visualSceneLibrary = colladaData.DaeObject.Library_Visual_Scene.Visual_Scene[0];
        var imageLibrary = colladaData.DaeObject.Library_Images;

        Assert.AreEqual(28, colladaData.DaeObject.Library_Images.Image.Length);
        Assert.AreEqual("adder_body_mtl_adder_body", colladaData.DaeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual("adder_body_mtl_adder_body-material", colladaData.DaeObject.Library_Materials.Material[0].ID);
        Assert.AreEqual(5, colladaData.DaeObject.Library_Materials.Material.Length);
        // library_effects
        Assert.AreEqual("adder_body_mtl_adder_body", colladaData.DaeObject.Library_Effects.Effect[0].Name);
        Assert.AreEqual("adder_body_mtl_decals", colladaData.DaeObject.Library_Effects.Effect[1].Name);
        Assert.AreEqual("adder_body_mtl_adder_body-effect", colladaData.DaeObject.Library_Effects.Effect[0].ID);

        // library_geometries
        Assert.AreEqual("adder_body_mtl_generic-material", colladaData.DaeObject.Library_Geometries.Geometry[0].Mesh.Triangles[0].Material);
        Assert.AreEqual("adder_body_mtl_generic-material", colladaData.DaeObject.Library_Geometries.Geometry[1].Mesh.Triangles[0].Material);
        Assert.AreEqual("adder_body_mtl_adder_variant-material", colladaData.DaeObject.Library_Geometries.Geometry[2].Mesh.Triangles[0].Material);
        Assert.AreEqual("adder_body_mtl_generic-material", colladaData.DaeObject.Library_Geometries.Geometry[1].Mesh.Triangles[0].Material);
        Assert.AreEqual("adder_body_mtl_adder_variant-material", colladaData.DaeObject.Library_Geometries.Geometry[2].Mesh.Triangles[0].Material);

        // Verify visual scene material ids are set right
        // bh1
        Assert.AreEqual("adr_right_torso_uac20_bh1", visualSceneLibrary.Node[0].Name);
        Assert.AreEqual("adder_body_mtl_generic-material", visualSceneLibrary.Node[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
        Assert.AreEqual("#adder_body_mtl_generic-material", visualSceneLibrary.Node[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("adder_body_mtl_generic_Diffuse", imageLibrary.Image[19].ID);
        // mount
        Assert.AreEqual("adr_right_torso_uac20_bh1_mount", visualSceneLibrary.Node[0].node[3].Name);
        Assert.AreEqual("adder_body_mtl_adder_variant-material", visualSceneLibrary.Node[0].node[3].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
        // barrel
        Assert.AreEqual("barrel013", visualSceneLibrary.Node[0].node[0].node[0].Name);
        Assert.AreEqual("adder_body_mtl_generic-material", visualSceneLibrary.Node[0].node[0].node[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
        // animations (no bind_mat)
        Assert.AreEqual("animation068", visualSceneLibrary.Node[0].node[0].Name);
        Assert.IsNull(visualSceneLibrary.Node[0].node[0].Instance_Geometry);
    }

    [TestMethod]
    public void FiftyCalNecklace_ColladaVerifyMaterials()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr", "-dds", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var daeObject = colladaData.DaeObject;
        Assert.AreEqual(1, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(1, daeObject.Library_Effects.Effect.Length);
        Assert.AreEqual(4, daeObject.Library_Images.Image.Length);
    }

    [TestMethod]
    public void FiftyCalNecklace_GltfConversion()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr", "-dds", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();

        Assert.AreEqual(1, gltfData.Materials.Count);
        Assert.AreEqual(1, gltfData.Meshes.Count);

        // Nodes check
        Assert.AreEqual(9, gltfData.Nodes.Count);
        Assert.AreEqual("Bip01", gltfData.Nodes[0].Name);
        Assert.AreEqual("hang seg1", gltfData.Nodes[1].Name);
        Assert.AreEqual("hang seg2", gltfData.Nodes[2].Name);

        //AssertExtensions.AreEqual([0, 0, 0, 1], gltfData.Nodes[0].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual([-0.4963841f, -0.5035906f, 0.491474152f, 0.5083822f], gltfData.Nodes[0].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual([0.00154118659f, -0.008913527f, 0.0122360326f, 0.9998842f], gltfData.Nodes[1].Rotation, TestUtils.delta);

        //AssertExtensions.AreEqual([0, 0, 0], gltfData.Nodes[1].Translation, TestUtils.delta);
        AssertExtensions.AreEqual([2.09588125E-13f, 0.0204448365f, -8.731578E-10f], gltfData.Nodes[0].Translation, TestUtils.delta);
        AssertExtensions.AreEqual([-0.0209655762f, -8.90577E-09f, 3.4356154E-10f], gltfData.Nodes[1].Translation, TestUtils.delta);

        Assert.AreEqual(1, gltfData.Nodes[0].Children.Count);
        Assert.AreEqual(1, gltfData.Nodes[1].Children.Count);
        Assert.AreEqual(1, gltfData.Nodes[2].Children.Count);

        // Accessors check
        Assert.AreEqual(8, gltfData.Accessors.Count);

        // Skins check
        Assert.AreEqual(1, gltfData.Skins.Count);
        Assert.AreEqual(8, gltfData.Skins[0].Joints.Count);
        Assert.AreEqual(7, gltfData.Skins[0].InverseBindMatrices);
        Assert.AreEqual("50calnecklace_a/skin", gltfData.Skins[0].Name);
    }

    [TestMethod]
    public void MwoFile_MtlFileWithNoDirInfo_OutputsToCorrectDir()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr", "-dds", "-objectdir", objectDir, "-mtl", "50calnecklace_a.mtl" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, materialFiles: args[5]);
        cryData.ProcessCryengineFiles();
        Assert.AreEqual(@"50calnecklace_a.mtl", cryData.MaterialFiles.First());
        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        Assert.AreEqual(1, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(1, daeObject.Library_Effects.Effect.Length);
        Assert.AreEqual(4, daeObject.Library_Images.Image.Length);
    }

    [TestMethod]
    public void MwoFile_MtlFileWithExtraSlash_OutputsToCorrectDir()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr", "-dds", "-objectdir", objectDir, "-mtl", "/50calnecklace_a.mtl" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, materialFiles: args[5]);
        cryData.ProcessCryengineFiles();
        Assert.AreEqual(@"/50calnecklace_a.mtl", cryData.MaterialFiles.First());
        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        Assert.AreEqual(1, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(1, daeObject.Library_Effects.Effect.Length);
        Assert.AreEqual(4, daeObject.Library_Images.Image.Length);
    }

    [TestMethod]
    public void MwoFile_MtlFileWithCurrentDir_OutputsToCorrectDir()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr", "-dds", "-objectdir", objectDir, "-mtl", "./50calnecklace_a.mtl" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, materialFiles: args[5]);
        cryData.ProcessCryengineFiles();
        Assert.AreEqual(@"./50calnecklace_a.mtl", cryData.MaterialFiles.First());
        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        Assert.AreEqual(1, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(1, daeObject.Library_Effects.Effect.Length);
        Assert.AreEqual(4, daeObject.Library_Images.Image.Length);
    }

    [TestMethod]
    public void Uav_VerifyMaterials()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\MWO\uav.cga", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);

    }

    [TestMethod]
    public void MWO_industrial_wetlamp_a_MaterialFileNotFound()
    {
        var args = new string[] { $@"D:\depot\MWO\Objects\environments\frontend\mechlab_a\lights\industrial_wetlamp_a.cgf", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void MWO_timberwolf_chr()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\timberwolf.chr", "-dds", "-dae", "-objectdir", @"d:\depot\lol\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();

        // Visual Scene Check
        Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
        Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
        Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
        Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

        // Armature Node check
        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("Bip01", node.ID);
        Assert.AreEqual("Bip01", node.sID);
        Assert.AreEqual("Bip01", node.Name);
        Assert.AreEqual("JOINT", node.Type.ToString());
        Assert.AreEqual("-0 1 0 0 -1 -0 0 0 0 0 1 -0 0 0 0 1", node.Matrix[0].Value_As_String);
        var pelvisNode = node.node[0];
        Assert.AreEqual("Bip01_Pelvis", pelvisNode.ID);
        Assert.AreEqual("Bip01_Pelvis", pelvisNode.Name);
        Assert.AreEqual("Bip01_Pelvis", pelvisNode.sID);
        Assert.AreEqual("JOINT", pelvisNode.Type.ToString());
        Assert.AreEqual("0 0 1 -8.346858 0 1 -0 0.000002 -1 0 0 -0.000001 0 0 0 1", pelvisNode.Matrix[0].Value_As_String);
        Assert.AreEqual(3, pelvisNode.node.Length);
        var pitchNode = pelvisNode.node.Where(a => a.ID == "Bip01_Pitch").FirstOrDefault();
        var leftHipNode = pelvisNode.node.Where(a => a.ID == "Bip01_L_Hip").FirstOrDefault();
        var rightHipNode = pelvisNode.node.Where(a => a.ID == "Bip01_R_Hip").FirstOrDefault();
        Assert.IsNotNull(pitchNode);
        Assert.AreEqual("Bip01_Pitch", pitchNode.sID);
        Assert.AreEqual("-1 -0 0 -1.627027 0 -0.022216 0.999753 -8.344796 -0 0.999753 0.022216 -0.185437 0 0 0 1", leftHipNode.Matrix[0].Value_As_String);
        Assert.AreEqual("1 -0 -0 -1.627022 -0 0.022216 -0.999753 8.344796 0 0.999753 0.022216 -0.185438 0 0 0 1", rightHipNode.Matrix[0].Value_As_String);
        Assert.AreEqual("0 -0.891905 0.452222 -4.127187 0.009909 0.452200 0.891861 -8.139535 -0.999951 0.004481 0.008838 -0.080661 0 0 0 1", pitchNode.Matrix[0].Value_As_String);

        // Geometry Node check
        node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[1];
        Assert.AreEqual("timberwolf", node.ID);
        Assert.AreEqual("timberwolf", node.Name);
        Assert.AreEqual("NODE", node.Type.ToString());
        Assert.IsNull(node.Instance_Geometry);
        Assert.AreEqual(1, node.Instance_Controller.Length);
        Assert.AreEqual("#Bip01", node.Instance_Controller[0].Skeleton[0].Value);

        // Controller check
        var controller = daeObject.Library_Controllers.Controller[0];
        Assert.AreEqual("Controller", controller.ID);
        var skin = controller.Skin;
        Assert.AreEqual("#timberwolf-mesh", skin.source);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", skin.Bind_Shape_Matrix.Value_As_String);
        Assert.AreEqual(3, skin.Source.Length);
        var controllerJoints = skin.Source.Where(a => a.ID == "Controller-joints").First();
        var controllerBindPose = skin.Source.Where(a => a.ID == "Controller-bind_poses").First();
        var controllerWeights = skin.Source.Where(a => a.ID == "Controller-weights").First();
        var joints = skin.Joints;
        Assert.AreEqual(64, controllerJoints.Name_Array.Count);
        Assert.AreEqual("Controller-joints-array", controllerJoints.Name_Array.ID);
        var nameArray = controllerJoints.Name_Array.Value();
        Assert.AreEqual(64, nameArray.Length);
        Assert.IsTrue(nameArray.Contains("Bip01"));
        Assert.IsTrue(nameArray.Contains("Bip01_L_Thigh"));
        Assert.IsTrue(nameArray.Contains("Bip01_R_Toe0Nub"));

        var bindPoseArray = "-0 1 0 0 -1 -0 0 0 0 0 1 -0 0 0 0 1 0 0 1 -8.346858 0 1 -0 0.000002 -1 0 0 -0.000001 0 0 0 1 -1 -0 0 -1.627027 0 -0.022216 0.999753 -8.344796 -0 0.999753 0.022216 -0.185437 0 0 0 1 0 -0.891905 0.452222 -4.127187 0.009909 0.452200 0.891861 -8.139535 -0.999951 0.004481 0.008838 -0.080661 0 0 0 1 1 -0 -0 -1.627022 -0 0.022216 -0.999753 8.344796 0 0.999753 0.022216 -0.185438 0 0 0 1 -0.003118 -0.652256 -0.757992 6.317856 0.003048 0.757986 -0.652263 5.453145 0.999990 -0.004344 -0.000376 2.888252 0 0 0 1 0.002098 0.554768 -0.832002 6.014472 0.003823 0.831994 0.554772 -1.264842 0.999990 -0.004344 -0.000376 2.888253 0 0 0 1 0 -0.000001 -1 1.399999 -0 1 -0.000001 -0.599996 1 0 0 2.885148 0 0 0 1 0.999961 -0.008298 -0.003012 2.504233 0.008648 0.989305 0.145603 1.111563 0.001772 -0.145624 0.989338 -5.841687 0 0 0 1 1 0 0 2.885148 -0 1 -0.000001 -0.599996 -0 0.000001 1 0.000002 0 0 0 1 0.256676 0.957929 -0.128409 -0.833228 -0.011007 0.135749 0.990682 -0.751913 0.966435 -0.252871 0.045388 2.090546 0 0 0 1 -0.256595 0.957623 -0.130836 -2.347204 -0.011012 0.132463 0.991127 -0.773801 0.966456 0.255758 -0.023444 3.607112 0 0 0 1 0 -0.972471 -0.233025 -0.390619 0 -0.233025 0.972471 -0.564066 -1 -0 0 -2.885148 0 0 0 1 -0 0.959551 -0.281534 -1.044431 -0.010661 0.281518 0.959497 -0.941531 0.999943 0.003002 0.010229 2.875273 0 0 0 1 0.257573 0.957690 -0.128399 -1.369874 0.012224 0.129642 0.991485 -0.701470 0.966181 -0.256950 0.021686 2.109286 0 0 0 1 0.210433 0.842365 -0.496124 -1.189731 0.011021 0.505411 0.862808 -1.430004 0.977546 -0.187031 0.097072 1.947561 0 0 0 1 0.210433 0.842365 -0.496124 -2.132204 0.011021 0.505411 0.862808 -1.430004 0.977546 -0.187031 0.097072 1.947561 0 0 0 1 -0.257518 0.957376 -0.130827 -2.893456 -0.035051 0.126049 0.991405 -0.863275 0.965638 0.259890 0.001097 3.583981 0 0 0 1 -0.225088 0.836700 -0.499268 -2.485888 -0.031675 0.505863 0.862032 -1.584614 0.973823 0.209848 -0.087362 3.763780 0 0 0 1 -0.225089 0.836700 -0.499268 -3.424760 -0.031675 0.505863 0.862032 -1.584614 0.973823 0.209848 -0.087362 3.763782 0 0 0 1 -0 -0.972471 -0.233025 -1.168555 0 -0.233025 0.972471 -0.564066 -1 0 0 -2.885148 0 0 0 1 -0 0.959551 -0.281534 -2.961370 -0.010661 0.281518 0.959497 -0.941530 0.999943 0.003002 0.010229 2.875274 0 0 0 1 1 0 -0 2.885148 -0 1 0.000001 -3.098014 0 -0.000001 1 0.000009 0 0 0 1 0 -0 1 -9.282160 -0 1 0 0.307090 -1 -0 0 0 0 0 0 1 0.000001 -0 1 -10.726149 -0 1 0 0.307090 -1 -0 0.000001 -0.000005 0 0 0 1 0.000001 -0 1 -10.957759 -0.000001 1 0 0.307090 -1 -0.000001 0.000001 -0.000009 0 0 0 1 -0.531478 -0.000001 -0.847072 8.086329 0 -1 0 -1.700724 -0.847072 -0 0.531478 -10.162396 0 0 0 1 0.531477 0.000001 -0.847073 8.086336 -0 -1 -0.000001 -1.700709 -0.847073 0 -0.531477 10.162374 0 0 0 1 1 0.000001 -0.000001 0.000010 -0.000001 1 0 -2.857098 0.000001 -0 1 -12.291666 0 0 0 1 1 0.000001 -0.000001 0.000009 -0.000001 1 0 -2.857097 0.000001 -0 1 -12.291667 0 0 0 1 -1 -0.000001 0.000001 -1.454218 -0 -0.707107 -0.707107 5.404586 0.000001 -0.707107 0.707107 -8.957491 0 0 0 1 -1 -0.000001 0.000001 -1.710584 -0 -0.707107 -0.707107 5.344195 0.000001 -0.707107 0.707107 -9.317745 0 0 0 1 -1 -0.000001 0.000001 1.511648 -0 -0.707107 -0.707107 5.402447 0.000002 -0.707107 0.707107 -8.954850 0 0 0 1 -1 -0.000001 0.000001 1.767603 -0 -0.707107 -0.707107 5.342055 0.000002 -0.707107 0.707107 -9.315106 0 0 0 1 -1 -0.000001 0.000001 0.041891 -0 -0.707107 -0.707107 5.313381 0.000001 -0.707107 0.707107 -9.133546 0 0 0 1 1 0.000001 -0.000001 0.000006 -0.000001 1 -0 -1.442909 0.000001 0 1 -11.937682 0 0 0 1 1 0.000001 -0.000001 -1.749993 -0.000001 1 0 -0.788977 0.000001 -0 1 -11.937676 0 0 0 1 1 0.000001 -0.000001 1.750005 -0.000001 1 -0 -0.788976 0.000001 0 1 -11.937681 0 0 0 1 -0.000001 -0.016842 -0.999858 11.221415 0.000188 -0.999858 0.016842 -1.889054 -1 -0.000188 0.000004 -4.937831 0 0 0 1 -0.000188 1 -0.000001 1.745410 -0.000004 -0.000001 -1 8.543955 -1 -0.000188 0.000004 -4.937831 0 0 0 1 -0 1 -0.000001 -1.278729 -0.011111 -0.000001 -0.999938 8.488578 -0.999938 -0 0.011111 -5.032661 0 0 0 1 0.999938 0.000191 0.011106 5.302608 -0.000191 1 0 1.515383 -0.011106 -0.000003 0.999938 -8.098288 0 0 0 1 -0 -0.016841 -0.999858 11.221406 -0.000191 -0.999858 0.016841 -1.889019 -1 0.000191 -0.000003 4.937818 0 0 0 1 0.000192 1 0 1.745375 0.000003 0 -1 8.543945 -1 0.000192 -0.000003 4.937819 0 0 0 1 0.000004 1 0.000001 -1.278764 -0.000001 0.000001 -1 8.543962 -1 0.000004 0.000001 4.938029 0 0 0 1 1 -0.000195 0.000003 -4.877823 0.000195 1 0.000002 1.695347 -0.000003 -0.000002 1 -8.533949 0 0 0 1 -0.003118 -0.652256 -0.757993 6.335854 0.003049 0.757987 -0.652263 5.435551 0.999990 -0.004345 -0.000375 -2.881985 0 0 0 1 0.002098 0.554769 -0.832002 6.002367 0.003823 0.831993 0.554773 -1.286910 0.999990 -0.004345 -0.000375 -2.881984 0 0 0 1 0 0 -1 1.400005 0 1 0 -0.600001 1 -0 0 -2.885144 0 0 0 1 0.999961 -0.008299 -0.003011 -3.075839 0.008649 0.989305 0.145604 1.091651 0.001771 -0.145625 0.989338 -5.711909 0 0 0 1 1 -0 -0 -2.885144 0 1 0 -0.600001 0 -0 1 -0.000008 0 0 0 1 -0.256676 0.957929 -0.128408 -0.833235 -0.011016 0.129951 0.991459 -0.704093 0.966435 0.255898 -0.022803 -2.107132 0 0 0 1 0.256594 0.957623 -0.130835 -2.347206 -0.011004 0.138253 0.990336 -0.691449 0.966456 -0.252675 0.046013 -3.623798 0 0 0 1 -0 -0.972471 -0.233025 -0.390614 -0 -0.233025 0.972471 -0.564074 -1 0 -0 2.885144 0 0 0 1 0 0.959551 -0.281533 -1.044434 -0.010661 0.281517 0.959497 -0.880024 0.999943 0.003001 0.010230 -2.894692 0 0 0 1 -0.257574 0.957690 -0.128398 -1.369879 -0.034420 0.123703 0.991722 -0.652829 0.965645 0.259861 0.001101 -2.124844 0 0 0 1 -0.225536 0.838453 -0.496115 -1.156751 -0.031134 0.502773 0.863858 -1.392009 0.973737 0.210277 -0.087289 -1.994333 0 0 0 1 -0.225536 0.838453 -0.496115 -2.099227 -0.031134 0.502773 0.863858 -1.392009 0.973737 0.210277 -0.087289 -1.994334 0 0 0 1 0.257518 0.957376 -0.130826 -2.893460 0.012854 0.131986 0.991168 -0.780703 0.966188 -0.256925 0.021683 -3.602867 0 0 0 1 0.225086 0.836690 -0.499286 -2.485926 0.011561 0.510106 0.860034 -1.506640 0.974270 -0.199354 0.105144 -3.795710 0 0 0 1 0.225086 0.836690 -0.499286 -3.424798 0.011561 0.510106 0.860034 -1.506639 0.974270 -0.199354 0.105144 -3.795709 0 0 0 1 -0 -0.972471 -0.233025 -1.168550 -0 -0.233025 0.972471 -0.564073 -1 0.000001 -0 2.885144 0 0 0 1 0 0.959552 -0.281533 -2.961374 -0.010661 0.281517 0.959497 -0.880024 0.999943 0.003001 0.010230 -2.894693 0 0 0 1 1 -0.000001 0.000001 -2.885145 0.000001 1 -0.000005 -3.098022 -0.000001 0.000005 1 -0.000021 0 0 0 1";
        Assert.IsTrue(controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArray));
        Assert.AreEqual(1024, controllerBindPose.Float_Array.Count);
        Assert.IsTrue(controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArray)); ;

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void MWO_candycane_a_MaterialFileNotAvailable()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\MWO\NoMats\candycane_a.chr", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();

        // Controller check
        var controller = daeObject.Library_Controllers.Controller[0];
        Assert.AreEqual("Controller", controller.ID);
        var skin = controller.Skin;
        Assert.AreEqual("#candycane_a-mesh", skin.source);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", skin.Bind_Shape_Matrix.Value_As_String);
        Assert.AreEqual(3, skin.Source.Length);
        var controllerJoints = skin.Source.Where(a => a.ID == "Controller-joints").First();
        var controllerBindPose = skin.Source.Where(a => a.ID == "Controller-bind_poses").First();
        var controllerWeights = skin.Source.Where(a => a.ID == "Controller-weights").First();
        var joints = skin.Joints;
        Assert.AreEqual(8, controllerJoints.Name_Array.Count);
        Assert.AreEqual("Controller-joints-array", controllerJoints.Name_Array.ID);

        var bindPoseArray = "0 0 -1 0.023305 1 0 0 0 0 -1 0 0 0 0 0 1 -0.000089 0 -1 -0.000092 1 0.000008 -0.000089 0 0.000008 -1 0 0 0 0 0 1 -0.000091 0 -1 -0.026455 1 0.000008 -0.000091 0 0.000008";
        var bindPoseArrayNegZeros = "-0 -0 -1 0.023305 1 -0 -0 -0 -0 -1 0 -0 0 0 0 1 -0.000089 -0 -1 -0.000092 1 0.000008 -0.000089 -0 0.000008 -1 0 -0 0 0 0 1 -0.000091 -0 -1 -0.026455 1 0.000008";
        Assert.AreEqual(128, controllerBindPose.Float_Array.Count);
        Assert.IsTrue(controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArray) || controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArrayNegZeros));
        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        Assert.AreEqual(2, actualMaterialsCount);

        // VisualScene
        var scene = daeObject.Library_Visual_Scene.Visual_Scene[0];
        Assert.AreEqual(2, scene.Node.Length);
        var armature = scene.Node[0];
        var instance = scene.Node[1];
        Assert.AreEqual("Bip01", armature.ID);
        Assert.AreEqual("Bip01", armature.Name);
        Assert.AreEqual("-0 -0 -1 0.023305 1 -0 -0 -0 -0 -1 0 -0 0 0 0 1", armature.Matrix[0].Value_As_String);
        Assert.AreEqual("hang_seg1", armature.node[0].ID);
        Assert.AreEqual("hang_seg1", armature.node[0].Name);
        Assert.AreEqual("-0.000089 -0 -1 -0.000092 1 0.000008 -0.000089 -0 0.000008 -1 0 -0 0 0 0 1", armature.node[0].Matrix[0].Value_As_String);
        Assert.AreEqual("hang_seg2", armature.node[0].node[0].ID);
        Assert.AreEqual("hang_seg2", armature.node[0].node[0].Name);
        Assert.AreEqual("-0.000091 -0 -1 -0.026455 1 0.000008 -0.000091 -0 0.000008 -1 0 -0 0 0 0 1", armature.node[0].node[0].Matrix[0].Value_As_String);

        Assert.AreEqual(2, instance.Instance_Controller[0].Bind_Material[0].Technique_Common.Instance_Material.Length);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void MWO_candycane_a_WithMaterialFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\MWO\candycane_a.chr", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();

        // Controller check
        var controller = daeObject.Library_Controllers.Controller[0];
        Assert.AreEqual("Controller", controller.ID);
        var skin = controller.Skin;
        Assert.AreEqual("#candycane_a-mesh", skin.source);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", skin.Bind_Shape_Matrix.Value_As_String);
        Assert.AreEqual(3, skin.Source.Length);
        var controllerJoints = skin.Source.Where(a => a.ID == "Controller-joints").First();
        var controllerBindPose = skin.Source.Where(a => a.ID == "Controller-bind_poses").First();
        var controllerWeights = skin.Source.Where(a => a.ID == "Controller-weights").First();
        var joints = skin.Joints;
        Assert.AreEqual(8, controllerJoints.Name_Array.Count);
        Assert.AreEqual("Controller-joints-array", controllerJoints.Name_Array.ID);

        var bindPoseArray = "0 0 -1 0.023305 1 0 0 0 0 -1 0 0 0 0 0 1 -0.000089 0 -1 -0.000092 1 0.000008 -0.000089 0 0.000008 -1 0 0 0 0 0 1 -0.000091 0 -1 -0.026455 1 0.000008 -0.000091 0 0.000008";
        var bindPoseArrayNegZeros = "-0 -0 -1 0.023305 1 -0 -0 -0 -0 -1 0 -0 0 0 0 1 -0.000089 -0 -1 -0.000092 1 0.000008 -0.000089 -0 0.000008 -1 0 -0 0 0 0 1 -0.000091 -0 -1 -0.026455 1 0.000008";
        Assert.AreEqual(128, controllerBindPose.Float_Array.Count);
        Assert.IsTrue(controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArray) || controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArrayNegZeros));
        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        Assert.AreEqual(2, actualMaterialsCount);

        // VisualScene
        var scene = daeObject.Library_Visual_Scene.Visual_Scene[0];
        Assert.AreEqual(2, scene.Node.Length);
        var armature = scene.Node[0];
        var geometryNode = scene.Node[1];
        Assert.AreEqual("Bip01", armature.ID);
        Assert.AreEqual("Bip01", armature.Name);
        Assert.AreEqual(2, geometryNode.Instance_Controller[0].Bind_Material[0].Technique_Common.Instance_Material.Length);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void MWO_hbr_right_torso_uac5_bh1_cga()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\mechs\hellbringer\body\hbr_right_torso.cga", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void MWO_hbr_right_torso_cga()
    {
        // No mtl file provided, so it creates generic mats
        var args = new string[] { $@"d:\depot\mwo\objects\mechs\hellbringer\body\hbr_right_torso.cga", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();

        Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
        Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
        // Visual Scene Check
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
        Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);
        // Node Matrix check
        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        const string matrix = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1";
        Assert.AreEqual(matrix, node.Matrix[0].Value_As_String);
        Assert.AreEqual("transform", node.Matrix[0].sID);
        // Instance Geometry check
        Assert.AreEqual("hbr_right_torso", node.Instance_Geometry[0].Name);
        Assert.AreEqual("#hbr_right_torso-mesh", node.Instance_Geometry[0].URL);
        Assert.AreEqual(1, node.Instance_Geometry[0].Bind_Material.Length);
        Assert.AreEqual(1, node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material.Length);
        Assert.AreEqual("mechDefault_mtl_material0-material", node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
        Assert.AreEqual("#mechDefault_mtl_material0-material", node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);

        // library_geometries check
        Assert.AreEqual(1, colladaData.DaeObject.Library_Geometries.Geometry.Length);
        var geometry = colladaData.DaeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("hbr_right_torso-mesh", geometry.ID);
        Assert.AreEqual("hbr_right_torso", geometry.Name);
        Assert.AreEqual(4, geometry.Mesh.Source.Length);
        Assert.AreEqual("hbr_right_torso-vertices", geometry.Mesh.Vertices.ID);
        Assert.AreEqual(1, geometry.Mesh.Triangles.Length);
        Assert.AreEqual(1908, geometry.Mesh.Triangles[0].Count);
        var source = geometry.Mesh.Source;
        var vertices = geometry.Mesh.Vertices;
        var triangles = geometry.Mesh.Triangles;
        // Triangles check
        Assert.AreEqual("mechDefault_mtl_material0-material", triangles[0].Material);
        Assert.AreEqual("#hbr_right_torso-mesh-pos", vertices.Input[0].source);
        Assert.IsTrue(triangles[0].P.Value_As_String.StartsWith("0 0 0 1 1 1 2 2 2 3 3 3 4 4 4 5 5 5 5 5 5 6 6 6 3 3 3 7 7 7 8 8 8 9 9 9 9 9 9"));
        // Source check
        Assert.AreEqual("hbr_right_torso-mesh-pos", source[0].ID);
        Assert.AreEqual("hbr_right_torso-pos", source[0].Name);
        Assert.AreEqual("hbr_right_torso-mesh-pos-array", source[0].Float_Array.ID);
        Assert.AreEqual(7035, source[0].Float_Array.Count);
        var floatArray = source[0].Float_Array.Value_As_String;
        Assert.IsTrue(floatArray.StartsWith("2.525999 -1.729837 -1.258107 2.526004 -1.863573 -1.080200 2.525999 -1.993050 -1.255200 2.740049 -0.917271 0.684382 2.740053 -0.917271 0.840976 2.793932"));
        Assert.IsTrue(floatArray.EndsWith("-3.263152 2.340879 -1.480840 -3.231119 2.352005 -1.494859 -3.268093 2.329598 -1.478497 -3.234514 2.335588 -1.491449 -3.273033 2.320036 -1.471824 -3.237391"));
        Assert.AreEqual((uint)2345, source[0].Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)0, source[0].Technique_Common.Accessor.Offset);
        Assert.AreEqual(3, source[0].Technique_Common.Accessor.Param.Length);
        Assert.AreEqual("X", source[0].Technique_Common.Accessor.Param[0].Name);
        Assert.AreEqual("float", source[0].Technique_Common.Accessor.Param[0].Type);

        Assert.AreEqual("hbr_right_torso", daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].ID);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].Instance_Geometry.Length);
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void HulaGirl_ColladaFormat()
    {
        var args = new string[] { @"d:\depot\MWO\objects\purchasable\cockpit_standing\hulagirl\hulagirl__gold_a.cga", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(1, actualMaterialsCount);
        var libraryGeometry = colladaData.DaeObject.Library_Geometries;
        Assert.AreEqual(2, libraryGeometry.Geometry.Length);
        var nodes = colladaData.DaeObject.Library_Visual_Scene.Visual_Scene[0].Node;
        var baseNode = nodes[0];
        Assert.AreEqual("hulagirl_a", baseNode.ID);
        Assert.AreEqual("hulagirl_a", baseNode.Name);
        Assert.AreEqual(2, baseNode.node.Length);
        Assert.IsNull(baseNode.Instance_Geometry);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", baseNode.Matrix[0].Value_As_String);

        // Serialize the object to XML
        XmlSerializer serializer = new(typeof(ColladaDoc));
        StringWriter writer = new();
        serializer.Serialize(writer, colladaData.DaeObject);

        // Load the serialized XML into an XDocument for LINQ querying
        XDocument doc = XDocument.Parse(writer.ToString());

        // Find nodes under visual_scene node
        var visualSceneNodes = doc.Descendants("visual_scene").Elements("node");

        foreach (var node in visualSceneNodes)
        {
            // Ensure the translate comes before rotate for each "node" element
            var translateElement = node.Elements("translate").FirstOrDefault();
            var rotateElement = node.Elements("rotate").FirstOrDefault();

            Assert.IsNotNull(translateElement, "Translate element not found");
            Assert.IsNotNull(rotateElement, "Rotate element not found");

            // Assert the order is correct
            int translateIndex = translateElement.ElementsBeforeSelf().Count();
            int rotateIndex = rotateElement.ElementsBeforeSelf().Count();

            Assert.IsTrue(translateIndex < rotateIndex, "Translate should come before Rotate");

            // Assert the values are correct
            string translateValues = translateElement.Value;
            string rotateValues = rotateElement.Value;

            Assert.AreEqual("0.000101 0 0.064078", translateValues, "Translate values are not as expected");
            Assert.AreEqual("-0.908837 0.415754 0.034101 12.949439", rotateValues, "Rotate values are not as expected");
        }
    }

    [TestMethod]
    public void HulaGirl_GltfFormat()
    {
        var args = new string[]
        {
            $@"D:\depot\MWO\Objects\purchasable\cockpit_standing\hulagirl\hulagirl__gold_a.cga",
            "-objectdir", objectDir,
            "-mat", "hulagirl_a.mtl"
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, materialFiles: args[4]);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();

        Assert.AreEqual(1, gltfData.Materials.Count);
        Assert.AreEqual(2, gltfData.Meshes.Count);

        // Nodes check
        Assert.AreEqual(3, gltfData.Nodes.Count);
        Assert.AreEqual("HulaGirl_UpperBody", gltfData.Nodes[0].Name);
        Assert.AreEqual("HulaGirl_LowerBody", gltfData.Nodes[1].Name);
        Assert.AreEqual("hulagirl_a", gltfData.Nodes[2].Name);

        var rotationMatrix = cryData.RootNode.AllChildNodes.First().Rot.ConvertToRotationMatrix();
        AssertExtensions.AreEqual([0, 0, 0, 1], gltfData.Nodes[1].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual([-0.10248467f, 0.00384537f, -0.04688235744833946f, 0.9936217f], gltfData.Nodes[0].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual([0, 0, 0, 1], gltfData.Nodes[2].Rotation, TestUtils.delta);

        AssertExtensions.AreEqual([0, 0, 0], gltfData.Nodes[1].Translation, TestUtils.delta);
        AssertExtensions.AreEqual([-0.000101296f, 0.0640777f, 0f], gltfData.Nodes[0].Translation, TestUtils.delta);
        AssertExtensions.AreEqual([0, 0, 0], gltfData.Nodes[2].Translation, TestUtils.delta);

        Assert.AreEqual(2, gltfData.Nodes[2].Children.Count);
        Assert.AreEqual(0, gltfData.Nodes[1].Children.Count);
        Assert.AreEqual(0, gltfData.Nodes[0].Children.Count);

        // Accessors check
        Assert.AreEqual(10, gltfData.Accessors.Count);
    }

    [TestMethod]
    public void MechFactory_CratesA_Gltf()
    {
        var args = new string[]
        {
            $@"d:\depot\mwo\objects\environments\mech_factory\mf_crates\mechfactory_cratesa.cgf",
            "-objectdir", objectDir,
            "-mat", "mf_crates.mtl"
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, materialFiles: args[4]);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
        Assert.AreEqual(2, gltfData.Textures.Count);
        Assert.AreEqual(2, gltfData.Materials.Count);
    }

    [TestMethod]
    public void MechFactory_CratesA_Collada()
    {
        var args = new string[] { $@"d:\depot\mwo\objects\environments\mech_factory\mf_crates\mechfactory_cratesa.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void Mf_Bldg_A_Corner_Slope_Gltf()
    {
        var args = new string[] { $@"D:\depot\mwo\objects\environments\mech_factory\building_blocks\mf_bldg_a_corner_slope_01.cgf", "-objectdir", "d:\\depot\\mwo" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
    }
}
