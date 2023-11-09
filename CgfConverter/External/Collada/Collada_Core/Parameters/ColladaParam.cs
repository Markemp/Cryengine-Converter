using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaParam
{
    [XmlAttribute("ref")]
    public string Ref;

    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("semantic")]
    public string Semantic;

    [XmlAttribute("type")]
    public string Type;

    [XmlAnyElement]
    public XmlElement[] Data;

    //TODO: this is used in a few contexts
}

