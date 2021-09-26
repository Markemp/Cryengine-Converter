using CgfConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CgfConverterTests.TestUtilities
{
    public class TestUtils
    {
        private readonly XmlSchemaSet schemaSet = new XmlSchemaSet();
        private readonly XmlReaderSettings settings = new XmlReaderSettings();
        internal readonly ArgsHandler argsHandler = new ArgsHandler();
        internal List<string> errors = new List<string>();

        public static double delta = 0.000001;

        internal void GetSchemaSet()
        {
            schemaSet.Add(@"http://www.collada.org/2005/11/COLLADASchema", @"..\..\..\Schemas\collada_schema_1_4_1_ms.xsd");
            schemaSet.Add(@"http://www.w3.org/XML/1998/namespace", @"..\..\..\Schemas\xml.xsd");

            settings.Schemas = schemaSet;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += ValidationEventHandler;
        }

        internal void ValidateColladaXml(Collada colladaData)
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
            using XmlReader reader = XmlReader.Create(xmlFile, settings);
            while (reader.Read()) ;
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

        public static void CompareTwoTransformMatrices(Matrix4x4 expected, Matrix4x4 actual)
        {
            Assert.AreEqual(expected.M11, actual.M11, delta);
            Assert.AreEqual(expected.M12, actual.M12, delta);
            Assert.AreEqual(expected.M13, actual.M13, delta);
            Assert.AreEqual(expected.M14, actual.M14, delta);
            Assert.AreEqual(expected.M21, actual.M21, delta);
            Assert.AreEqual(expected.M22, actual.M22, delta);
            Assert.AreEqual(expected.M23, actual.M23, delta);
            Assert.AreEqual(expected.M24, actual.M24, delta);
            Assert.AreEqual(expected.M31, actual.M31, delta);
            Assert.AreEqual(expected.M32, actual.M32, delta);
            Assert.AreEqual(expected.M33, actual.M33, delta);
            Assert.AreEqual(expected.M34, actual.M34, delta);
            Assert.AreEqual(expected.M41, actual.M41, delta);
            Assert.AreEqual(expected.M42, actual.M42, delta);
            Assert.AreEqual(expected.M43, actual.M43, delta);
            Assert.AreEqual(expected.M44, actual.M44, delta);
        }

        public static byte[] GetBone1WorldToBoneBytes()
        {
            byte[] hexData = {
                (byte)0x84, (byte)0x94, (byte)0x5D, (byte)0xA8, (byte)0x68, (byte)0x21, (byte)0xA2, (byte)0xB3, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x4A, (byte)0xE9, (byte)0xBE, (byte)0x3C,
                (byte)0xFE, (byte)0xFF, (byte)0x7F, (byte)0x3F, (byte)0x4A, (byte)0xEF, (byte)0x2E, (byte)0xB4, (byte)0x18, (byte)0x51, (byte)0xC5, (byte)0x9B, (byte)0xC4, (byte)0x57, (byte)0x3F, (byte)0xA5,
                (byte)0x4B, (byte)0xEF, (byte)0x2E, (byte)0xB4, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x68, (byte)0x21, (byte)0xA2, (byte)0x33, (byte)0x4A, (byte)0xE9, (byte)0x3E, (byte)0xB1
            };
            return hexData;
        }

        public static byte[] GetBone1BoneToWorldBytes()
        {
            byte[] hexData = {
                (byte)0x84, (byte)0x94, (byte)0x5D, (byte)0xA8, (byte)0xFE, (byte)0xFF, (byte)0x7F, (byte)0x3F, (byte)0x4B, (byte)0xEF, (byte)0x2E, (byte)0xB4, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,
                (byte)0x68, (byte)0x21, (byte)0xA2, (byte)0xB3, (byte)0x4A, (byte)0xEF, (byte)0x2E, (byte)0xB4, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x7F, (byte)0x01, (byte)0x8C, (byte)0xB0,
                (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x18, (byte)0x51, (byte)0xC5, (byte)0x9B, (byte)0x68, (byte)0x21, (byte)0xA2, (byte)0x33, (byte)0x4B, (byte)0xE9, (byte)0xBE, (byte)0x3C
            };
            return hexData;
        }

        public static byte[] GetBone2WorldToBoneBytes()
        {
            byte[] hexData = {
                (byte)0x1F, (byte)0xA2, (byte)0xB9, (byte)0xB8, (byte)0xBE, (byte)0x0C, (byte)0x49, (byte)0xB4, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x8E, (byte)0x22, (byte)0xC0, (byte)0xB8,
                (byte)0xFE, (byte)0xFF, (byte)0x7F, (byte)0x3F, (byte)0x3A, (byte)0x44, (byte)0x0D, (byte)0x37, (byte)0x1E, (byte)0xA2, (byte)0xB9, (byte)0xB8, (byte)0x31, (byte)0x68, (byte)0x11, (byte)0xB2,
                (byte)0x4D, (byte)0x44, (byte)0x0D, (byte)0x37, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0xDE, (byte)0x3F, (byte)0x48, (byte)0x34, (byte)0xB7, (byte)0x0F, (byte)0x81, (byte)0xAF
            };

            return hexData;
        }

        public static byte[] GetBone2BoneToWorldBytes()
        {
            byte[] hexData = {
                (byte)0x1F, (byte)0xA2, (byte)0xB9, (byte)0xB8, (byte)0xFE, (byte)0xFF, (byte)0x7F, (byte)0x3F, (byte)0x4D, (byte)0x44, (byte)0x0D, (byte)0x37, (byte)0xB0, (byte)0xB1, (byte)0xC2, (byte)0x2F,
                (byte)0xBE, (byte)0x0C, (byte)0x49, (byte)0xB4, (byte)0x3A, (byte)0x44, (byte)0x0D, (byte)0x37, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0xFB, (byte)0x73, (byte)0x8A, (byte)0xAF,
                (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x1E, (byte)0xA2, (byte)0xB9, (byte)0xB8, (byte)0xDE, (byte)0x3F, (byte)0x48, (byte)0x34, (byte)0x8F, (byte)0x22, (byte)0xC0, (byte)0xB8
            };
            return hexData;
        }

        public static byte[] GetBone3WorldToBoneBytes()
        {
            byte[] hexData = {
                (byte)0x21, (byte)0xA2, (byte)0xBE, (byte)0xB8, (byte)0x43, (byte)0x12, (byte)0x49, (byte)0xB4, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0xDF, (byte)0xB7, (byte)0xD8, (byte)0xBC,
                (byte)0xFE, (byte)0xFF, (byte)0x7F, (byte)0x3F, (byte)0xE3, (byte)0xF9, (byte)0x09, (byte)0x37, (byte)0x20, (byte)0xA2, (byte)0xBE, (byte)0xB8, (byte)0x38, (byte)0x42, (byte)0x8E, (byte)0xB3,
                (byte)0xF6, (byte)0xF9, (byte)0x09, (byte)0x37, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0xC5, (byte)0x44, (byte)0x48, (byte)0x34, (byte)0x2F, (byte)0x89, (byte)0xBC, (byte)0xAE
            };
            return hexData;
        }

        public static byte[] GetBone3BoneToWorldBytes()
        {
            byte[] hexData = {
                (byte)0x21, (byte)0xA2, (byte)0xBE, (byte)0xB8, (byte)0xFE, (byte)0xFF, (byte)0x7F, (byte)0x3F, (byte)0xF6, (byte)0xF9, (byte)0x09, (byte)0x37, (byte)0xA7, (byte)0xEF, (byte)0x1C, (byte)0xB6,
                (byte)0x43, (byte)0x12, (byte)0x49, (byte)0xB4, (byte)0xE3, (byte)0xF9, (byte)0x09, (byte)0x37, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x2F, (byte)0x25, (byte)0xAD, (byte)0xB1,
                (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x20, (byte)0xA2, (byte)0xBE, (byte)0xB8, (byte)0xC5, (byte)0x44, (byte)0x48, (byte)0x34, (byte)0xE0, (byte)0xB7, (byte)0xD8, (byte)0xBC
            };
            return hexData;
        }

        public static Matrix4x4 GetExpectedBone1BPM()
        {
            Matrix4x4 m = new()
            {
                M11 = 0,
                M12 = 1,
                M13 = 0,
                M14 = 0,
                M21 = 0,
                M22 = 0,
                M23 = -1,
                M24 = 0,
                M31 = -1,
                M32 = 0,
                M33 = 0,
                M34 = 0.023305f,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };
            return m;
        }

        public static Matrix4x4 GetExpectedBone2BPM()
        {
            Matrix4x4 m = new()
            {
                M11 = 1,
                M12 = 0.000089f,
                M13 = 0,
                M14 = 0.023396f,
                M21 = -0.000089f,
                M22 = 1,
                M23 = 0.000009f,
                M24 = 0,
                M31 = 0,
                M32 = -0.000009f,
                M33 = 1,
                M34 = 0,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };
            return m;
        }

        public static Matrix4x4 GetExpectedBone3BPM()
        {
            Matrix4x4 m = new()
            {
                M11 = 1,
                M12 = 0.000002f,
                M13 = 0,
                M14 = 0.026363f,
                M21 = -0.000002f,
                M22 = 1,
                M23 = 0,
                M24 = 0,
                M31 = 0,
                M32 = 0,
                M33 = 1,
                M34 = 0,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };
            return m;
        }
    }
}
