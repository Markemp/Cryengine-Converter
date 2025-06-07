using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Gltf;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class CgfConverterIntegrationTests
{
    private readonly TestUtils testUtils = new TestUtils();
    string userHome;

    [TestInitialize]
    public void Initialize()
    {
        testUtils.errors = new List<string>();
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void SimpleCubeSchemaValidation()
    {
        testUtils.ValidateXml($@"{userHome}\OneDrive\ResourceFiles\simple_cube.dae");
        Assert.AreEqual(0, testUtils.errors.Count);
    }

    [TestMethod]
    public void SimpleCubeSchemaValidation_BadColladaWithOneError()
    {
        testUtils.ValidateXml($@"{userHome}\OneDrive\ResourceFiles\simple_cube_bad.dae");
        Assert.AreEqual(1, testUtils.errors.Count);
    }

    [TestMethod]
    public void UnknownSource_forest_ruin()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\forest_ruin.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void GhostSniper3_raquel_eyeoverlay_skin()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Test01\raquel_eyeoverlay.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Test01\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, objectDir: @"..\..\ResourceFiles\Test01\");
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        Assert.AreEqual(6, actualMaterialsCount);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void Prey_Dahl_GenMaleBody01_MaterialFileFound()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Prey\Dahl_GenMaleBody01.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Prey\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        Assert.AreEqual(1, actualMaterialsCount);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void Prey_Dahl_GenMaleBody01_MaterialFileNotAvailable()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Prey\Dahl_GenMaleBody01.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        Assert.AreEqual(1, actualMaterialsCount);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void UnknownSource_osv_96_muzzle_brake_01_fp_NoMaterialFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\osv_96_muzzle_brake_01_fp.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void UnknownSource_spriggan_proto_mesh_skin_NoMaterialFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\spriggan_proto_mesh.skin" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        Assert.AreEqual((uint)41, cryData.Models[0].NumChunks);
        Assert.AreEqual(ChunkType.Node, cryData.Models[0].ChunkMap[47].ChunkType);
        var datastream = cryData.Models[0].ChunkMap[40] as ChunkDataStream;

        Assert.AreEqual((uint)12, datastream.BytesPerElement);
        Assert.AreEqual((uint)22252, datastream.NumElements);

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void UnknownSource_spriggan_proto_skel_chr_NoMaterialFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\spriggan_proto_skel.chr" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        testUtils.ValidateColladaXml(colladaData);
    }

    // Model appears to be broken.  Assigns 3 materials, but only 2 materials in mtlname chunks
    [TestMethod]
    public void Cnylgt_marauder_Collada_NoMaterialFile()
    {
        var args = new string[] { $@"d:\depot\mwo\Objects\purchasable\cockpit_hanging\cnylgt\cnylgt_marauder.cga", "cnylgt_off.mtl,cnylgt_on.mtl" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem, materialFiles: "cnylgt_off.mtl,cnylgt_on.mtl", objectDir: @"d:\depot\mwo");
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
    }

    [TestMethod]
    public void Cnylgt_marauder_Gltf_NoMaterialFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\cnylgt_marauder.cga", "-gltf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var objectData = gltfRenderer.GenerateGltfObject();

        // Validate Scene
        Assert.AreEqual(1, objectData.Scenes.Count);
        Assert.AreEqual("Scene", objectData.Scenes[0].Name);

        // Validate Nodes
        Assert.AreEqual(1, objectData.Nodes.Count);
        Assert.AreEqual("cnylgt_marauder", objectData.Nodes[0].Name);
        Assert.AreEqual(0, objectData.Nodes[0].Mesh);

        // Validate Meshes
        Assert.AreEqual(3, objectData.BufferViews.Count);
        Assert.AreEqual(32004, objectData.BufferViews[0].ByteLength);
        Assert.AreEqual(32004, objectData.BufferViews[1].ByteLength);
        Assert.AreEqual(29832, objectData.BufferViews[2].ByteLength);
    }
}

