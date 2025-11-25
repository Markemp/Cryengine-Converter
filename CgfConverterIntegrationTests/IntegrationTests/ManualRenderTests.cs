using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Gltf;
using CgfConverter.Renderers.USD;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

/// <summary>
/// Manual render tests that write output files to the source file's directory.
/// Run these from Test Explorer to quickly iterate on renderer changes.
///
/// These tests are NOT for CI - they require game assets and write to disk.
/// Use TestCategory "manual" to exclude from automated runs.
/// </summary>
[TestClass]
[TestCategory("manual")]
public class ManualRenderTests
{
    private readonly ArgsHandler argsHandler = new();
    private readonly string mwoObjectDir = @"d:\depot\mwo";
    private readonly string sc41ObjectDir = @"d:\depot\sc4.1\data";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
    }

    #region MWO Test Files

    [TestMethod]
    public void _50Cal_Necklace_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\Objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Box_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\Objects\default\box.cgf", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Box_Collada()
    {
        RenderToCollada($@"{mwoObjectDir}\Objects\default\box.cgf", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Box_Gltf()
    {
        RenderToGltf($@"{mwoObjectDir}\Objects\default\box.cgf", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_AdderCockpit_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\mechs\cockpit_standard\adder_a_cockpit_standard.cga", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_AdderBody_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\mechs\adder\body\adder_body.cga", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_AdderChr_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\mechs\adder\body\adder.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Mechanic_Collada()
    {
        RenderToCollada($@"{mwoObjectDir}\objects\characters\mechanic\mechanic.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Mechanic_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\characters\mechanic\mechanic.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Mechanic_Gtlf()
    {
        RenderToGltf($@"{mwoObjectDir}\objects\characters\mechanic\mechanic.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Hatchetman_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\mechs\hatchetman\body\hatchetman_body.cga", mwoObjectDir);
    }

    // Add more test files here as needed...

    #endregion

    #region SC test files

    [TestMethod]
    public void SC41_Avenger_USD()
    {
        RenderToUsd($@"{sc41ObjectDir}\Objects\Spaceships\Ships\AEGS\Avenger\AEGS_Avenger.cga", sc41ObjectDir);
    }

    [TestMethod]
    public void SC41_Avenger_Gltf()
    {
        RenderToGltf($@"{sc41ObjectDir}\Objects\Spaceships\Ships\AEGS\Avenger\AEGS_Avenger.cga", sc41ObjectDir);
    }

    #endregion

    #region Helper Methods

    private void RenderToUsd(string inputFile, string objectDir)
    {
        var args = new string[] { inputFile, "-usd", "-objectdir", objectDir };
        argsHandler.ProcessArgs(args);

        var cryData = new CryEngine(inputFile, argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var renderer = new UsdRenderer(argsHandler, cryData);
        renderer.Render();

        var outputPath = Path.ChangeExtension(inputFile, ".usda");
        Assert.IsTrue(File.Exists(outputPath), $"Output file not created: {outputPath}");
    }

    private void RenderToCollada(string inputFile, string objectDir)
    {
        var args = new string[] { inputFile, "-dae", "-objectdir", objectDir };
        argsHandler.ProcessArgs(args);

        var cryData = new CryEngine(inputFile, argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var renderer = new ColladaModelRenderer(argsHandler, cryData);
        renderer.Render();

        var outputPath = Path.ChangeExtension(inputFile, ".dae");
        Assert.IsTrue(File.Exists(outputPath), $"Output file not created: {outputPath}");
    }

    private void RenderToGltf(string inputFile, string objectDir)
    {
        var args = new string[] { inputFile, "-gltf", "-objectdir", objectDir };
        argsHandler.ProcessArgs(args);

        var cryData = new CryEngine(inputFile, argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var renderer = new GltfModelRenderer(argsHandler, cryData, false, false);
        renderer.Render();

        var outputPath = Path.ChangeExtension(inputFile, ".gltf");
        Assert.IsTrue(File.Exists(outputPath), $"Output file not created: {outputPath}");
    }

    #endregion
}
