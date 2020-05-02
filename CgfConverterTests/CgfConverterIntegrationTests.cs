using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CgfConverter;
using grendgine_collada;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests
{
    [TestClass]
    public class CgfConverterIntegrationTests
    {
        readonly ArgsHandler argsHandler = new ArgsHandler();
        private readonly XmlSchemaSet schemaSet = new XmlSchemaSet();
        private readonly XmlReaderSettings settings = new XmlReaderSettings();
        List<string> errors;

        [TestInitialize]
        public void Initialize()
        {
            errors = new List<string>();
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            GetSchemaSet();
        }

        [TestMethod]
        public void SimpleCubeSchemaValidation()
        {
            ValidateXml(@"..\..\ResourceFiles\simple_cube.dae");
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void SimpleCubeSchemaValidation_BadColladaWithOneError()
        {
            ValidateXml(@"..\..\ResourceFiles\simple_cube_bad.dae");
            Assert.AreEqual(1, errors.Count);
        }

        [TestMethod]
        public void MWO_industrial_wetlamp_a_MaterialFileNotFound()
        {
            var args = new string[] { @"..\..\ResourceFiles\industrial_wetlamp_a.cgf", "-dds", "-dae", "-objectdir", @"d:\depot\mwo\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(3, actualMaterialsCount);
            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_timberwolf_chr()
        {
            var args = new string[] { @"..\..\ResourceFiles\timberwolf.chr", "-dds", "-dae", "-objectdir", @"d:\depot\lol\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(11, actualMaterialsCount);
            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_candycane_a_MaterialFileNotAvailable()
        {
            var args = new string[] { @"..\..\ResourceFiles\candycane_a.chr", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(2, actualMaterialsCount);
            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_hbr_right_torso_uac5_bh1_cga()
        {
            var args = new string[] { @"..\..\ResourceFiles\hbr_right_torso_uac5_bh1.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(21, actualMaterialsCount);
            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_hbr_right_torso_cga()
        {
            var args = new string[] { @"..\..\ResourceFiles\hbr_right_torso.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            var colladaData = new COLLADA(argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            //colladaData.GenerateDaeObject();
            colladaData.Render();
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
            Assert.AreEqual(1, geometry.Mesh.Polylist.Length);
            Assert.AreEqual(1908, geometry.Mesh.Polylist[0].Count);
            var source = geometry.Mesh.Source;
            var vertices = geometry.Mesh.Vertices;
            var polylist = geometry.Mesh.Polylist;
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
            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void AEGS_Avenger_IntegrationTest()
        {
            var args = new string[] { @"..\..\ResourceFiles\SC\AEGS_Avenger.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\SC\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            var colladaData = new COLLADA(argsHandler, cryData);
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
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();
            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void UnknownSource_forest_ruin()
        {
            var args = new string[] { @"..\..\ResourceFiles\forest_ruin.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(13, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void GhostSniper3_raquel_eyeoverlay_skin()
        {
            var args = new string[] { @"..\..\ResourceFiles\Test01\raquel_eyeoverlay.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Test01\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(6, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Prey_Dahl_GenMaleBody01_MaterialFileFound()
        {
            var args = new string[] { @"..\..\ResourceFiles\Prey\Dahl_GenMaleBody01.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Prey\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(1, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Prey_Dahl_GenMaleBody01_MaterialFileNotAvailable()
        {
            var args = new string[] { @"..\..\ResourceFiles\Prey\Dahl_GenMaleBody01.skin" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(1, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Evolve_griffin_skin_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\Evolve\griffin.skin" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Evolve_griffin_menu_harpoon_skin_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\Evolve\griffin_menu_harpoon.skin" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Evolve_griffin_fp_skeleton_chr_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\Evolve\griffin_fp_skeleton.chr" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void UnknownSource_osv_96_muzzle_brake_01_fp_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\osv_96_muzzle_brake_01_fp.cgf" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void UnknownSource_spriggan_proto_mesh_skin_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\spriggan_proto_mesh.skin" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void UnknownSource_spriggan_proto_skel_chr_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\spriggan_proto_skel.chr" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void Cnylgt_marauder_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\cnylgt_marauder.cga" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(3, actualMaterialsCount);

            ValidateColladaXml(colladaData);

        }

        [TestMethod]
        public void Green_fern_bush_a_MaterialFileExists()
        {
            var args = new string[] { @"..\..\ResourceFiles\CryEngine\green_fern_bush_a.cgf" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA colladaData = new COLLADA(argsHandler, cryData);
            colladaData.Render();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(3, actualMaterialsCount);

            ValidateColladaXml(colladaData);
        }

        private void ValidateColladaXml(COLLADA colladaData)
        {
            using (var stringWriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(colladaData.DaeObject.GetType());
                serializer.Serialize(stringWriter, colladaData.DaeObject);
                string dae = stringWriter.ToString();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(dae);
                doc.Schemas = settings.Schemas;
                doc.Validate(ValidationEventHandler);
            }
        }

        private void ValidateXml(string xmlFile)
        {
            using (XmlReader reader = XmlReader.Create(xmlFile, settings))
            {
                while (reader.Read()) ;
            }
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    errors.Add($@"Error: {e.Message}");
                    break;
                case XmlSeverityType.Warning:
                    errors.Add($@"Warning: {e.Message}");
                    break;
            }
        }

        private void GetSchemaSet()
        {
            schemaSet.Add(@"http://www.collada.org/2005/11/COLLADASchema", @"..\..\Schemas\collada_schema_1_4_1_ms.xsd");
            schemaSet.Add(@"http://www.w3.org/XML/1998/namespace", @"..\..\Schemas\xml.xsd");

            settings.Schemas = schemaSet;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += ValidationEventHandler;
        }
    }
}

