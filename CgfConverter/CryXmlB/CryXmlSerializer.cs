using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HoloXPLOR.DataForge;

public static class CryXmlSerializer
{
    public static XmlDocument ReadFile(string inFile, bool writeLog = false) => ReadStream(File.OpenRead(inFile), writeLog);

    public static XmlDocument ReadStream(Stream inStream, bool writeLog = false)
    {
        using var br = new BinaryReader(inStream);
        var peek = (char)br.PeekChar();

        if (peek == '<')    // XML
        {
            var xml = new XmlDocument();
            xml.Load(inStream);
            return xml;     // File is already XML
        }
        else if (peek == 'p')  // pbxml, for Sonic Boom {
            return LoadPbxmlFile(writeLog, br);
        else if (peek == 'C')  // Binary Cryengine XML (CryXmlB)
            return LoadScXmlFile(writeLog, br);

        throw new Exception("Unknown File Format"); // Unknown file format
    }

    private static XmlDocument LoadPbxmlFile(bool writeLog, BinaryReader br)
    {
        string header = br.ReadCString();

        if (header != "pbxml")
            throw new Exception("Unknown File Format");

        XmlDocument doc = new();

        var element = CreateNewElement(br, doc);
        doc.AppendChild(element);
        return doc;
    }

    private static XmlElement CreateNewElement(BinaryReader br, XmlDocument doc)
    {
        int numberOfChildren = br.ReadByte();
        int numberOfAttributes = br.ReadByte();

        var nodeName = br.ReadCString();

        var element = doc.CreateElement(nodeName);

        for (int i = 0; i < numberOfAttributes; i++)
        {
            var key = br.ReadCString();
            var value = br.ReadCString();
            element.SetAttribute(key, value);
        }
        for (int i = 0; i < numberOfChildren; i++)
        {
            br.ReadByte();
            var flag = br.ReadByte();
            if (flag != 0x00 && flag != 0x4B && flag != 0x4D)
                br.ReadByte();

            element.AppendChild(CreateNewElement(br, doc));
        }

        return element;
    }

