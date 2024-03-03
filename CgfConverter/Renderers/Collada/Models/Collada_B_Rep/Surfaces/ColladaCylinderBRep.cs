using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Surfaces;

[Serializable]
[XmlType(AnonymousType = true)]

public partial class ColladaCylinderBRep
{
    [XmlElement(ElementName = "radius")]
    public ColladaFloatArrayString Radius;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

