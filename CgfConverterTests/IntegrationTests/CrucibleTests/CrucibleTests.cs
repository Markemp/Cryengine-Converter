using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests.Crucible
{
    [TestClass]
    public class CrucibleTests
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
        public void TechnomancerPillar_ValidateGeometry()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Crucible\technomancerpillar.cgf", "-dds", "-dae" };
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
