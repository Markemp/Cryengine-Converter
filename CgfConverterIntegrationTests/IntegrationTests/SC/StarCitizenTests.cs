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
        public void AEGS_Vanguard_LandingGear_Front_IvoFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\new_skin_format\Avenger_Landing_Gear\AEGS_Vanguard_LandingGear_Front.skin", "-dds", "-dae" };

            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            var colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            var daeObject = colladaData.DaeObject;
        }

        [TestMethod]
        public void M_ccc_vanduul_helmet_01_312IvoSkinFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\m_ccc_vanduul_helmet_01.skin", "-dds", "-dae" };

            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            var colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            var daeObject = colladaData.DaeObject;
        }

        [TestMethod]
        public void M_ccc_heavy_armor_helmet_01Skin_Colors2()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\m_ccc_heavy_armor_helmet_01.skin", "-dds", "-dae" };

            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            var colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            var daeObject = colladaData.DaeObject;
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

            Assert.AreEqual(17, cryData.Materials.Count);

            testUtils.ValidateColladaXml(colladaData);
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
            var daeObject = colladaData.DaeObject;

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void AEGS_Avenger()
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
            const string frontLGDoorLeftMatrix = "1.000000 0.000000 0.000000 -0.300001 0.000000 -0.938131 -0.346280 0.512432 0.000000 0.346280 -0.938131 -1.835138 0.000000 0.000000 0.000000 0.000000";
            var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
            Assert.AreEqual("Nose", noseNode.ID);
            Assert.AreEqual("Front_LG_Door_Left", noseNode.node[28].ID);
            Assert.AreEqual(frontLGDoorLeftMatrix, noseNode.node[28].Matrix[0].Value_As_String);

            testUtils.ValidateColladaXml(colladaData);
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

            var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
            Assert.AreEqual(3, geometries.Length);

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

            var mesh = geometries[0].Mesh;
            Assert.AreEqual(4, mesh.Source.Length);
            Assert.AreEqual("brfl_fps_behr_p4ar_parts-vertices", mesh.Vertices.ID);
            Assert.AreEqual(9, mesh.Triangles.Length);
            Assert.AreEqual(78, mesh.Triangles[0].Count);
            Assert.AreEqual(134, mesh.Triangles[8].Count);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void BehrRifle_312_NonIVO()
        {
            var args = new string[] {
                $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar_body.cgf",
                "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            // Geometry Library checks
            var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
            Assert.AreEqual(1, geometries.Length);

            var mesh = geometries[0].Mesh;
            Assert.AreEqual(4, mesh.Source.Length);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-vertices", mesh.Vertices.ID);
            Assert.AreEqual(13, mesh.Triangles.Length);
            Assert.AreEqual(84, mesh.Triangles[0].Count);
            Assert.AreEqual(1460, mesh.Triangles[8].Count);

            var vertices = mesh.Source[0];
            var normals = mesh.Source[1];
            var uvs = mesh.Source[2];
            var colors = mesh.Source[3];
            Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-pos", vertices.ID);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-pos", vertices.Name);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-norm", normals.ID);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-norm", normals.Name);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-UV", uvs.ID);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-UV", uvs.Name);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-color", colors.ID);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-color", colors.Name);
            Assert.AreEqual(56058, vertices.Float_Array.Count);
            Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-pos-array", vertices.Float_Array.ID);
            Assert.IsTrue(vertices.Float_Array.Value_As_String.StartsWith("-0.020622 0.180945 0.097055 -0.020622 0.178238 0.092718 -0.020622 0.175470 0.097055 -0.020622 0.175408 0.105175 -0.020622"));
            Assert.AreEqual((uint)18686, vertices.Technique_Common.Accessor.Count);
            Assert.AreEqual((uint)3, vertices.Technique_Common.Accessor.Stride);
            Assert.AreEqual(56058, normals.Float_Array.Count);
            Assert.AreEqual((uint)18686, normals.Technique_Common.Accessor.Count);
            Assert.AreEqual((uint)3, normals.Technique_Common.Accessor.Stride);
            Assert.AreEqual(37372, uvs.Float_Array.Count);
            Assert.AreEqual((uint)18686, uvs.Technique_Common.Accessor.Count);
            Assert.AreEqual((uint)2, uvs.Technique_Common.Accessor.Stride);
            Assert.AreEqual(74744, colors.Float_Array.Count);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void DRAK_Buccaneer_Landing_Gear_Front_Skin()
        {
            var args = new string[] {
                $@"{userHome}\OneDrive\ResourceFiles\SC\DRAK_Buccaneer_Landing_Gear_Front_Skin.skin",
                "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            // Geometry Library checks
            var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
            Assert.AreEqual(1, geometries.Length);
        }

        [TestMethod]
        public void Mobiglass()
        {
            var args = new string[] {
                $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\f_mobiglas_civilian_01.skin",
                "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            // Geometry Library checks
            var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
            Assert.AreEqual(1, geometries.Length);

        }
    }
}