    private static XmlDocument LoadScXmlFile(bool writeLog, BinaryReader br)
    {
        string header = br.ReadCString();

        if (header != "CryXmlB")
            throw new Exception("Unknown File Format");

        var headerLength = br.BaseStream.Position;
        var fileLength = br.ReadInt32();

        var nodeTableOffset = br.ReadInt32();
        var nodeTableCount = br.ReadInt32();
        var nodeTableSize = 28;

        var referenceTableOffset = br.ReadInt32();
        var referenceTableCount = br.ReadInt32();
        var referenceTableSize = 8;

        var offset3 = br.ReadInt32();
        var count3 = br.ReadInt32();
        var length3 = 4;

        var contentOffset = br.ReadInt32();
        var contentLength = br.ReadInt32();

        if (writeLog)
        {
            Console.WriteLine("Header");
            Console.WriteLine("0x{0:X6}: {1}", 0x00, header);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x00, fileLength);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x04, nodeTableOffset);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x08, nodeTableCount);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x12, referenceTableOffset);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x16, referenceTableCount);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x20, offset3);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x24, count3);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x28, contentOffset);
            Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x32, contentLength);
            Console.WriteLine("");
            Console.WriteLine("Node Table");
        }

        var nodeTable = new List<CryXmlNode>();
        br.BaseStream.Seek(nodeTableOffset, SeekOrigin.Begin);
        int nodeID = 0;

        while (br.BaseStream.Position < nodeTableOffset + nodeTableCount * nodeTableSize)
        {
            var position = br.BaseStream.Position;
            var value = new CryXmlNode
            {
                NodeID = nodeID++,
                NodeNameOffset = br.ReadInt32(),
                ItemType = br.ReadInt32(),
                AttributeCount = br.ReadInt16(),
                ChildCount = br.ReadInt16(),
                ParentNodeID = br.ReadInt32(),
                FirstAttributeIndex = br.ReadInt32(),
                FirstChildIndex = br.ReadInt32(),
                Reserved = br.ReadInt32(),
            };

            nodeTable.Add(value);
            if (writeLog)
            {
                Console.WriteLine(
                    "0x{0:X6}: {1:X8} {2:X8} {3:X4} {4:X4} {5:X8} {6:X8} {7:X8} {8:X8}",
                    position,
                    value.NodeNameOffset,
                    value.ItemType,
                    value.AttributeCount,
                    value.ChildCount,
                    value.ParentNodeID,
                    value.FirstAttributeIndex,
                    value.FirstChildIndex,
                    value.Reserved);
            }
        }

        if (writeLog)
            Console.WriteLine("\nReference Table");

        var attributeTable = new List<CryXmlReference>();
        br.BaseStream.Seek(referenceTableOffset, SeekOrigin.Begin);

        while (br.BaseStream.Position < referenceTableOffset + referenceTableCount * referenceTableSize)
        {
            var position = br.BaseStream.Position;
            var value = new CryXmlReference
            {
                NameOffset = br.ReadInt32(),
                ValueOffset = br.ReadInt32()
            };

            attributeTable.Add(value);

            if (writeLog)
                Console.WriteLine("0x{0:X6}: {1:X8} {2:X8}", position, value.NameOffset, value.ValueOffset);
        }
        if (writeLog)
            Console.WriteLine("\nOrder Table");

        var table3 = new List<int>();
        br.BaseStream.Seek(offset3, SeekOrigin.Begin);
        while (br.BaseStream.Position < offset3 + count3 * length3)
        {
            var position = br.BaseStream.Position;
            var value = br.ReadInt32();

            table3.Add(value);

            if (writeLog)
                Console.WriteLine("0x{0:X6}: {1:X8}", position, value);
        }

        if (writeLog)
            Console.WriteLine("\nDynamic Dictionary");

        var dataTable = new List<CryXmlValue>();
        br.BaseStream.Seek(contentOffset, SeekOrigin.Begin);

        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var position = br.BaseStream.Position;
            var value = new CryXmlValue
            {
                Offset = (int)position - contentOffset,
                Value = br.ReadCString(),
            };
            dataTable.Add(value);

            if (writeLog)
                Console.WriteLine("0x{0:X6}: {1:X8} {2}", position, value.Offset, value.Value);
        }

        var dataMap = dataTable.ToDictionary(k => k.Offset, v => v.Value);

        var attributeIndex = 0;

        var xmlDoc = new XmlDocument();

#pragma warning disable CS0219 // The variable 'bugged' is assigned but its value is never used
        var bugged = false;
#pragma warning restore CS0219 // The variable 'bugged' is assigned but its value is never used

        var xmlMap = new Dictionary<int, XmlElement> { };

        foreach (var node in nodeTable)
        {
            XmlElement element = xmlDoc.CreateElement(dataMap[node.NodeNameOffset]);

            for (int i = 0, j = node.AttributeCount; i < j; i++)
            {
                if (dataMap.ContainsKey(attributeTable[attributeIndex].ValueOffset))
                    element.SetAttribute(dataMap[attributeTable[attributeIndex].NameOffset], dataMap[attributeTable[attributeIndex].ValueOffset]);
                else
                {
                    bugged = true;
                    element.SetAttribute(dataMap[attributeTable[attributeIndex].NameOffset], "BUGGED");
                }
                attributeIndex++;
            }

            xmlMap[node.NodeID] = element;
            if (xmlMap.ContainsKey(node.ParentNodeID))
                xmlMap[node.ParentNodeID].AppendChild(element);
            else
                xmlDoc.AppendChild(element);
        }

        return xmlDoc;
    }

    public static TObject Deserialize<TObject>(Stream inStream) where TObject : class
    {
        using MemoryStream ms = new();
        var xs = new XmlSerializer(typeof(TObject));
        var xmlDoc = ReadStream(inStream);

        xmlDoc.Save(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return xs.Deserialize(ms) as TObject;
    }

    public static TObject Deserialize<TObject>(string inFile) where TObject : class
    {
        using MemoryStream ms = new MemoryStream();
        var xmlDoc = CryXmlSerializer.ReadFile(inFile);

        xmlDoc.Save(ms);

        ms.Seek(0, SeekOrigin.Begin);

        XmlSerializer xs = new XmlSerializer(typeof(TObject));

        return xs.Deserialize(ms) as TObject;
    }
}