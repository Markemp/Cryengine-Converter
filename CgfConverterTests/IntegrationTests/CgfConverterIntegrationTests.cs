using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverterTests.TestUtilities;
using grendgine_collada;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests
{
    [TestClass]
    public class CgfConverterIntegrationTests
    {
        private readonly TestUtils testUtils = new TestUtils();

        [TestInitialize]
        public void Initialize()
        {
            testUtils.errors = new List<string>();
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            testUtils.GetSchemaSet();
        }

        [TestMethod]
        public void SimpleCubeSchemaValidation()
        {
            testUtils.ValidateXml(@"..\..\ResourceFiles\simple_cube.dae");
            Assert.AreEqual(0, testUtils.errors.Count);
        }

        [TestMethod]
        public void SimpleCubeSchemaValidation_BadColladaWithOneError()
        {
            testUtils.ValidateXml(@"..\..\ResourceFiles\simple_cube_bad.dae");
            Assert.AreEqual(1, testUtils.errors.Count);
        }

        [TestMethod]
        public void MWO_industrial_wetlamp_a_MaterialFileNotFound()
        {
            var args = new string[] { @"..\..\ResourceFiles\industrial_wetlamp_a.cgf", "-dds", "-dae", "-objectdir", @"d:\depot\mwo\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(3, actualMaterialsCount);
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_timberwolf_chr()
        {
            var args = new string[] { @"..\..\ResourceFiles\timberwolf.chr", "-dds", "-dae", "-objectdir", @"d:\depot\lol\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(11, actualMaterialsCount);
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_candycane_a_MaterialFileNotAvailable()
        {
            var args = new string[] { @"..\..\ResourceFiles\candycane_a.chr", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
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

        [TestMethod]
        public void MWO_hbr_right_torso_uac5_bh1_cga()
        {
            var args = new string[] { @"..\..\ResourceFiles\hbr_right_torso_uac5_bh1.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(21, actualMaterialsCount);
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_hbr_right_torso_cga()
        {
            var args = new string[] { @"..\..\ResourceFiles\hbr_right_torso.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            var colladaData = new COLLADA(testUtils.argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            //colladaData.GenerateDaeObject();
            colladaData.GenerateDaeObject();
            Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
            Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
            // Visual Scene Check
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
            Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);
            // Node check
            var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
            Assert.AreEqual("hbr_right_torso", node.ID);
            Assert.AreEqual("hbr_right_torso", node.Name);
            Assert.AreEqual(1, node.Instance_Geometry.Length);
            Assert.AreEqual(2, node.node.Length);
            Assert.AreEqual(1, node.Matrix.Length);
            Assert.AreEqual(1, node.Instance_Geometry.Length);
            Assert.AreEqual("hbr_right_torso_case", node.node[0].ID);
            Assert.AreEqual("hbr_right_torso_case", node.node[0].Name);
            Assert.AreEqual("hbr_right_torso_fx", node.node[1].Name);
            Assert.AreEqual(Grendgine_Collada_Node_Type.NODE, node.node[0].Type);
            const string caseMatrix = "-1.000000 -0.000005 0.000008 1.830486 0.000001 -0.866025 -0.500000 -2.444341 0.000009 -0.500000 0.866025 -1.542505 0.000000 0.000000 0.000000 1.000000";
            const string fxMatrix = "1.000000 0.000000 0.000009 1.950168 0.000000 1.000000 0.000000 0.630385 -0.000009 0.000000 1.000000 -0.312732 0.000000 0.000000 0.000000 1.000000";
            Assert.AreEqual(caseMatrix, node.node[0].Matrix[0].Value_As_String);
            Assert.AreEqual(fxMatrix, node.node[1].Matrix[0].Value_As_String);
            // Node Matrix check
            const string matrix = "1.000000 0.000000 0.000000 0.000000 0.000000 1.000000 0.000000 0.000000 0.000000 0.000000 1.000000 0.000000 0.000000 0.000000 0.000000 1.000000";
            Assert.AreEqual(matrix, node.Matrix[0].Value_As_String);
            Assert.AreEqual("transform", node.Matrix[0].sID);
            // Instance Geometry check
            Assert.AreEqual("hbr_right_torso", node.Instance_Geometry[0].Name);
            Assert.AreEqual("#hbr_right_torso-mesh", node.Instance_Geometry[0].URL);
            Assert.AreEqual(1, node.Instance_Geometry[0].Bind_Material.Length);
            Assert.AreEqual(1, node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material.Length);
            Assert.AreEqual("hellbringer_body-material", node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
            Assert.AreEqual("#hellbringer_body-material", node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
            // library_materials Check
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            var materials = colladaData.DaeObject.Library_Materials;
            Assert.AreEqual(5, actualMaterialsCount);
            Assert.AreEqual("hellbringer_body-material", materials.Material[0].ID);
            Assert.AreEqual("decals-material", materials.Material[1].ID);
            Assert.AreEqual("hellbringer_variant-material", materials.Material[2].ID);
            Assert.AreEqual("hellbringer_window-material", materials.Material[3].ID);
            Assert.AreEqual("Material #0-material", materials.Material[4].ID);
            Assert.AreEqual("#hellbringer_body-effect", materials.Material[0].Instance_Effect.URL);
            Assert.AreEqual("#decals-effect", materials.Material[1].Instance_Effect.URL);
            Assert.AreEqual("#hellbringer_variant-effect", materials.Material[2].Instance_Effect.URL);
            Assert.AreEqual("#hellbringer_window-effect", materials.Material[3].Instance_Effect.URL);
            Assert.AreEqual("#Material #0-effect", materials.Material[4].Instance_Effect.URL);

            // library_geometries check
            Assert.AreEqual(1, colladaData.DaeObject.Library_Geometries.Geometry.Length);
            var geometry = colladaData.DaeObject.Library_Geometries.Geometry[0];
            Assert.AreEqual("hbr_right_torso-mesh", geometry.ID);
            Assert.AreEqual("hbr_right_torso", geometry.Name);
            Assert.AreEqual(4, geometry.Mesh.Source.Length);
            Assert.AreEqual("hbr_right_torso-vertices", geometry.Mesh.Vertices.ID);
            Assert.AreEqual(1, geometry.Mesh.Triangles.Length);
            Assert.AreEqual(1908, geometry.Mesh.Triangles[0].Count);
            var source = geometry.Mesh.Source;
            var vertices = geometry.Mesh.Vertices;
            var triangles = geometry.Mesh.Triangles;
            // Triangles check
            Assert.AreEqual("hellbringer_body-material", triangles[0].Material);
            Assert.AreEqual("#hbr_right_torso-mesh-pos", vertices.Input[0].source);
            Assert.IsTrue(triangles[0].P.Value_As_String.StartsWith("0 0 0 1 1 1 2 2 2 3 3 3 4 4 4 5 5 5 5 5 5 6 6 6 3 3 3 7 7 7 8 8 8 9 9 9 9 9 9"));
            // Source check
            Assert.AreEqual("hbr_right_torso-mesh-pos", source[0].ID);
            Assert.AreEqual("hbr_right_torso-pos", source[0].Name);
            Assert.AreEqual("hbr_right_torso-mesh-pos-array", source[0].Float_Array.ID);
            Assert.AreEqual(7035, source[0].Float_Array.Count);
            var floatArray = source[0].Float_Array.Value_As_String;
            Assert.IsTrue(floatArray.StartsWith("2.525999 -1.729837 -1.258107 2.526004 -1.863573 -1.080200 2.525999 -1.993050 -1.255200 2.740049 -0.917271 0.684382 2.740053 -0.917271 0.840976 2.793932"));
            Assert.IsTrue(floatArray.EndsWith("-3.263152 2.340879 -1.480840 -3.231119 2.352005 -1.494859 -3.268093 2.329598 -1.478497 -3.234514 2.335588 -1.491449 -3.273033 2.320036 -1.471824 -3.237391"));
            Assert.AreEqual((uint)2345, source[0].Technique_Common.Accessor.Count);
            Assert.AreEqual((uint)0, source[0].Technique_Common.Accessor.Offset);
            Assert.AreEqual(3, source[0].Technique_Common.Accessor.Param.Length);
            Assert.AreEqual("X", source[0].Technique_Common.Accessor.Param[0].Name);
            Assert.AreEqual("float", source[0].Technique_Common.Accessor.Param[0].Type);


            Assert.AreEqual("hbr_right_torso", daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].ID);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].Instance_Geometry.Length);
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void AEGS_Avenger_IntegrationTest()
        {
            var args = new string[] { @"..\..\ResourceFiles\SC\AEGS_Avenger.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\SC\" };
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
            var args = new string[] { @"..\..\ResourceFiles\hangar_asteroid_controlroom_fan.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void UnknownSource_forest_ruin()
        {
            var args = new string[] { @"..\..\ResourceFiles\forest_ruin.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
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
            var args = new string[] { @"..\..\ResourceFiles\Test01\raquel_eyeoverlay.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Test01\" };
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
            var args = new string[] { @"..\..\ResourceFiles\Prey\Dahl_GenMaleBody01.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Prey\" };
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
            var args = new string[] { @"..\..\ResourceFiles\Prey\Dahl_GenMaleBody01.skin" };
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
            var args = new string[] { @"..\..\ResourceFiles\Evolve\griffin.skin" };
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
            var args = new string[] { @"..\..\ResourceFiles\Evolve\griffin_menu_harpoon.skin" };
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
            var args = new string[] { @"..\..\ResourceFiles\Evolve\griffin_fp_skeleton.chr" };
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
            var args = new string[] { @"..\..\ResourceFiles\osv_96_muzzle_brake_01_fp.cgf" };
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
            var args = new string[] { @"..\..\ResourceFiles\spriggan_proto_mesh.skin" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            Assert.AreEqual((uint)41, cryData.Models[0].NumChunks);
            Assert.AreEqual(ChunkTypeEnum.Node, cryData.Models[0].ChunkMap[47].ChunkType);
            var datastream = cryData.Models[0].ChunkMap[40] as ChunkDataStream_801;
            Assert.AreEqual((uint)12, datastream.BytesPerElement);
            Assert.AreEqual((uint)22252, datastream.NumElements);
            Assert.AreEqual(0.29570183, datastream.Vertices[0].x, testUtils.delta);
            Assert.AreEqual(0.42320457, datastream.Vertices[0].y, testUtils.delta);
            Assert.AreEqual(3.24175549, datastream.Vertices[0].z, testUtils.delta);

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
            var args = new string[] { @"..\..\ResourceFiles\spriggan_proto_skel.chr" };
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
        //    var args = new string[] { @"..\..\ResourceFiles\cnylgt_marauder.cga" };
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
            var args = new string[] { @"..\..\ResourceFiles\CryEngine\green_fern_bush_a.cgf" };
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
            var args = new string[] { @"..\..\ResourceFiles\SC\LR-7_UOPP.cga" };
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

