using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Threading;
using System;
using CgfConverter.Renderers.Collada;
using CgfConverter;
using System.Linq;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class EvolveIntegrationTests
{
    private readonly TestUtils testUtils = new();
    private readonly string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void Goliath_Collada()
    {
        var args = new string[] { $@"D:\depot\Evolve\objects\characters\monsters\goliath\goliath1_body.skin", "-dds", "-dae", "-objectdir", @"d:\depot\evolve" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void Evolve_griffin_skin_NoMaterialFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Evolve\griffin.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        Assert.AreEqual(15, actualMaterialsCount);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void Evolve_griffin_menu_harpoon_skin_NoMaterialFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Evolve\griffin_menu_harpoon.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        Assert.AreEqual(2, actualMaterialsCount);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void Evolve_griffin_fp_skeleton_chr_NoMaterialFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Evolve\griffin_fp_skeleton.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        Assert.AreEqual(3, actualMaterialsCount);

        testUtils.ValidateColladaXml(colladaData);
    }
}
