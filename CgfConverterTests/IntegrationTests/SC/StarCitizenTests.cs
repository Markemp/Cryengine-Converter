using CgfConverter;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;

namespace CgfConverterTests.IntegrationTests.SC
{
    [TestClass]
    public class StarCitizenTests
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
        public void BehrRifle_312IvoChrFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar.chr", "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            var colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject(); 
            var daeObject = colladaData.DaeObject;

            Assert.AreEqual(1, cryData.Materials.Count);
        }

        [TestMethod]
        public void BehrRifle_312IvoSkinFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar_parts.skin", "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            var colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
        }

        [TestMethod]
        public void AEGS_Avenger_IntegrationTest()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\AEGS_Avenger.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\SC\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            var colladaData = new COLLADA(testUtils.argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            colladaData.GenerateDaeObject();
            // Make sure Rotations are still right
            const string frontLGDoorLeftMatrix = "1.000000 0.000000 0.000000 -0.300001 0.000000 -0.938131 -0.346280 0.512432 0.000000 0.346280 -0.938131 -1.835138 0.000000 0.000000 0.000000 1.000000";
            var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
            Assert.AreEqual("Nose", noseNode.ID);
            Assert.AreEqual("Front_LG_Door_Left", noseNode.node[28].ID);
            Assert.AreEqual(frontLGDoorLeftMatrix, noseNode.node[28].Matrix[0].Value_As_String);

        }

        [TestMethod]
        public void SC_hangar_asteroid_controlroom_fan()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\hangar_asteroid_controlroom_fan.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void SC_BehrRifle_34()
        {
            var args = new string[] { 
                $@"{userHome}\OneDrive\ResourceFiles\SC\brfl_fps_behr_p4ar_parts_3.4.skin", 
                "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            var controllers = colladaData.DaeObject.Library_Controllers.Controller;
            var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
            Assert.AreEqual(1, controllers.Length);
            Assert.AreEqual(1, geometries.Length);

            var meshes = geometries[0].Mesh;
            Assert.AreEqual(4, meshes.Source);

            testUtils.ValidateColladaXml(colladaData);
        }
    }
}
