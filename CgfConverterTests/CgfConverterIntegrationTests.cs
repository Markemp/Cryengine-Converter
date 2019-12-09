using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CgfConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests
{
    [TestClass]
    public class CgfConverterIntegrationTests
    {
        ArgsHandler argsHandler = new ArgsHandler();
        private XmlSchemaSet schemaSet = new XmlSchemaSet();
        private XmlReaderSettings settings = new XmlReaderSettings();
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
        public void MWO_industrial_wetlamp_a()
        {
            var args = new String[] { @"..\..\ResourceFiles\industrial_wetlamp_a.cgf", "-dds", "-dae", "-objectdir", @"d:\depot\mwo\" };
            Int32 result = argsHandler.ProcessArgs(args);
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
            Int32 result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);
            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void MWO_candycane_a()
        {
            var args = new String[] { @"..\..\ResourceFiles\candycane_a.chr", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            Int32 result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);
            ValidateColladaXml(daeFile);
        }


        [TestMethod]
        public void MWO_hbr_right_torso_uac5_bh1_cga()
        {
            var args = new String[] { @"..\..\ResourceFiles\hbr_right_torso_uac5_bh1.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            Int32 result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(24, actualMaterialsCount);
            ValidateColladaXml(daeFile);
        }

        [TestMethod]
        public void MWO_hbr_right_torso_cga()
        {
            var args = new String[] { @"..\..\ResourceFiles\hbr_right_torso.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            Int32 result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(24, actualMaterialsCount);
            ValidateColladaXml(daeFile);
        }


        [TestMethod]
        public void SC_uee_asteroid_ACTutorial_rail_01()
        {
            var args = new String[] { @"..\..\ResourceFiles\uee_asteroid_ACTutorial_rail_01.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            Int32 result = argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result); 
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
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
            XmlReader reader = XmlReader.Create(xmlFile, settings);
            while (reader.Read()) ;
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
