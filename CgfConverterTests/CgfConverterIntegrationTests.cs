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
            
            XmlReader reader = XmlReader.Create(@"..\..\ResourceFiles\simple_cube.dae", settings);
            while (reader.Read()) ;
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void SimpleCubeSchemaValidation_BadCollada()
        {
            XmlReader reader = XmlReader.Create(@"..\..\ResourceFiles\simple_cube_bad.dae", settings);
            while (reader.Read()) ;
            Assert.AreEqual(1, errors.Count);
        }

        [TestMethod]
        public void MWO_industrial_wetlamp_a()
        {
            var args = new String[] { @"D:\depot\mwo\Objects\environments\frontend\mechlab_a\lights\industrial_wetlamp_a.cgf", "-dds", "-dae", "-objectdir", @"d:\depot\mwo\" };
            Int32 result = argsHandler.ProcessArgs(args);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);
            
            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            int actualMaterialsCount = daeFile.daeObject.Library_Materials.Material.Count();
            Assert.AreEqual(3, actualMaterialsCount);
            ValidateXml(daeFile);
        }

        [TestMethod]
        public void SC_uee_asteroid_ACTutorial_rail_01()
        {
            var args = new String[] { @"..\..\ResourceFiles\uee_asteroid_ACTutorial_rail_01.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            Int32 result = argsHandler.ProcessArgs(args);
            CryEngine cryData = new CryEngine(args[0], argsHandler.DataDir.FullName);

            COLLADA daeFile = new COLLADA(argsHandler, cryData);
            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
            ValidateXml(daeFile);
        }

        private void ValidateXml(COLLADA daeFile)
        {
            using (var stringWriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(daeFile.daeObject.GetType());
                serializer.Serialize(stringWriter, daeFile.daeObject);
                string dae = stringWriter.ToString();

                XmlReader reader = XmlReader.Create(dae, settings);
                while (reader.Read()) ;
            }


            //    XmlDocument document = new XmlDocument();
            //document.Load(daeFile.daeObject.ToString());

            //ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

            //// the following call to Validate succeeds.
            //document.Validate(eventHandler);
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
