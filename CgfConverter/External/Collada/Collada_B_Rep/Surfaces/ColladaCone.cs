using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCone
{
    [XmlElement(ElementName = "radius")]
    public float Radius;

    [XmlElement(ElementName = "angle")]
    public float Angle;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

