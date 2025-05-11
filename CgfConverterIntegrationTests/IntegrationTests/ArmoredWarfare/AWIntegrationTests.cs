using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Gltf;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
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

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
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

        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfData = new(testUtils.argsHandler, cryData, false, false);
        gltfData.GenerateGltfObject();
    }


    [TestMethod]
    public void Chicken_Gltf()
    {
        // Verify bones and materials
        var args = new string[] { $@"d:\depot\armoredwarfare\objects\characters\animals\birds\chicken\chicken.chr", "-dds", "-objectdir", @"d:\depot\armoredwarfare\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
    }

    [TestMethod]
    public void Chicken_Dae()
    {
        // Verify bones and materials
        var args = new string[] { $@"d:\depot\armoredwarfare\objects\characters\animals\birds\chicken\chicken.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
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

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void T62_Turret()
    {
        // material file doesn't have extension and is in same directory as cgf (t-62_turret_t-62.mtl)
        var args = new string[] { $@"{objectDir}\objects\vehicles\afv\fv721-fox\fv721-fox_cannon_milan.cgf" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }
}
