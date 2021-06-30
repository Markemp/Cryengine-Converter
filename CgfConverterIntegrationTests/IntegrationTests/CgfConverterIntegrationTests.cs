using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests
{
    [TestClass]
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
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(13, actualMaterialsCount);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void GhostSniper3_raquel_eyeoverlay_skin()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Test01\raquel_eyeoverlay.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Test01\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
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
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
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
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(1, actualMaterialsCount);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Evolve_griffin_skin_NoMaterialFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Evolve\griffin.skin" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Evolve_griffin_menu_harpoon_skin_NoMaterialFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Evolve\griffin_menu_harpoon.skin" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Evolve_griffin_fp_skeleton_chr_NoMaterialFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Evolve\griffin_fp_skeleton.chr" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void UnknownSource_osv_96_muzzle_brake_01_fp_NoMaterialFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\osv_96_muzzle_brake_01_fp.cgf" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void UnknownSource_spriggan_proto_mesh_skin_NoMaterialFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\spriggan_proto_mesh.skin" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            Assert.AreEqual((uint)41, cryData.Models[0].NumChunks);
            Assert.AreEqual(ChunkType.Node, cryData.Models[0].ChunkMap[47].ChunkType);
            var datastream = cryData.Models[0].ChunkMap[40] as ChunkDataStream_801;
            Assert.AreEqual((uint)12, datastream.BytesPerElement);
            Assert.AreEqual((uint)22252, datastream.NumElements);
            Assert.AreEqual(0.29570183, datastream.Vertices[0].x, TestUtils.delta);
            Assert.AreEqual(0.42320457, datastream.Vertices[0].y, TestUtils.delta);
            Assert.AreEqual(3.24175549, datastream.Vertices[0].z, TestUtils.delta);

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            //Assert.AreEqual();

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void UnknownSource_spriggan_proto_skel_chr_NoMaterialFile()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\spriggan_proto_skel.chr" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            testUtils.ValidateColladaXml(colladaData);
        }

        // Model appears to be broken.  Assigns 3 materials, but only 2 materials in mtlname chunks
        //[TestMethod]
        //public void Cnylgt_marauder_NoMaterialFile()
        //{
        //    var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\cnylgt_marauder.cga" };
        //    int result = argsHandler.ProcessArgs(args);
        //    Assert.AreEqual(0, result);
        //    CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

        //    COLLADA colladaData = new COLLADA(argsHandler, cryData);
        //    colladaData.GenerateDaeObject();

        //    int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
        //    Assert.AreEqual(3, actualMaterialsCount);

        //    ValidateColladaXml(colladaData);
        //}

        [TestMethod]
        public void Green_fern_bush_a_MaterialFileExists()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\CryEngine\green_fern_bush_a.cgf" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(3, actualMaterialsCount);
            var libraryGeometry = colladaData.DaeObject.Library_Geometries;
            Assert.AreEqual(3, libraryGeometry.Geometry.Length);
            // Validate geometry and colors
            var mesh = colladaData.DaeObject.Library_Geometries.Geometry[0].Mesh;
            Assert.AreEqual(4, mesh.Source.Length);
            Assert.AreEqual(2, mesh.Triangles.Length);
            // Validate Triangles
            Assert.AreEqual(918, mesh.Triangles[0].Count);
            Assert.AreEqual("green_fern_bush-material", mesh.Triangles[0].Material);
            Assert.IsTrue(mesh.Triangles[0].P.Value_As_String.StartsWith("0 0 0 0 1 1 1 1 2 2 2 2 3 3 3 3 4 4 4 4 1 1 1 1 1 1 1 1 0 0 0 0 3 3 3 3 5"));
            Assert.AreEqual(4, mesh.Triangles[0].Input.Length);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void SC_LR7_UOPP_VerifyImageFilePath()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\LR-7_UOPP.cga" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(2, actualMaterialsCount);

            testUtils.ValidateColladaXml(colladaData);
        }
    }
}

