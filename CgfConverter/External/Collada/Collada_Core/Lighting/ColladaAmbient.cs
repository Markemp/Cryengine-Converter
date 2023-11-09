using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAmbient
{
    [XmlElement(ElementName = "color")]
    public ColladaColor Color;

}

