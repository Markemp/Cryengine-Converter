using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers;
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
    private readonly string objectDir = @"d:\depot\sc3.24\data";
    private readonly string objectDir322 = @"d:\depot\sc3.22\data";
    private readonly string objectDir41 = @"d:\depot\sc4.1\data";

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
    public void AEGS_Avenger_324()
    {
        var args = new string[] { $@"{objectDir}\objects\spaceships\ships\AEGS\Avenger\AEGS_Avenger.cga", "-dds", "-dae", "-objectdir", $"{objectDir}" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);

        var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        var leftWing = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[1].node[0];

        Assert.AreEqual("Nose", noseNode.ID);
        Assert.AreEqual("hardpoint_radar", noseNode.node[28].ID);
        Assert.AreEqual("1 0 0 0 0 1 0 3.925374 0 0 1 -1.074105 0 0 0 1", noseNode.node[28].Matrix[0].Value_As_String);
        Assert.AreEqual("Wing_Left", leftWing.Name);
        Assert.AreEqual("1 0 0 -5.550000 0 1 0 -0.070000 0 0 1 -0.883000 0 0 0 1", leftWing.Matrix[0].Value_As_String);

        Assert.AreEqual(49, colladaData.DaeObject.Library_Materials.Material.Length);
        Assert.AreEqual(128, colladaData.DaeObject.Library_Images.Image.Length);

        // Geometry
        var noseGeo = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("Nose-mesh", noseGeo.ID);
        Assert.AreEqual(4, noseGeo.Mesh.Source.Length);
        Assert.AreEqual(15, noseGeo.Mesh.Triangles.Length);
        Assert.AreEqual(59817, noseGeo.Mesh.Source[0].Float_Array.Count);
        Assert.IsTrue(noseGeo.Mesh.Source[0].Float_Array.Value_As_String.StartsWith("4.480176 -3.697465 -0.268108"));
    }

    [TestMethod]
    public void AEGS_Avenger_41()
    {
        var args = new string[] { $@"{objectDir41}\objects\spaceships\ships\AEGS\Avenger\AEGS_Avenger.cga", "-dds", "-dae", "-objectdir", $"{objectDir}" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);

        var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        var leftWing = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[1].node[0];

        Assert.AreEqual("Nose", noseNode.ID);
        Assert.AreEqual("hardpoint_radar", noseNode.node[28].ID);
        Assert.AreEqual("1 0 0 0 0 1 0 3.925374 0 0 1 -1.074105 0 0 0 1", noseNode.node[28].Matrix[0].Value_As_String);
        Assert.AreEqual("Wing_Left", leftWing.Name);
        Assert.AreEqual("1 0 0 -5.550000 0 1 0 -0.070000 0 0 1 -0.883000 0 0 0 1", leftWing.Matrix[0].Value_As_String);

        Assert.AreEqual(49, colladaData.DaeObject.Library_Materials.Material.Length);
        Assert.AreEqual(128, colladaData.DaeObject.Library_Images.Image.Length);

        // Geometry
        var noseGeo = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("Nose-mesh", noseGeo.ID);
        Assert.AreEqual(4, noseGeo.Mesh.Source.Length);
        Assert.AreEqual(15, noseGeo.Mesh.Triangles.Length);
        Assert.AreEqual(59817, noseGeo.Mesh.Source[0].Float_Array.Count);
        Assert.IsTrue(noseGeo.Mesh.Source[0].Float_Array.Value_As_String.StartsWith("4.480176 -3.697465 -0.268108"));
    }


    [TestMethod]
    public void AEGS_Avenger_322()
    {
        var args = new string[] { $@"{objectDir322}\objects\spaceships\ships\AEGS\Avenger\AEGS_Avenger.cga" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir322);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);

        var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        var leftWing = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[1].node[0];
        Assert.AreEqual("Nose", noseNode.ID);
        Assert.AreEqual("1 -0 0 0 0 1 0 5.702999 0 0 1 -0.473000 0 0 0 0", noseNode.Matrix[0].Value_As_String);
        Assert.AreEqual("#Nose-mesh", noseNode.Instance_Geometry[0].URL);
        Assert.AreEqual(15, noseNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material.Length);
        Assert.AreEqual("#aegs_avenger_exterior_mtl_white_insulation_pads-material", noseNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("Front_LG_Door_Left", noseNode.node[28].ID);
        Assert.AreEqual("1 0 0 -0.300001 0 -0.938131 -0.346280 0.512432 0 0.346280 -0.938131 -1.835138 0 0 0 0", noseNode.node[28].Matrix[0].Value_As_String);
        Assert.AreEqual("Wing_Left", leftWing.Name);
        Assert.AreEqual("1 0 0 -5.550000 0 1 0 -0.070000 0 0 1 -0.883000 0 0 0 0", leftWing.Matrix[0].Value_As_String);

        Assert.AreEqual(49, colladaData.DaeObject.Library_Materials.Material.Length);
        Assert.AreEqual(129, colladaData.DaeObject.Library_Images.Image.Length);


        // Geometry
        var noseGeo = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("Nose-mesh", noseGeo.ID);
        Assert.AreEqual(4, noseGeo.Mesh.Source.Length);
        Assert.AreEqual(15, noseGeo.Mesh.Triangles.Length);
        Assert.AreEqual(59817, noseGeo.Mesh.Source[0].Float_Array.Count);
    }

    [TestMethod]
    public void AEGS_Avenger_Gltf()
    {
        var args = new string[] {$@"{objectDir41}\objects\spaceships\ships\aegs\Avenger\AEGS_Avenger.cga", "-objectDir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();

        Assert.AreEqual(28, gltfData.Materials.Count);
        Assert.AreEqual(36, gltfData.Meshes.Count);

        // Nodes check
        Assert.AreEqual(121, gltfData.Nodes.Count);
        Assert.AreEqual("Front_LG_Door_Right", gltfData.Nodes[0].Name);
        Assert.AreEqual("Front_LG_Door_Left", gltfData.Nodes[1].Name);
        Assert.AreEqual("Canopy", gltfData.Nodes[2].Name);

        //AssertExtensions.AreEqual([0, 0, 0, 1], gltfData.Nodes[0].Rotation, TestUtils.delta);
        //AssertExtensions.AreEqual([-0.0f, -0.0f, 0.0f, 1f], gltfData.Nodes[1].Rotation, TestUtils.delta);
        //AssertExtensions.AreEqual([0, 0, 0, 1], gltfData.Nodes[2].Rotation, TestUtils.delta);

        //AssertExtensions.AreEqual([0, 0.7958946f, 1.898374f], gltfData.Nodes[0].Translation, TestUtils.delta);
        //AssertExtensions.AreEqual([0.0f, 0.472894579f, -6.56762552f], gltfData.Nodes[1].Translation, TestUtils.delta);
        //AssertExtensions.AreEqual([0f, 0.472894579f, -6.46762562f], gltfData.Nodes[2].Translation, TestUtils.delta);

        //// Grip.  Test loc and rotation on a node with a parent
        //var grip = gltfData.Nodes.Where(x => x.Name == "Grip").FirstOrDefault();
        //AssertExtensions.AreEqual([1.41231394f, 0.0213999934f, 1.660965f], grip.Translation, TestUtils.delta);
        //AssertExtensions.AreEqual([0.464955121f, -0.221349508f, 0.769474566f, 0.3777963f], grip.Rotation, TestUtils.delta);

        Assert.AreEqual(0, gltfData.Nodes[0].Children.Count); // Root
        Assert.AreEqual(0, gltfData.Nodes[1].Children.Count);
        Assert.AreEqual(0, gltfData.Nodes[2].Children.Count);

        // Accessors check
        Assert.AreEqual(250, gltfData.Accessors.Count);
    }

    [TestMethod]
    public void AEGS_GladiusLandingGearFront_CHR()
    {
        var args = new string[] { $@"{objectDir}\Objects\Spaceships\Ships\AEGS\LandingGear\Gladius\AEGS_Gladius_LandingGear_Front_CHR.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        Assert.IsFalse(cryData.Models[0].HasGeometry);

        var daeObject = colladaData.DaeObject;
        var scene = daeObject.Library_Visual_Scene.Visual_Scene[0];
        var geometry = daeObject.Library_Geometries;
        var materials = daeObject.Library_Materials;

        Assert.AreEqual("AEGS_Gladius_LandingGear_Front_Anim", scene.Node[0].ID);
        Assert.AreEqual(0, geometry.Geometry.Length);
        Assert.AreEqual(55, materials.Material.Length);
    }

    [TestMethod]
    public void AEGS_Idris_Holo_viewer_cgf_41()
    {
        var args = new string[] { $@"{objectDir41}\Objects\Spaceships\holoviewer_ships\aegs_idris_holo_viewer.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, null, materialFiles: "aegs_idris_holo_viewer.mtl", objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var daeObject = colladaData.DaeObject;
        var scene = daeObject.Library_Visual_Scene.Visual_Scene[0];
        var geometry = daeObject.Library_Geometries.Geometry[0];
        var materials = daeObject.Library_Materials;

        Assert.AreEqual("AEGS_Idris_holo_viewer", scene.Node[0].ID);
        Assert.AreEqual("AEGS_Idris_holo_viewer-mesh", geometry.ID);
        Assert.AreEqual(4, geometry.Mesh.Triangles.Length);
        Assert.AreEqual(10, materials.Material.Length);
    }

    [TestMethod]
    public void AEGS_Idris_Holo_01_cga_41()
    {
        // No geometry or scenes
        var args = new string[] { $@"{objectDir}\Objects\Spaceships\holoviewer_ships\AEGS_Idris_holo_01.cga" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, null, materialFiles: "AEGS_Idris_holo.mtl", objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var daeObject = colladaData.DaeObject;
        var scene = daeObject.Library_Visual_Scene.Visual_Scene[0];
        var geometry = daeObject.Library_Geometries.Geometry[0];
        var materials = daeObject.Library_Materials;

        Assert.AreEqual("AEGS_Idris_holo_01", scene.Node[0].ID);
        Assert.AreEqual("addiitonal01-mesh", geometry.ID);
        Assert.AreEqual(1, geometry.Mesh.Triangles.Length);
        Assert.AreEqual(6, materials.Material.Length);
    }

    [TestMethod]
    public void AEGS_Vanguard_LandingGear_Front_IvoFile()
    {
        var args = new string[] { $@"{objectDir41}\objects\spaceships\ships\AEGS\LandingGear\Vanguard\AEGS_Vanguard_LandingGear_Front.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void ANVL_Arrow_322()
    {
        var args = new string[] { $@"{objectDir322}\objects\spaceships\ships\ANVL\Arrow\ANVL_Arrow.cga" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir322);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        var rightWingNode = cryData.Nodes.Where(x => x.Name == "wing_right");
        var rightWingGeoNode = cryData.Models[1].NodeMap.Values.Where(x => x.Name == "wing_right").First();
        var colladaGeo = daeObject.Library_Geometries.Geometry[24].Mesh.Source[0].Float_Array.Value_As_String.Split(' ');
    }

    [TestMethod]
    public void ANVL_Arrow_Ivo()
    {
        var args = new string[] { $@"{objectDir41}\objects\spaceships\ships\ANVL\Arrow\ANVL_Arrow.cga" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void ANVL_Hurricane_Front_LandingGear_Ivo_Skin_324()
    {
        var args = new string[] {
            $@"{objectDir}\Objects\Spaceships\Ships\ANVL\LandingGear\Hurricane\anvl_hurricane_landing_gear_front_SKIN.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();
        var mesh = (ChunkMesh)cryData.RootNode.MeshData;
        Assert.AreEqual(-0.443651f, mesh.MinBound.X, TestUtils.delta);
        Assert.AreEqual(-0.2984485f, mesh.MinBound.Y, TestUtils.delta);
        Assert.AreEqual(-2.20503f, mesh.MinBound.Z, TestUtils.delta);
        Assert.AreEqual(0.443650f, mesh.MaxBound.X, TestUtils.delta);
        Assert.AreEqual(3.3411438f, mesh.MaxBound.Y, TestUtils.delta);
        Assert.AreEqual(1.4569355f, mesh.MaxBound.Z, TestUtils.delta);

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void ANVL_Hurricane_Front_LandingGear_IvoCHR()
    {
        var args = new string[] { $@"{objectDir322}\Objects\Spaceships\Ships\ANVL\LandingGear\Hurricane\anvl_hurricane_landing_gear_front_chr.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir322);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void ANVL_Valkyrie_Turret_Bubble_Cga()
    {
        var args = new string[] {
            @$"{objectDir41}\Objects\Spaceships\Turrets\ANVL\Valkyrie\ANVL_Valkyrie_Turret_Bubble.cga" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Argo_Atlas_Powersuit_41()
    {
        var args = new string[] { $@"{objectDir41}\Objects\Characters\PowerSuit\ARGO\ATLS\argo_atls_powersuit_l_leg.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        
    }

    [TestMethod]
    public void Avenger_LandingGear_SkinFile_324()
    {
        var args = new string[] { $@"{objectDir}\Objects\Spaceships\Ships\AEGS\LandingGear\Avenger\AEGS_Avenger_LandingGear_Back.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
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
    public void BehrRifle_324IvoCgfFile()
    {
        var args = new string[] { $@"{objectDir}\Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar_stock.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void BehrRifle_324IvoChrFile()
    {
        var args = new string[] { $@"{objectDir}\Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        Assert.AreEqual(43, daeObject.Library_Materials.Material.Length);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void BehrRifle_324IvoSkinFile()
    {
        var args = new string[]
        {
            $@"{objectDir}\Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar_parts.skin",
            "-dds", "-dae", "-objectdir", objectDir
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: args[4]);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void BehrRifle_41IvoSkinFile()
    {
        var args = new string[]
        {
            $@"{objectDir41}\Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar_parts.skin",
            "-dds", "-dae", "-objectdir", objectDir41
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: args[4]);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void Bmsl_Fps_APAR_Animus_Body()
    {
        var args = new string[] { $@"{objectDir}\Objects\fps_weapons\weapons_v7\apar\launcher\animus\bmsl_fps_apar_animus_body.cga" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void BMSL_FPS_APAR_Animus_Body_v324_Ivo()
    {
        var args = new string[] { $@"{objectDir}\Objects\fps_weapons\weapons_v7\apar\launcher\animus\bmsl_fps_apar_animus_body.cga", "-dds", "-dae",
            "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();
    }

    [TestMethod]
    public void Box_Collada_Ivo()
    {
        var args = new string[] {$@"{objectDir}\Objects\default\box.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        // Visual Scene checks
        var boxNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        var boxMeshNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node;
        Assert.AreEqual("box", boxNode.ID);
        Assert.AreEqual(ColladaNodeType.NODE, boxNode.Type);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", boxNode.Matrix[0].Value_As_String);
        Assert.AreEqual("#grid_grayyellow_mtl_grid_grey-material", boxMeshNode[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey-material", boxMeshNode[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);

        // Geometry Checks
        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("mesh_box-mesh", geometry.ID);
        Assert.AreEqual(1, daeObject.Library_Geometries.Geometry.Length);
        var mesh = geometry.Mesh;
        Assert.AreEqual(4, mesh.Source.Length);
        Assert.AreEqual(1, mesh.Triangles.Length);
        Assert.AreEqual(12, mesh.Triangles[0].Count);

        // Materials Checks
        var mats = daeObject.Library_Materials;
        Assert.AreEqual(3, mats.Material.Length);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey", mats.Material[0].Name);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey-material", mats.Material[0].ID);
        Assert.AreEqual("#grid_grayyellow_mtl_grid_grey-effect", mats.Material[0].Instance_Effect.URL);
        Assert.AreEqual("grid_grayyellow_mtl_grid_yellow", mats.Material[1].Name);
        var boundMaterials = boxMeshNode[0].Instance_Geometry[0].Bind_Material;
        Assert.AreEqual("#grid_grayyellow_mtl_grid_grey-material", boundMaterials[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey-material", boundMaterials[0].Technique_Common.Instance_Material[0].Symbol);
        Assert.AreEqual(1, boundMaterials[0].Technique_Common.Instance_Material.Length);
    }

    [TestMethod]
    public void Box_Gltf_Ivo()
    {
        var args = new string[] { $@"{objectDir41}\Objects\default\box.cgf", "-objectDir", objectDir41};
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();

        // Materials checks
        Assert.AreEqual(3, gltfData.Materials.Count);
        Assert.AreEqual("grid_grey", gltfData.Materials[0].Name);
        Assert.AreEqual(2, gltfData.Textures.Count);
        Assert.AreEqual(2, gltfData.Images.Count);
        Assert.AreEqual(@"d:\depot\sc4.1\data\Textures\defaults\defaultnouvs.dds", gltfData.Images[0].Uri);

        // Geometry checks
        Assert.AreEqual(1, gltfData.Meshes.Count);
        Assert.AreEqual("mesh_box/mesh", gltfData.Meshes[0].Name);
    }

    [TestMethod]
    public void Console_Info_Banu_1_a_Gltf()
    {
        var args = new string[] { $@"{objectDir41}\Objects\buildingsets\banu\props\interactive\console\console_info_banu_1_a.cgf", "-objectDir", objectDir41};
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
    }

    [TestMethod]
    public void Console_Info_Banu_1_a_Gltf_EmbedImages()
    {
        var args = new string[] { $@"{objectDir41}\Objects\buildingsets\banu\props\interactive\console\console_info_banu_1_a.cgf", "-objectDir", objectDir41, "-embedtextures" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();

        // Verify name for embedded image
        Assert.AreEqual(@"decal pom-normal/image", gltfData.Images[0].Name);
        Assert.IsNull(gltfData.Images[0].Uri);
    }

    [TestMethod]
    public void Console_Info_Banu_1_a_Collada()
    {
        var args = new string[] { $@"{objectDir41}\Objects\buildingsets\banu\props\interactive\console\console_info_banu_1_a.cgf", "-objectDir", @"d:\depot\sc4.1\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Box_Collada_322()
    {
        var args = new string[] {$@"{objectDir322}\Objects\default\box.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir322);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        // Visual Scene checks
        var rootNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("box", rootNode.ID);
        Assert.AreEqual(ColladaNodeType.NODE, rootNode.Type);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0", rootNode.Matrix[0].Value_As_String);
        var boxNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        Assert.AreEqual("mesh_box", boxNode.ID);
        Assert.AreEqual(ColladaNodeType.NODE, boxNode.Type);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0", boxNode.Matrix[0].Value_As_String);
        Assert.AreEqual("#grid_grayyellow_mtl_grid_grey-material", boxNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey-material", boxNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);

        // Geometry Checks
        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("mesh_box-mesh", geometry.ID);
        Assert.AreEqual(1, daeObject.Library_Geometries.Geometry.Length);
        var mesh = geometry.Mesh;
        Assert.AreEqual(4, mesh.Source.Length);
        Assert.AreEqual(1, mesh.Triangles.Length);
        Assert.AreEqual(12, mesh.Triangles[0].Count);

        // Materials Checks
        var mats = daeObject.Library_Materials;
        Assert.AreEqual(3, mats.Material.Length);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey", mats.Material[0].Name);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey-material", mats.Material[0].ID);
        Assert.AreEqual("#grid_grayyellow_mtl_grid_grey-effect", mats.Material[0].Instance_Effect.URL);
        Assert.AreEqual("grid_grayyellow_mtl_grid_yellow", mats.Material[1].Name);
        var boundMaterials = boxNode.Instance_Geometry[0].Bind_Material;
        Assert.AreEqual("#grid_grayyellow_mtl_grid_grey-material", boundMaterials[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey-material", boundMaterials[0].Technique_Common.Instance_Material[0].Symbol);
        Assert.AreEqual(1, boundMaterials[0].Technique_Common.Instance_Material.Length);
    }

    [TestMethod]
    public void CRUS_Spirit_Exterior()
    {
        var args = new string[] { $@"{objectDir41}\objects\spaceships\ships\CRUS\spirit\exterior\crus_Spirit.cga" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        var bodyNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        Assert.AreEqual("body", bodyNode.ID);
        Assert.AreEqual("hardpoint_turret_rear_radar", bodyNode.node[28].ID);
        Assert.AreEqual("1 0 0 0.000001 0 1 0 1 0 0 1 -1.350000 0 0 0 1", bodyNode.node[28].Matrix[0].Value_As_String);

        Assert.AreEqual(134, colladaData.DaeObject.Library_Materials.Material.Length);
        Assert.AreEqual(246, colladaData.DaeObject.Library_Images.Image.Length);
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void DRAK_Buccaneer_Landing_Gear_Front_Skin()
    {
        var args = new string[] {
            $@"{objectDir41}\Objects\Spaceships\Ships\DRAK\Buccaneer\Landing_Gear\DRAK_Buccaneer_Landing_Gear_Front_Skin.skin",
            "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(39, materials.Length);
    }

    [TestMethod]
    public void Model_m_ccc_vanduul_helmet_01_IvoSkinFile()
    {
        var args = new string[] { $@"{objectDir}\Objects\Characters\Human\male_v7\armor\ccc\m_ccc_vanduul_helmet_01.skin" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Model_m_ccc_bear_helmet_01_IvoSkinFile()
    {
        var args = new string[] { $@"{objectDir41}\Objects\Characters\Human\male_v7\armor\ccc\m_ccc_bear_helmet_01.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Model_m_qrt_specialist_heavy_arms_01_02_Ivo41()
    {
        // mtlname chunk doesn't match any material file.  Create dummy mats.
        // Skin not mapping
        var args = new string[] {
            $@"{objectDir41}\Objects\Characters\Human\male_v7\armor\qrt\quirinus\m_qrt_specialist_heavy_arms_01_02.skin", "-dds", "-dae",
            "-objectdir", objectDir41 };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var daeObject = colladaData.DaeObject;
        var scene = daeObject.Library_Visual_Scene.Visual_Scene[0];
        var geometry = daeObject.Library_Geometries.Geometry[0];
        var materials = daeObject.Library_Materials;

        Assert.AreEqual("World", scene.Node[0].ID);
        Assert.AreEqual("m_qrt_specialist_heavy_arms_01_02-mesh", geometry.ID);
        Assert.AreEqual(2, geometry.Mesh.Triangles.Length);
        Assert.AreEqual(2, materials.Material.Length);
    }

    [TestMethod]
    public void Model_m_qrt_specialist_heavy_arms_01_cgfm_Ivo41()
    {
        // mtlname chunk doesn't match any material file.  Create dummy mats.
        var args = new string[] {
            $@"{objectDir41}\Objects\Characters\Human\male_v7\armor\qrt\quirinus\m_qrt_specialist_heavy_arms_01.cgf", "-dds", "-dae",
            "-objectdir", objectDir41 };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var daeObject = colladaData.DaeObject;
        var scene = daeObject.Library_Visual_Scene.Visual_Scene[0];
        var geometry = daeObject.Library_Geometries.Geometry[0];
        var materials = daeObject.Library_Materials;

        Assert.AreEqual("m_qrt_specialist_heavy_arms_01", scene.Node[0].ID);
        Assert.AreEqual("m_qrt_specialist_heavy_arms_01-mesh", geometry.ID);
        Assert.AreEqual(2, geometry.Mesh.Triangles.Length);
        Assert.AreEqual(2, materials.Material.Length);
    }

    [TestMethod]
    public void Med_bay_wall_bed_extender_a_Ivo()
    {
        var args = new string[] { $@"{objectDir41}\Objects\Spaceships\Ships\AEGS\Idris_Frigate\interior\med_bay\med_bay_wall_bed_extender_a.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        var visualScene = daeObject.Library_Visual_Scene;
        Assert.AreEqual("med_bay_wall_bed_extender_a", visualScene.Visual_Scene[0].Node[0].Name);
        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("med_bay_wall_bed_extender_a-mesh", geometry.ID);
        Assert.AreEqual(4, geometry.Mesh.Triangles.Length);
    }

    [TestMethod]
    public void MISC_Fury_322()
    {
        var args = new string[] { $@"{objectDir322}\Objects\spaceships\ships\MISC\Fury\MISC_Fury.cga" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir322);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var visualScene = colladaData.DaeObject.Library_Visual_Scene;
        var meshWingTopRight = visualScene.Visual_Scene[0].Node[0].node[0].node[70].node[1].node[0].node[0];
        var matrix = meshWingTopRight.Matrix[0].Value_As_String;
        Assert.AreEqual("1 -0 0 -0.848649 0 1 0.000001 -1.239070 -0 -0.000001 1 0.058854 0 0 0 0", matrix);
    }

    [TestMethod]
    public void MISC_Fury_Ivo()
    {
        var args = new string[] { $@"{objectDir41}\Objects\spaceships\ships\MISC\Fury\MISC_Fury.cga" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var visualScene = colladaData.DaeObject.Library_Visual_Scene;
        var meshWingTopRight = visualScene.Visual_Scene[0].Node[0].node[0].node[72].node[1].node[0].node[0];
        var matrix = meshWingTopRight.Matrix[0].Value_As_String;
        Assert.AreEqual("1 -0 0 -0.848649 0 1 0.000001 -1.239070 -0 -0.000001 1 0.058854 0 0 0 1", matrix);
    }

    [TestMethod]
    public void Mobiglass_Civilian_01_Skin_Collada()
    {
        var args = new string[] { $@"{objectDir41}\Objects\Characters\Mobiglas\f_mobiglas_civilian_01.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(21, materials.Length);

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
            $@"D:\depot\SC4.1\Data\Objects\Characters\Mobiglas\f_mobiglas_civilian_01.skin",
            "-objectdir", objectDir41 };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
        gltfRenderer.Render();
    }

    [TestMethod]
    public void NavyPilotFlightSuit_Ivo()
    {
        var args = new string[] { $@"{objectDir}\Objects\Characters\Human\male_v7\armor\nvy\pilot_flightsuit\m_nvy_pilot_light_helmet_01.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, materialFiles: "m_nvy_pilot_light_no_name_01_01_01", objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    // D:\depot\sc4.1\data\objects\spaceships\turrets\rsi\polaris\rsi_polaris_seataccess_turret_sideleft.cga
    [TestMethod]
    public void Rsi_polaris_seataccess_turret_sideleft_Cga()
    {
        // No geometry file, just the .cga.
        var args = new string[] { $@"{objectDir41}\objects\spaceships\turrets\rsi\polaris\rsi_polaris_seataccess_turret_sideleft.cga" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Teapot_Ivo()
    {
        var args = new string[] { $@"{objectDir41}\Objects\default\teapot.cgf", "-objectdir", objectDir41 };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Teapot_Ivo_Unsplit()
    {
        var args = new string[] { $@"{objectDir41}\Objects\default\teapot.cgf", "-objectdir", objectDir41, "-ut" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Teapot_322()
    {
        var args = new string[] { $@"{objectDir322}\Objects\default\teapot.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir322);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Vgl_Armor_Medium_Helmet_324()
    {
        // Game file has wrong mtl name. It's vgl_armor_medium_helmet_01_01_01 in the game files but actual mtl file is m_vgl_armor_medium_helmet_01_01_01.mtl
        var args = new string[] { $@"{objectDir}\objects\characters\human\male_v7\armor\vgl\m_vgl_armor_medium_helmet_01.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, null, materialFiles: "m_vgl_armor_medium_helmet_01_01_01", objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void Vgl_Armor_Medium_Helmet_41()
    {
        // Game file has wrong mtl name. It's vgl_armor_medium_helmet_01_01_01 in the game files but actual mtl file is m_vgl_armor_medium_helmet_01_01_01.mtl
        var args = new string[] { $@"{objectDir41}\objects\characters\human\male_v7\armor\vgl\m_vgl_armor_medium_helmet_01.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, null, materialFiles: "m_vgl_armor_medium_helmet_01_01_01", objectDir: objectDir41);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }
}
