using CgfConverter;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada;

namespace CgfConverterTests.TestUtilities;

public class TestUtils
{
    private readonly XmlSchemaSet schemaSet = new();
    private readonly XmlReaderSettings settings = new();
    internal readonly ArgsHandler argsHandler = new();
    internal List<string> errors = new();

    public const double delta = 0.000001;

    internal void GetSchemaSet()
    {
        schemaSet.Add(@"http://www.collada.org/2005/11/COLLADASchema", @"..\..\..\Schemas\collada_schema_1_4_1_ms.xsd");
        schemaSet.Add(@"http://www.w3.org/XML/1998/namespace", @"..\..\..\Schemas\xml.xsd");

        settings.Schemas = schemaSet;
        settings.ValidationType = ValidationType.Schema;
        settings.ValidationEventHandler += ValidationEventHandler;
    }

    internal void ValidateColladaXml(ColladaModelRenderer colladaData)
    {
        using var stringWriter = new System.IO.StringWriter();
        var serializer = new XmlSerializer(colladaData.DaeObject.GetType());
        serializer.Serialize(stringWriter, colladaData.DaeObject);
        string dae = stringWriter.ToString();

        XmlDocument doc = new();
        doc.LoadXml(dae);
        doc.Schemas = settings.Schemas;
        doc.Validate(ValidationEventHandler);
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

    public static byte[] GetBone1WorldToBoneBytes()
    {
        byte[] hexData = {
            (byte)0x84, (byte)0x94, (byte)0x5D, (byte)0xA8, (byte)0x68, (byte)0x21, (byte)0xA2, (byte)0xB3, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x4A, (byte)0xE9, (byte)0xBE, (byte)0x3C,
            (byte)0xFE, (byte)0xFF, (byte)0x7F, (byte)0x3F, (byte)0x4A, (byte)0xEF, (byte)0x2E, (byte)0xB4, (byte)0x18, (byte)0x51, (byte)0xC5, (byte)0x9B, (byte)0xC4, (byte)0x57, (byte)0x3F, (byte)0xA5,
            (byte)0x4B, (byte)0xEF, (byte)0x2E, (byte)0xB4, (byte)0xFF, (byte)0xFF, (byte)0x7F, (byte)0xBF, (byte)0x68, (byte)0x21, (byte)0xA2, (byte)0x33, (byte)0x4A, (byte)0xE9, (byte)0x3E, (byte)0xB1
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
}
