using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSweptSurface
{

    [XmlElement(ElementName = "curve")]
    public ColladaCurve Curve;

    [XmlElement(ElementName = "origin")]
    public ColladaOrigin Origin;

    [XmlElement(ElementName = "direction")]
    public ColladaFloatArrayString Direction;

    [XmlElement(ElementName = "axis")]
    public ColladaFloatArrayString Axis;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

