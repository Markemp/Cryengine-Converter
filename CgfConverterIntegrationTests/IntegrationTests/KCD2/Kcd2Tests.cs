using CgfConverter.Renderers.Collada;
using CgfConverter;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;
using CgfConverter.Utils;
using System.Linq;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class Kcd2Tests
{
    private readonly TestUtils testUtils = new();
    string userHome;
    private readonly string objectDir = @"d:\depot\kcd2";

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
    public void M_head_capon()
    {
        var args = new string[]
        {
            $@"{objectDir}\objects\characters\humans\male\head\m_head_capon\m_head_capon.skin", "-dds", "-dae"
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);

        // Verify materials
        var mats = daeObject.Library_Materials;
        Assert.AreEqual(5, mats.Material.Length);
    }

    [TestMethod]
    public void Kcd2_MaterialFile()
    {
        var mat = MaterialUtilities.FromFile(@"D:\depot\KCD2\objects\characters\humans\shared\head\lashes.mtl", "lashes", objectDir);
        Assert.IsNotNull(mat);
        Assert.AreEqual("lashes", mat.Name);
        Assert.AreEqual(1, mat.SubMaterials.Length);
    }

    [TestMethod]
    public void Cv_Tachov_1_Smithy_01()
    {
        var args = new string[]
        {
            $@"{objectDir}\Objects\manmade\structures\industrial\smitheries\unique\tachov\cv_tachov_1_smithy_01.cgf", "-dds", "-dae"
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);

        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual(42, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual("tachov_1_smithy_mtl_rock_wall", daeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);
        Assert.AreEqual("#tachov_1_smithy_mtl_rock_wall-material", node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
    }

    [TestMethod]
    public void Cv_Tachov_1_Smithy_02()
    {
        // No material chunks.
        var args = new string[]
        {
            $@"{objectDir}\Objects\manmade\structures\industrial\smitheries\unique\tachov\cv_tachov_1_smithy_02.cgf", "-dds", "-dae"
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);

        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual(1, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual("default_mtl_material0", daeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);
        Assert.AreEqual("#default_mtl_material0-material", node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
    }

    [TestMethod]
    public void Boar_unsplit()
    {
        var args = new string[]
        {
            $@"{objectDir}\Objects\characters\animals\boar\boar.skin", "-dds", "-dae", "-ut", "-objectdir", objectDir
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);

        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual(3, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual("boar_mtl_boar_hair", daeObject.Library_Materials.Material[0].Name);
        Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

        // Textures
        var textures = daeObject.Library_Images.Image;
        Assert.AreEqual(13, textures.Length);
        Assert.AreEqual("boar_mtl_boar_hair_Diffuse", textures[0].Name);
    }
}
