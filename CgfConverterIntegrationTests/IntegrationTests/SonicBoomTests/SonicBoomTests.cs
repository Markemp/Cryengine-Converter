using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;
using CgfConverter.Renderers.Collada;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class SonicBoomTests
{
    private readonly TestUtils testUtils = new TestUtils();
    string userHome;

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
    public void Checkpoint_ValidateGeometry()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SonicBoom\checkpoint.cgf", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        Assert.AreEqual((uint)17, cryData.Models[0].NumChunks);
        Assert.AreEqual(ChunkType.Node, cryData.Models[0].ChunkMap[33].ChunkType);
        var datastream = cryData.Models[0].ChunkMap[23] as ChunkDataStream;
        Assert.AreEqual((uint)12, datastream.BytesPerElement);
        Assert.AreEqual((uint)629, datastream.NumElements);
        Assert.AreEqual(-0.1071479171, datastream.Vertices[0].X, TestUtils.delta);
        Assert.AreEqual(0, datastream.Vertices[0].Y, TestUtils.delta);
        Assert.AreEqual(0.983217179775238, datastream.Vertices[0].Z, TestUtils.delta);

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(2, actualMaterialsCount);
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void JungleChase_ValidateGeometry()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SonicBoom\jungle_chase.cgf", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        Assert.AreEqual((uint)24, cryData.Models[0].NumChunks);
        Assert.AreEqual(ChunkType.Node, cryData.Models[0].ChunkMap[47].ChunkType);
        var datastream = cryData.Models[0].ChunkMap[40] as ChunkDataStream;
        Assert.AreEqual((uint)12, datastream.BytesPerElement);
        Assert.AreEqual((uint)46244, datastream.NumElements);
        Assert.AreEqual(-130.51466369, datastream.Vertices[0].X, TestUtils.delta);
        Assert.AreEqual(36.3558387756, datastream.Vertices[0].Y, TestUtils.delta);
        Assert.AreEqual(24.2049655914, datastream.Vertices[0].Z, TestUtils.delta);

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(20, actualMaterialsCount);
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void JungleChaseb_ValidateGeometry()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SonicBoom\jungle_chase_b.cgf", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        Assert.AreEqual((uint)30, cryData.Models[0].NumChunks);
        Assert.AreEqual(ChunkType.Node, cryData.Models[0].ChunkMap[59].ChunkType);
        var datastream = cryData.Models[0].ChunkMap[52] as ChunkDataStream;
        Assert.AreEqual((uint)12, datastream.BytesPerElement);
        Assert.AreEqual((uint)60794, datastream.NumElements);
        Assert.AreEqual(104.20726013183594, datastream.Vertices[0].X, TestUtils.delta);
        Assert.AreEqual(137.044921875, datastream.Vertices[0].Y, TestUtils.delta);
        Assert.AreEqual(24.923294067382812, datastream.Vertices[0].Z, TestUtils.delta);

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(20, actualMaterialsCount);
        testUtils.ValidateColladaXml(colladaData);
    }
}
