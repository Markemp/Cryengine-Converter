using CgfConverter;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;
using CgfConverter.Renderers.Collada;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class CrysisIntegrationTests
{
    private readonly TestUtils testUtils = new();
    private readonly string objectDir = @"d:\depot\crysis";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void AlienBase()
    {
        var args = new string[] { $@"{objectDir}\objects\characters\alien\alienbase\alienbase.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void FemaleArcheologist()
    {
        var args = new string[] { $@"{objectDir}\objects\characters\human\US\Archaeologist\Archaeologist_female_01.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, objectDir: objectDir);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }
}
