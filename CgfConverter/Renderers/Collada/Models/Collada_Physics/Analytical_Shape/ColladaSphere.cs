using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Analytical_Shape;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "sphere", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSphere
{
    [XmlElement(ElementName = "radius")]
    public float Radius;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

