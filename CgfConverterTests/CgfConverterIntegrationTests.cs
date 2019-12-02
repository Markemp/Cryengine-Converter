using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CgfConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests
{
    [TestClass]
    public class CgfConverterIntegrationTests
    {
        ArgsHandler argsHandler = new ArgsHandler();
        public XmlSchema schema = new XmlSchema();

        [TestInitialize]
        public void Initialize()
        {
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;
            GetSchema();
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
            //ValidateXml(daeFile); // TODO:   Get this to work. :(
        }
        
        void ValidateXml(COLLADA daeFile)  // For testing
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(@"http://www.collada.org/2005/11/COLLADASchema", @"..\..\..\collada_schema_1_4.xsd");
            settings.ValidationType = ValidationType.Schema;
            
            //settings.Schemas.Add(@"http://www.w3.org/2001/XMLSchema", @"..\..\..\xml.xsd");

            XmlDocument document = new XmlDocument();
            document.Load(daeFile.ToString());

            ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

            // the following call to Validate succeeds.
            document.Validate(eventHandler);
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.WriteLine("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    Console.WriteLine("Warning {0}", e.Message);
                    break;
            }
        }

        private void GetSchema()                                             // Get the schema from kronos.org.  Needs error checking in case it's offline
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(@"http://www.collada.org/2005/11/COLLADASchema", @"..\..\..\collada_schema_1_4.xsd");

            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            schema.TargetNamespace = @"http://www.collada.org/2005/11/COLLADASchema";
        }
    }
}
