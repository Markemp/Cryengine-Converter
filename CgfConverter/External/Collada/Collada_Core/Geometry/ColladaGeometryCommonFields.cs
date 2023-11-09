using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaGeometryCommonFields
{
    [XmlAttribute("count")]
    public int Count;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("material")]
    public string Material;

    [XmlElement(ElementName = "input")]
    public ColladaInputShared[] Input;

    [XmlElement(ElementName = "p")]
    public ColladaIntArrayString P;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

}

