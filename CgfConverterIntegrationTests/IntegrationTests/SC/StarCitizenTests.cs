using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Gltf;
using CgfConverterIntegrationTests.Extensions;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class StarCitizenTests
{
    private readonly TestUtils testUtils = new();
    string userHome;

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void NavyPilotFlightSuit_Ivo()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\pilot_flightsuit\m_nvy_pilot_light_helmet_01.skin", "-dds", "-dae", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void CutlassRed_312_NonIvo()
    {
        var args = new string[] { $@"D:\depot\SC2\Data\objects\Spaceships\Ships\DRAK\Cutlass\Cutlass_Red\DRAK_Cutlass_Red.cga", "-dds", "-dae", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void CutlassBlue_312_Gltf_NonIvo()
    {
        var args = new string[] { $@"D:\depot\SC2\Data\objects\Spaceships\Ships\DRAK\Cutlass\Cutlass_Blue\DRAK_Cutlass_Blue.cga", "-dds", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        gltfRenderer.Render();
    }

    [TestMethod]
    public void AEGS_Vanguard_LandingGear_Front_IvoFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\AEGS_Vanguard_LandingGear_Front.skin", "-dds", "-dae" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void M_ccc_vanduul_helmet_01_312IvoSkinFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\m_ccc_vanduul_helmet_01.skin", "-dds", "-dae" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void M_ccc_bear_helmet_01_Pre320IvoSkinFile()
    {
        var args = new string[] { @"D:\depot\SC2\Data\Objects\Characters\Human\male_v7\armor\ccc\m_ccc_bear_helmet_01.skin", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void ANVL_Hurricane_Front_LandingGear_IvoCHR()
    {
        var args = new string[] {
            @"D:\depot\SC3.22\Data\Objects\Spaceships\Ships\ANVL\LandingGear\Hurricane\anvl_hurricane_landing_gear_front_chr.chr",
            "-dds", "-dae",
            "-objectdir", @"d:\depot\sc3.22\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void ANVL_Hurricane_Front_LandingGear_Ivo_Skin_3_22()
    {
        var args = new string[] {
            @"D:\depot\SC3.22\Data\Objects\Spaceships\Ships\ANVL\LandingGear\Hurricane\anvl_hurricane_landing_gear_front_SKIN.skin",
            "-dds", "-dae",
            "-objectdir", @"d:\depot\sc3.22\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();
        var meshChunk = (ChunkMesh)cryData.Chunks[7];  // This is the generated one from the skinm file
        Assert.AreEqual(-0.443651f, meshChunk.MinBound.X, TestUtils.delta);
        Assert.AreEqual(-0.2984485f, meshChunk.MinBound.Y, TestUtils.delta);
        Assert.AreEqual(-2.20503f, meshChunk.MinBound.Z, TestUtils.delta);
        Assert.AreEqual(0.443650f, meshChunk.MaxBound.X, TestUtils.delta);
        Assert.AreEqual(3.3411438f, meshChunk.MaxBound.Y, TestUtils.delta);
        Assert.AreEqual(1.4569355f, meshChunk.MaxBound.Z, TestUtils.delta);

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void ANVL_Hurricane_Front_LandingGear_Ivo_Skin_3_23()
    {
        var args = new string[] {
            @"D:\depot\SC3.23\Data\Objects\Spaceships\Ships\ANVL\LandingGear\Hurricane\anvl_hurricane_landing_gear_front_SKIN.skin",
            "-dds", "-dae",
            "-objectdir", @"d:\depot\sc3.23\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();
        var meshChunk = (ChunkMesh)cryData.Chunks[7];  // This is the generated one from the skinm file
        Assert.AreEqual(-0.443651f, meshChunk.MinBound.X, TestUtils.delta);
        Assert.AreEqual(-0.2984485f, meshChunk.MinBound.Y, TestUtils.delta);
        Assert.AreEqual(-2.20503f, meshChunk.MinBound.Z, TestUtils.delta);
        Assert.AreEqual(0.443650f, meshChunk.MaxBound.X, TestUtils.delta);
        Assert.AreEqual(3.3411438f, meshChunk.MaxBound.Y, TestUtils.delta);
        Assert.AreEqual(1.4569355f, meshChunk.MaxBound.Z, TestUtils.delta);

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void M_ccc_bear_helmet_01_320IvoSkinFile()
    {
        var args = new string[] {
            @"D:\depot\SC3.22\Data\Objects\Characters\Human\male_v7\armor\ccc\m_ccc_bear_helmet_01.skin", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Avenger_LandingGear_SkinFile_322()
    {
        var args = new string[] { @"D:\depot\SC3.22\Data\Objects\Spaceships\Ships\AEGS\LandingGear\Avenger\AEGS_Avenger_LandingGear_Back.skin", "-dds", "-dae", "-objectdir", @"d:\depot\sc3.22\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void BehrRifle_312IvoChrFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar.chr", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        //Assert.AreEqual(17, cryData.Materials.Count);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void BehrRifle_312IvoSkinFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar_parts.skin", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void AEGS_Avenger()
    {
        var args = new string[] { $@"d:\depot\sc2\data\objects\spaceships\ships\AEGS\Avenger\AEGS_Avenger.cga", "-dds", "-dae", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        Assert.AreEqual("Nose", noseNode.ID);
        Assert.AreEqual("Front_LG_Door_Left", noseNode.node[28].ID);
        Assert.AreEqual("1 0 0 -0.300001 0 -0.938131 -0.346280 0.512432 0 0.346280 -0.938131 -1.835138 0 0 0 1", noseNode.node[28].Matrix[0].Value_As_String);

        Assert.AreEqual(29, colladaData.DaeObject.Library_Materials.Material.Length);
        Assert.AreEqual(88, colladaData.DaeObject.Library_Images.Image.Length);
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void CRUS_Spirit_Exterior()
    {
        var args = new string[] { $@"d:\depot\sc3.22\data\objects\spaceships\ships\CRUS\spirit\exterior\crus_Spirit.cga", "-dds", "-dae", "-objectdir", @"d:\depot\sc3.22\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        var bodyNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        Assert.AreEqual("body", bodyNode.ID);
        Assert.AreEqual("wing_base_right", bodyNode.node[28].ID);
        Assert.AreEqual("1 0 0 9.400001 0 1 0 -2.750000 0 0 1 -1.200000 0 0 0 1", bodyNode.node[28].Matrix[0].Value_As_String);

        Assert.AreEqual(93, colladaData.DaeObject.Library_Materials.Material.Length);
        Assert.AreEqual(88, colladaData.DaeObject.Library_Images.Image.Length);
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void AEGS_Avenger_v320()
    {
        var args = new string[] {
            $@"d:\depot\sc3.22\data\objects\spaceships\ships\AEGS\Avenger\AEGS_Avenger.cga",
            "-dds", "-dae",
            "-objectdir", @"d:\depot\sc3.22\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        Assert.AreEqual("Nose", noseNode.ID);
        Assert.AreEqual("Front_LG_Door_Left", noseNode.node[28].ID);
        Assert.AreEqual("1 0 0 -0.300001 0 -0.938131 -0.346280 0.512432 0 0.346280 -0.938131 -1.835138 0 0 0 1", noseNode.node[28].Matrix[0].Value_As_String);

        Assert.AreEqual(31, colladaData.DaeObject.Library_Materials.Material.Length);
        Assert.AreEqual(53, colladaData.DaeObject.Library_Images.Image.Length);
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void AEGS_Avenger_Gltf()
    {
        var args = new string[] { @"d:\depot\sc2\data\objects\spaceships\ships\aegs\Avenger\AEGS_Avenger.cga", "-dds", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();

        Assert.AreEqual(26, gltfData.Materials.Count);
        Assert.AreEqual(34, gltfData.Meshes.Count);

        // Nodes check
        Assert.AreEqual(116, gltfData.Nodes.Count);
        Assert.AreEqual("UI_Helper", gltfData.Nodes[0].Name);
        Assert.AreEqual("hardpoint_controller_energy", gltfData.Nodes[1].Name);
        Assert.AreEqual("hardpoint_controller_door", gltfData.Nodes[2].Name);

        AssertExtensions.AreEqual([0, 0, 0, 1], gltfData.Nodes[0].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual([-0.0f, -0.0f, 0.0f, 1f], gltfData.Nodes[1].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual([0, 0, 0, 1], gltfData.Nodes[2].Rotation, TestUtils.delta);

        AssertExtensions.AreEqual([0, 0.7958946f, 1.898374f], gltfData.Nodes[0].Translation, TestUtils.delta);
        AssertExtensions.AreEqual([0.0f, 0.472894579f, -6.56762552f], gltfData.Nodes[1].Translation, TestUtils.delta);
        AssertExtensions.AreEqual([0f, 0.472894579f, -6.46762562f], gltfData.Nodes[2].Translation, TestUtils.delta);

        // Grip.  Test loc and rotation on a node with a parent
        var grip = gltfData.Nodes.Where(x => x.Name == "Grip").FirstOrDefault();
        AssertExtensions.AreEqual([1.41231394f, 0.0213999934f, 1.660965f], grip.Translation, TestUtils.delta);
        AssertExtensions.AreEqual([0.464955121f, -0.221349508f, 0.769474566f, 0.3777963f], grip.Rotation, TestUtils.delta);

        Assert.AreEqual(0, gltfData.Nodes[0].Children.Count); // Root
        Assert.AreEqual(0, gltfData.Nodes[1].Children.Count);
        Assert.AreEqual(0, gltfData.Nodes[2].Children.Count);

        // Accessors check
        Assert.AreEqual(281, gltfData.Accessors.Count);
    }

    [TestMethod]
    public void AEGS_GladiusLandingGearFront_CHR()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\AEGS_Gladius_LandingGear_Front_CHR.chr", "-dds", "-dae", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();

        Assert.IsFalse(cryData.Models[0].HasGeometry);
    }

    [TestMethod]
    public void BehrRifle_312_NonIvo()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar_body.cgf",
            "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        var mesh = geometries[0].Mesh;
        Assert.AreEqual(4, mesh.Source.Length);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-vertices", mesh.Vertices.ID);
        Assert.AreEqual(13, mesh.Triangles.Length);
        Assert.AreEqual(84, mesh.Triangles[0].Count);
        Assert.AreEqual(1460, mesh.Triangles[8].Count);

        var vertices = mesh.Source[0];
        var normals = mesh.Source[1];
        var uvs = mesh.Source[2];
        var colors = mesh.Source[3];
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-pos", vertices.ID);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-pos", vertices.Name);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-norm", normals.ID);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-norm", normals.Name);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-UV", uvs.ID);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-UV", uvs.Name);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-color", colors.ID);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-color", colors.Name);
        Assert.AreEqual(56058, vertices.Float_Array.Count);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-pos-array", vertices.Float_Array.ID);
        Assert.IsTrue(vertices.Float_Array.Value_As_String.StartsWith("-0.020622 0.180945 0.097055 -0.020622 0.178238 0.092718 -0.020622 0.175470 0.097055 -0.020622 0.175408 0.105175 -0.020622"));
        Assert.AreEqual((uint)18686, vertices.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)3, vertices.Technique_Common.Accessor.Stride);
        Assert.AreEqual(56058, normals.Float_Array.Count);
        Assert.AreEqual((uint)18686, normals.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)3, normals.Technique_Common.Accessor.Stride);
        Assert.AreEqual(37372, uvs.Float_Array.Count);
        Assert.AreEqual((uint)18686, uvs.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)2, uvs.Technique_Common.Accessor.Stride);
        Assert.AreEqual(74744, colors.Float_Array.Count);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void DRAK_Buccaneer_Landing_Gear_Front_Skin()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\DRAK_Buccaneer_Landing_Gear_Front_Skin.skin",
            "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(25, materials.Length);
    }

    //D:\depot\SC3.22\Data\Objects\Characters\Mobiglas
    [TestMethod]
    public void Mobiglass()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\f_mobiglas_civilian_01.skin",
            "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(5, materials.Length);
    }

    [TestMethod]
    public void Mobiglass_Collada_InDirectory()
    {
        var args = new string[] {
            $@"D:\depot\SC3.22\Data\Objects\Characters\Mobiglas\f_mobiglas_civilian_01.skin",
            "-dds", "-dae", "-objectdir", @"d:\depot\sc3.22\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(5, materials.Length);

        // Visual scene checks
        var visualScene = colladaData.DaeObject.Library_Visual_Scene.Visual_Scene[0];
        Assert.AreEqual(2, visualScene.Node.Length);
        Assert.AreEqual(ColladaNodeType.JOINT, visualScene.Node[0].Type);
        Assert.AreEqual(ColladaNodeType.NODE, visualScene.Node[1].Type);
        Assert.AreEqual("World", visualScene.Node[0].Name);
        Assert.AreEqual("f_mobiglas_civilian_01", visualScene.Node[1].Name);
        var node0 = visualScene.Node[0];
        var node1 = visualScene.Node[1];
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", node1.Matrix[0].Value_As_String);
        Assert.AreEqual(5, node0.node.Length);
        var hipNode = node0.node[2];
        Assert.AreEqual("Hips", hipNode.Name);
        Assert.AreEqual("Hips", hipNode.ID);
        Assert.AreEqual(13, hipNode.node.Length);
        Assert.AreEqual("-0 -0.000002 -1 -0 -0 1 -0.000002 -0.014728 1 0 -0 1.005547 0 0 0 1", hipNode.Matrix[0].Value_As_String);
        Assert.AreEqual("RItemPort_IKTarget", hipNode.node[0].Name);
        Assert.AreEqual("-1 0 -0 -0.033969 -0 0 1 0.014687 0 1 0 -0.245430 0 0 0 1", hipNode.node[0].Matrix[0].Value_As_String);

        // Controller checks
        var controller = colladaData.DaeObject.Library_Controllers.Controller[0];
        Assert.AreEqual("#f_mobiglas_civilian_01-mesh", controller.Skin.source);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", controller.Skin.Bind_Shape_Matrix.Value_As_String);
        Assert.IsTrue(controller.Skin.Source[1].Float_Array.Value_As_String.StartsWith("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 -0 -0 0 0 1 -0 0 0 0 1 -0 -0 0 0 1 -0 -0.000002 -1 1.005547 -0 1 -0.000002 0.014730 1 0 -0 0 0 0 0 1 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 -0 -1 -0.000002 -0.000038 -0 -0.000002 1 -0.971578 -1 0 -0 0.245430 0 0 0 1 -0 -0.000002 -1 1.005547 -0 1 -0.000002 0.014730 1 0 -0 0 0 0 0 1 0.949039"));
    }

    [TestMethod]
    public void Mobiglass_Gltf()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\f_mobiglas_civilian_01.skin",
            "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
        gltfRenderer.Render();
    }

    [TestMethod]
    public void Avenger_Ramp_Exterior()
    {
        var args = new string[] { $@"D:\depot\SC3.22\Data\Objects\Spaceships\Ships\AEGS\Avenger\aegs_avenger_ramp_exterior.cga", "-dds", "-gltf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
        var geometries = gltfData.Meshes;

        gltfRenderer.Render();
    }


    [TestMethod]
    public void Glaive()
    {
        var args = new string[] {
            $@"d:\depot\sc3.22\data\objects\spaceships\ships\VNCL\Glaive\VNCL_Glaive.cga",
            "-dds", "-dae", "-objectdir", @"d:\depot\sc3.22\data" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(96, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(21, materials.Length);
    }
}
