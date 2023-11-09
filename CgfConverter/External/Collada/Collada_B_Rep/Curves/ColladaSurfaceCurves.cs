using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSurfaceCurves
{
    [XmlElement(ElementName = "curve")]
    public ColladaCurve[] Curve;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

