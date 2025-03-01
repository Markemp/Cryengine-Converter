using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Gltf;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
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
        var args = new string[] { $@"d:\depot\armoredwarfare\objects\characters\animals\birds\chicken\chicken.chr", "-dds", "-objectdir", @"d:\depot\armoredwarfare\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void T62_Turret()
    {
        // material file doesn't have extension and is in same directory as cgf (t-62_turret_t-62.mtl)
        var args = new string[] { $@"D:\depot\ArmoredWarfare\objects\vehicles\mbt\t-62\t-62_turret_t-62.cgf", "-dds", "-dae", "-objectdir", @"d:\depot\armoredwarfare\" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }
}
