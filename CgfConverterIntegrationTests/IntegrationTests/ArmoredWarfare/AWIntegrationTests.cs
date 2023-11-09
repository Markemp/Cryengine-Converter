using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Gltf;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
public class ArmoredWarfareIntegrationTests
{
    private readonly TestUtils testUtils = new();

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
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
        colladaData.Render();
    }

    [TestMethod]
    public void Chicken_Dae_WalkAnim()
    {
        var args = new string[] { $@"d:\depot\armoredwarfare\animations\animals\birds\chicken\walk.caf", "-dds", "-dae", "-objectdir", @"d:\depot\armoredwarfare\" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.Render();
    }
}
