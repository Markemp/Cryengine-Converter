using CgfConverter;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CgfConverterTests.TestUtilities
{
    class TestUtils
    {
        private readonly XmlSchemaSet schemaSet = new XmlSchemaSet();
        private readonly XmlReaderSettings settings = new XmlReaderSettings();
        internal readonly ArgsHandler argsHandler = new ArgsHandler();
        internal List<string> errors = new List<string>();

        public double delta = 0.00000001;

        internal void GetSchemaSet()
        {
            schemaSet.Add(@"http://www.collada.org/2005/11/COLLADASchema", @"..\..\..\Schemas\collada_schema_1_4_1_ms.xsd");
            schemaSet.Add(@"http://www.w3.org/XML/1998/namespace", @"..\..\..\Schemas\xml.xsd");

            settings.Schemas = schemaSet;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += ValidationEventHandler;
        }

        internal void ValidateColladaXml(COLLADA colladaData)
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

        internal void ValidateXml(string xmlFile)
        {
            using (XmlReader reader = XmlReader.Create(xmlFile, settings))
            {
                while (reader.Read()) ;
            }
        }

        internal void ValidationEventHandler(object sender, ValidationEventArgs e)
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
    }
}
