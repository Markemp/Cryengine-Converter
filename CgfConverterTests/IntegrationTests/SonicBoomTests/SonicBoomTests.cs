using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.SonicBoom
{
    [TestClass]
    public class SonicBoomTests
    {
        private readonly TestUtils testUtils = new TestUtils();

        [TestInitialize]
        public void Initialize()
        {
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            testUtils.GetSchemaSet();
        }

        [TestMethod]
        public void Checkpoint_ValidateGeometry()
        {
            var args = new string[] { @"..\..\ResourceFiles\SonicBoom\checkpoint.cgf", "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            Assert.AreEqual((uint)11, cryData.Models[0].NumChunks);
            Assert.AreEqual(ChunkTypeEnum.Node, cryData.Models[0].ChunkMap[22].ChunkType);
            var datastream = cryData.Models[0].ChunkMap[16] as ChunkDataStream_80000800;
            Assert.AreEqual((uint)8, datastream.BytesPerElement);
            Assert.AreEqual((uint)96, datastream.NumElements);
            Assert.AreEqual(-1.390625, datastream.Vertices[0].x, testUtils.delta);
            Assert.AreEqual(1.9326171875, datastream.Vertices[0].y, testUtils.delta);
            Assert.AreEqual(1.9189453125, datastream.Vertices[0].z, testUtils.delta);

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(1, actualMaterialsCount);
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void JungleChase_ValidateGeometry()
        {
            var args = new string[] { @"..\..\ResourceFiles\SonicBoom\jungle_chase.cgf", "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            Assert.AreEqual((uint)24, cryData.Models[0].NumChunks);
            Assert.AreEqual(ChunkTypeEnum.Node, cryData.Models[0].ChunkMap[0x2F].ChunkType);
            var datastream = cryData.Models[0].ChunkMap[0x28] as ChunkDataStream_80000800;
            Assert.AreEqual((uint)12, datastream.BytesPerElement);
            Assert.AreEqual((uint)46244, datastream.NumElements);
            Assert.AreEqual(-130.51466369628906, datastream.Vertices[0].x, testUtils.delta);
            Assert.AreEqual(36.355838775634766, datastream.Vertices[0].y, testUtils.delta);
            Assert.AreEqual(24.204965591430664, datastream.Vertices[0].z, testUtils.delta);

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(12, actualMaterialsCount);
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void JungleChaseb_ValidateGeometry()
        {
            var args = new string[] { @"..\..\ResourceFiles\SonicBoom\jungle_chase_b.cgf", "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            Assert.AreEqual((uint)11, cryData.Models[0].NumChunks);
            Assert.AreEqual(ChunkTypeEnum.Node, cryData.Models[0].ChunkMap[22].ChunkType);
            var datastream = cryData.Models[0].ChunkMap[16] as ChunkDataStream_801;
            Assert.AreEqual((uint)8, datastream.BytesPerElement);
            Assert.AreEqual((uint)96, datastream.NumElements);
            Assert.AreEqual(-1.390625, datastream.Vertices[0].x, testUtils.delta);
            Assert.AreEqual(1.9326171875, datastream.Vertices[0].y, testUtils.delta);
            Assert.AreEqual(1.9189453125, datastream.Vertices[0].z, testUtils.delta);

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(1, actualMaterialsCount);
            testUtils.ValidateColladaXml(colladaData);
        }
    }
}
