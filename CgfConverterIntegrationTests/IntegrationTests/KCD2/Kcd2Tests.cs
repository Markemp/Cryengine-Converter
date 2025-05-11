using CgfConverter.Renderers.Collada;
using CgfConverter;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;
using CgfConverter.Utils;

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
}
