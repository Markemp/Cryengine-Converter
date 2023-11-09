using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]

public partial class ColladaCylinderBRep
{
    [XmlElement(ElementName = "radius")]
    public ColladaFloatArrayString Radius;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

