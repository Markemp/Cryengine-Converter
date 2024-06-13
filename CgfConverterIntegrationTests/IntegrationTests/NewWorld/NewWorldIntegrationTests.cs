using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class NewWorldIntegrationTests
{
    private readonly TestUtils testUtils = new();
    private readonly string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private readonly string objectDir = @"d:\depot\newworld\";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void Npc_Horus_Skel_Chr()
    {
        var args = new string[] { $@"D:\depot\newworld\Objects\characters\npc\npc_horus_skel.chr", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, null);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        var nodes = daeObject.Library_Visual_Scene.Visual_Scene[0].Node;
        Assert.AreEqual(2, nodes.Length);

        Assert.AreEqual("1 0 0 -0 0 1 0 0.022861 0 0 1 -1.073091 0 0 0 1", nodes[0].node[0].Matrix[0].Value_As_String);
        Assert.AreEqual("Pelvis", nodes[0].node[0].Name);
    }

    [TestMethod]
    public void Adiana_Body_Skin()
    {
        var args = new string[] { $@"D:\depot\newworld\objects\characters\npc\natural\adiana\adiana_body.skin", "-dds", "-dae", "-objectdir", objectDir };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, null);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        var nodes = daeObject.Library_Visual_Scene.Visual_Scene[0].Node;
    }

    [TestMethod]
    public void AugerTrap_cgf()
    {
        var args = new string[] { $@"D:\depot\NewWorld\objects\props\augertrap\augertrap.cgf", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, null);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        var nodes = daeObject.Library_Visual_Scene.Visual_Scene[0].Node;
    }

}
