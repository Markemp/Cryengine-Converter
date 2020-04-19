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
            var args = new String[] { @"..\..\ResourceFiles\industrial_wetlamp_a.cgf", "-dds", "-dae", "-objectdir", @"d:\depot\mwo\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(3, actualMaterialsCount);
            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void MWO_timberwolf_chr()
        {
            var args = new String[] { @"..\..\ResourceFiles\timberwolf.chr", "-dds", "-dae", "-objectdir", @"d:\depot\lol\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(11, actualMaterialsCount);
            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void MWO_candycane_a_MaterialFileNotAvailable()
        {
            var args = new String[] { @"..\..\ResourceFiles\candycane_a.chr", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(2, actualMaterialsCount);
            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void MWO_hbr_right_torso_uac5_bh1_cga()
        {
            var args = new String[] { @"..\..\ResourceFiles\hbr_right_torso_uac5_bh1.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(21, actualMaterialsCount);
            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void MWO_hbr_right_torso_cga()
        {
            var args = new String[] { @"..\..\ResourceFiles\hbr_right_torso.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            var daeFile = new COLLADA(argsHandler, cryData);
            var daeObject = daeFile.DaeObject;
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
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
            const string caseMatrix = "-1.000000 0.000001 0.000009 1.830486 -0.000005 -0.866025 -0.500000 -2.444341 0.000008 -0.500000 0.866025 -1.542505 0.000000 0.000000 0.000000 1.000000";
            const string fxMatrix = "1.000000 0.000000 -0.000009 1.950168 0.000000 1.000000 0.000000 0.630385 0.000009 0.000000 1.000000 -0.312732 0.000000 0.000000 0.000000 1.000000";
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
            // Material Check
            Assert.AreEqual(5, actualMaterialsCount);
            Assert.AreEqual("hbr_right_torso", daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].ID);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].Instance_Geometry.Length);
            ValidateColladaXml(daeFile);
        }


        [TestMethod]
        public void SC_uee_asteroid_ACTutorial_rail_01()
        {
            var args = new String[] { @"..\..\ResourceFiles\uee_asteroid_ACTutorial_rail_01.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void UnknownSource_forest_ruin()
        {
            var args = new String[] { @"..\..\ResourceFiles\forest_ruin.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(12, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void GhostSniper3_raquel_eyeoverlay_skin()
        {
            var args = new String[] { @"..\..\ResourceFiles\Test01\raquel_eyeoverlay.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Test01\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(6, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void Prey_Dahl_GenMaleBody01_MaterialFileFound()
        {
            var args = new String[] { @"..\..\ResourceFiles\Prey\Dahl_GenMaleBody01.skin", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\Prey\" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(1, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void Prey_Dahl_GenMaleBody01_MaterialFileNotAvailable()
        {
            var args = new String[] { @"..\..\ResourceFiles\Prey\Dahl_GenMaleBody01.skin" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(1, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void Evolve_griffin_skin_NoMaterialFile()
        {
            var args = new String[] { @"..\..\ResourceFiles\Evolve\griffin.skin" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void Evolve_griffin_menu_harpoon_skin_NoMaterialFile()
        {
            var args = new String[] { @"..\..\ResourceFiles\Evolve\griffin_menu_harpoon.skin" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void Evolve_griffin_fp_skeleton_chr_NoMaterialFile()
        {
            var args = new String[] { @"..\..\ResourceFiles\Evolve\griffin_fp_skeleton.chr" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void UnknownSource_osv_96_muzzle_brake_01_fp_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\osv_96_muzzle_brake_01_fp.cgf" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void UnknownSource_spriggan_proto_mesh_skin_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\spriggan_proto_mesh.skin" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void UnknownSource_spriggan_proto_skel_chr_NoMaterialFile()
        {
            var args = new string[] { @"..\..\ResourceFiles\spriggan_proto_skel.chr" };
            int result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);

            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);

            ValidateColladaXml(daeFile);
        }

        private void ValidateColladaXml(COLLADA daeFile)
        {
            using (var stringWriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(daeFile.DaeObject.GetType());
                serializer.Serialize(stringWriter, daeFile.DaeObject);
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
