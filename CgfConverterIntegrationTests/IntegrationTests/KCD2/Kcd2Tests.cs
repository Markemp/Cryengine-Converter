using CgfConverter.Renderers.Collada;
using CgfConverter;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;

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
            $@"{objectDir}\m_head_capon.skin", "-dds", "-dae",
            "-objectdir", $"{objectDir}"
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        testUtils.ValidateColladaXml(colladaData);
    }
}
