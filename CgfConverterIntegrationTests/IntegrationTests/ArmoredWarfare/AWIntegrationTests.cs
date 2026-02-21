using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Gltf;
using CgfConverter.Renderers.USD;
using CgfConverter.Renderers.USD.Models;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class ArmoredWarfareIntegrationTests
{
    private readonly TestUtils testUtils = new();
    private readonly string objectDir = @"d:\depot\armoredwarfare";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void Box_Collada()
    {
        var args = new string[] { $@"{objectDir}\Objects\default\primitive_box.cgf", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        // Visual Scene checks
        var boxNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("box", boxNode.ID);
        Assert.AreEqual(ColladaNodeType.NODE, boxNode.Type);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0", boxNode.Matrix[0].Value_As_String);
        Assert.AreEqual("#helper_mtl_material0-material", boxNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("helper_mtl_material0-material", boxNode.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);

        // Geometry Checks
        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("box-mesh", geometry.ID);
        Assert.AreEqual(1, daeObject.Library_Geometries.Geometry.Length);
        var mesh = geometry.Mesh;
        Assert.AreEqual(4, mesh.Source.Length);
        Assert.AreEqual(2, mesh.Triangles.Length);
        Assert.AreEqual(12, mesh.Triangles[0].Count);
        Assert.AreEqual(0, mesh.Triangles[1].Count);

        // Materials Checks
        var mats = daeObject.Library_Materials;
        Assert.AreEqual(2, mats.Material.Length);
        Assert.AreEqual("helper_mtl_material0", mats.Material[0].Name);
        Assert.AreEqual("helper_mtl_material0-material", mats.Material[0].ID);
        Assert.AreEqual("#helper_mtl_material0-effect", mats.Material[0].Instance_Effect.URL);
        Assert.AreEqual("helper_mtl_material1", mats.Material[1].Name);
        var boundMaterials = boxNode.Instance_Geometry[0].Bind_Material;
        Assert.AreEqual("#helper_mtl_material0-material", boundMaterials[0].Technique_Common.Instance_Material[0].Target);
        Assert.AreEqual("helper_mtl_material0-material", boundMaterials[0].Technique_Common.Instance_Material[0].Symbol);
        Assert.AreEqual("#helper_mtl_material1-material", boundMaterials[0].Technique_Common.Instance_Material[1].Target);
        Assert.AreEqual("helper_mtl_material1-material", boundMaterials[0].Technique_Common.Instance_Material[1].Symbol);

    }

    [TestMethod]
    public void Box_Gltf()
    {
        var args = new string[] { $@"{objectDir}\Objects\default\primitive_box.cgf", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        CryEngine cryData = new(args[0], testUtils.argsHandler.Args.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfData = new(testUtils.argsHandler.Args, cryData);
        gltfData.GenerateGltfObject();
    }


    [TestMethod]
    public void Chicken_Gltf()
    {
        // Verify bones and materials
        var args = new string[] { $@"d:\depot\armoredwarfare\objects\characters\animals\birds\chicken\chicken.chr", "-dds", "-objectdir", @"d:\depot\armoredwarfare\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler.Args, cryData);
        var gltfData = gltfRenderer.GenerateGltfObject();
    }

    [TestMethod]
    public void Chicken_Dae()
    {
        // Verify bones and materials
        var args = new string[] { $@"d:\depot\armoredwarfare\objects\characters\animals\birds\chicken\chicken.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();

        var daeData = colladaData.DaeObject;

        // Validate Collada XML
        testUtils.ValidateColladaXml(colladaData);

        // Test Library Materials
        Assert.IsNotNull(daeData.Library_Materials);
        Assert.AreEqual(2, daeData.Library_Materials.Material.Length);

        var material1 = daeData.Library_Materials.Material[0];
        Assert.AreEqual("Chicken_mtl_collision-material", material1.ID);
        Assert.AreEqual("Chicken_mtl_collision", material1.Name);

        var material2 = daeData.Library_Materials.Material[1];
        Assert.AreEqual("Chicken_mtl_Chicken-material", material2.ID);
        Assert.AreEqual("Chicken_mtl_Chicken", material2.Name);

        // Test Library Effects
        Assert.IsNotNull(daeData.Library_Effects);
        Assert.AreEqual(2, daeData.Library_Effects.Effect.Length);

        var effect1 = daeData.Library_Effects.Effect[0];
        Assert.AreEqual("Chicken_mtl_collision-effect", effect1.ID);
        Assert.AreEqual("Chicken_mtl_collision", effect1.Name);

        var effect2 = daeData.Library_Effects.Effect[1];
        Assert.AreEqual("Chicken_mtl_Chicken-effect", effect2.ID);
        Assert.AreEqual("Chicken_mtl_Chicken", effect2.Name);

        // Test Library Images
        Assert.IsNotNull(daeData.Library_Images);
        Assert.AreEqual(3, daeData.Library_Images.Image.Length);

        var image1 = daeData.Library_Images.Image[0];
        Assert.AreEqual("Chicken_mtl_Chicken_Diffuse", image1.ID);

        var image2 = daeData.Library_Images.Image[1];
        Assert.AreEqual("Chicken_mtl_Chicken_Specular", image2.ID);

        var image3 = daeData.Library_Images.Image[2];
        Assert.AreEqual("Chicken_mtl_Chicken_Normals", image3.ID);

        // Test Library Geometries
        Assert.IsNotNull(daeData.Library_Geometries);
        Assert.AreEqual(1, daeData.Library_Geometries.Geometry.Length);

        var geometry = daeData.Library_Geometries.Geometry[0];
        Assert.AreEqual("Chicken-mesh", geometry.ID);
        Assert.AreEqual("Chicken", geometry.Name);

        var mesh = geometry.Mesh;
        Assert.IsNotNull(mesh);
        Assert.AreEqual(4, mesh.Source.Length); // Should have sources for pos, norm, UV, color
        Assert.AreEqual(4, mesh.Vertices.Input.Length);

        // Test triangle counts
        Assert.AreEqual(2, mesh.Triangles.Length);
        Assert.AreEqual(1006, mesh.Triangles[0].Count); // Main mesh triangles
        Assert.AreEqual(2, mesh.Triangles[1].Count); // Collision triangles

        // Test Library Controllers
        Assert.IsNotNull(daeData.Library_Controllers);
        Assert.AreEqual(1, daeData.Library_Controllers.Controller.Length);

        var controller = daeData.Library_Controllers.Controller[0];
        Assert.AreEqual("Controller", controller.ID);
        Assert.IsNotNull(controller.Skin);
        Assert.AreEqual("Controller-joints", controller.Skin.Source[0].ID);

        // Test joints count
        var jointsAccessor = controller.Skin.Source
            .First(s => s.ID == "Controller-joints")
            .Technique_Common;

        // Test Library Visual Scene
        Assert.IsNotNull(daeData.Library_Visual_Scene);
        Assert.AreEqual(1, daeData.Library_Visual_Scene.Visual_Scene.Length);

        var visualScene = daeData.Library_Visual_Scene.Visual_Scene[0];
        Assert.AreEqual("Scene", visualScene.ID);

        // Test Visual Scene hierarchy
        Assert.AreEqual(2, visualScene.Node.Length); // Bip01_ and Chicken nodes

        var rootJoint = visualScene.Node[0];
        Assert.AreEqual("Bip01_", rootJoint.Name);
        Assert.AreEqual("JOINT", rootJoint.Type.ToString());

        var chickenNode = visualScene.Node[1];
        Assert.AreEqual("Chicken", chickenNode.Name);
        Assert.AreEqual("NODE", chickenNode.Type.ToString());
        Assert.IsNotNull(chickenNode.Instance_Controller);

        // Test skeleton binding
        Assert.AreEqual(1, chickenNode.Instance_Controller[0].Skeleton.Length);

        // Test bone hierarchy (Bip01_ joint has expected children)
        var pelvisJoint = rootJoint.node[0];
        Assert.AreEqual("Bip01_Pelvis", pelvisJoint.Name);
        Assert.AreEqual("JOINT", pelvisJoint.Type.ToString());

        // Verify Pelvis joint children count (should have 5 children)
        Assert.AreEqual(4, pelvisJoint.node.Length);

        // Test specific joints
        var leftLegThigh = pelvisJoint.node[0];
        Assert.AreEqual("Bip01_LLegThigh", leftLegThigh.Name);

        var rightLegThigh = pelvisJoint.node[1];
        Assert.AreEqual("Bip01_RLegThigh", rightLegThigh.Name);

        var spine1 = pelvisJoint.node[2];
        Assert.AreEqual("Bip01_Spine1", spine1.Name);

        var tail = pelvisJoint.node[3];
        Assert.AreEqual("Bip01_Tail", tail.Name);

        // Test asset information
        Assert.IsNotNull(daeData.Asset);
        Assert.AreEqual("meter", daeData.Asset.Unit.Name);
        Assert.AreEqual(1, daeData.Asset.Unit.Meter);
        Assert.AreEqual("Z_UP", daeData.Asset.Up_Axis);
        Assert.AreEqual("Chicken", daeData.Asset.Title);
    }

    [TestMethod]
    public void Fv721_fox_cannon_l21rarden()
    {
        // spec map channel and gloss map channel uses floats instead of ints.
        var args = new string[] { $@"{objectDir}\objects\vehicles\afv\fv721-fox\fv721-fox_cannon_milan.cgf", "-dds", "-dae", "-objectdir", @"d:\depot\armoredwarfare" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void T62_Turret()
    {
        // material file doesn't have extension and is in same directory as cgf (t-62_turret_t-62.mtl)
        var args = new string[] { $@"{objectDir}\objects\vehicles\afv\fv721-fox\fv721-fox_cannon_milan.cgf" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void Chicken_Usd()
    {
        // Verify skeleton, joints, and skinning for chicken model (ChunkController_829)
        var args = new string[] { $@"d:\depot\armoredwarfare\objects\characters\animals\birds\chicken\chicken.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        // Generate USD
        UsdRenderer usdRenderer = new(testUtils.argsHandler.Args, cryData);
        var usdDoc = usdRenderer.GenerateUsdObject();

        // Serialize to string for inspection
        var serializer = new UsdSerializer();
        using var writer = new StringWriter();
        serializer.Serialize(usdDoc, writer);
        var usdOutput = writer.ToString();

        // Verify basic structure
        Assert.IsTrue(usdOutput.Contains("def SkelRoot \"Armature\""), "Should have SkelRoot");
        Assert.IsTrue(usdOutput.Contains("def Skeleton \"Skeleton\""), "Should have Skeleton");

        // Verify skeleton data arrays
        Assert.IsTrue(usdOutput.Contains("uniform token[] joints"), "Should have joints array");
        Assert.IsTrue(usdOutput.Contains("uniform matrix4d[] bindTransforms"), "Should have bindTransforms");
        Assert.IsTrue(usdOutput.Contains("uniform matrix4d[] restTransforms"), "Should have restTransforms");

        // Verify joint hierarchy (15 joints total)
        Assert.IsTrue(usdOutput.Contains("\"Bip01_\""), "Should have root joint Bip01_");
        Assert.IsTrue(usdOutput.Contains("\"Bip01_/Bip01_Pelvis\""), "Should have Pelvis joint");
        Assert.IsTrue(usdOutput.Contains("\"Bip01_/Bip01_Pelvis/Bip01_LLegThigh\""), "Should have LLegThigh joint");
        Assert.IsTrue(usdOutput.Contains("\"Bip01_/Bip01_Pelvis/Bip01_RLegThigh\""), "Should have RLegThigh joint");
        Assert.IsTrue(usdOutput.Contains("\"Bip01_/Bip01_Pelvis/Bip01_Spine1\""), "Should have Spine1 joint");
        Assert.IsTrue(usdOutput.Contains("\"Bip01_/Bip01_Pelvis/Bip01_Tail\""), "Should have Tail joint");
        Assert.IsTrue(usdOutput.Contains("\"Bip01_/Bip01_Pelvis/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Head\""), "Should have Head joint");

        // Verify mesh skinning attributes
        Assert.IsTrue(usdOutput.Contains("primvars:skel:geomBindTransform"), "Should have geomBindTransform");
        Assert.IsTrue(usdOutput.Contains("primvars:skel:jointIndices"), "Should have jointIndices");
        Assert.IsTrue(usdOutput.Contains("primvars:skel:jointWeights"), "Should have jointWeights");
        Assert.IsTrue(usdOutput.Contains("rel skel:skeleton"), "Should have skeleton relationship");
        Assert.IsTrue(usdOutput.Contains("</root/Armature/Skeleton>"), "Should reference skeleton path");

        // Verify mesh geometry
        Assert.IsTrue(usdOutput.Contains("def Mesh \"Chicken\""), "Should have Chicken mesh");
    }

    [TestMethod]
    public void Chicken_WalkAnimation_Usd()
    {
        // Verify animation export for chicken walk loop (ChunkController_829 animation)
        // The chrparams file at chicken.chrparams defines animations that are auto-loaded
        // When multiple animations exist, they're exported to separate files
        var modelFile = $@"d:\depot\armoredwarfare\objects\characters\animals\birds\chicken\chicken.chr";
        var modelDir = Path.GetDirectoryName(modelFile)!;

        var args = new string[] { modelFile, "-usd", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(modelFile, testUtils.argsHandler.Args.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        // Animations are loaded automatically via chrparams
        Assert.IsNotNull(cryData.CafAnimations, "Should have CAF animations loaded");
        Assert.IsTrue(cryData.CafAnimations.Count >= 2, "Should have at least 2 CAF animations (walk_loop, idle01)");

        // Verify walk_loop animation is present
        var walkAnimation = cryData.CafAnimations.FirstOrDefault(a => a.Name.Contains("walk_loop"));
        Assert.IsNotNull(walkAnimation, "Should have walk_loop animation");

        // Verify animation has bone tracks (14 bones animated, Tail excluded)
        Assert.AreEqual(14, walkAnimation.BoneTracks.Count, "Walk loop should have 14 bone tracks");

        // Generate and write USD (animations go to separate files)
        UsdRenderer usdRenderer = new(testUtils.argsHandler.Args, cryData);
        usdRenderer.Render();

        // Verify animation file was created
        var walkAnimFile = Path.Combine(modelDir, "chicken_anim_walk_loop.usda");
        Assert.IsTrue(File.Exists(walkAnimFile), $"Animation file should exist: {walkAnimFile}");

        // Read the animation file to verify content
        var animContent = File.ReadAllText(walkAnimFile);

        // Verify animation structure
        Assert.IsTrue(animContent.Contains("def SkelAnimation"), "Should have SkelAnimation");
        Assert.IsTrue(animContent.Contains("walk_loop"), "Should have walk_loop animation name");

        // Verify animation has timeSamples
        Assert.IsTrue(animContent.Contains("translations.timeSamples"), "Should have translation timeSamples");
        Assert.IsTrue(animContent.Contains("rotations.timeSamples"), "Should have rotation timeSamples");
        Assert.IsTrue(animContent.Contains("scales.timeSamples"), "Should have scale timeSamples");

        // Verify animation time range (31 frames, 0-30)
        Assert.IsTrue(animContent.Contains("startTimeCode = 0"), "Should start at frame 0");
        Assert.IsTrue(animContent.Contains("endTimeCode = 30"), "Should end at frame 30");

        // Verify animation joints (14 animated joints, excludes Tail)
        Assert.IsTrue(animContent.Contains("\"Bip01_/Bip01_Pelvis\""), "Animation should have Pelvis joint");
        Assert.IsTrue(animContent.Contains("\"Bip01_/Bip01_Pelvis/Bip01_LLegThigh\""), "Animation should have LLegThigh joint");

        // Verify skeleton reference
        Assert.IsTrue(animContent.Contains("rel skel:animationSource"), "Should have animationSource relationship");

        // Verify pelvis translation values are reasonable (not doubled due to previous bug)
        // Frame 0 pelvis: (-0, 0.007701, 0.102881) - Z should be ~0.10, not ~0.22
        Assert.IsTrue(animContent.Contains("0.102881") || animContent.Contains("0.10288"),
            "Pelvis Z translation at frame 0 should be ~0.102881 (not doubled)");

        // Verify frame 7 pelvis Z (highest value ~0.120)
        Assert.IsTrue(animContent.Contains("0.120023") || animContent.Contains("0.12002"),
            "Pelvis Z translation at frame 7 should be ~0.120023");
    }
}
