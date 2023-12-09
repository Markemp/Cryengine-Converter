using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Surfaces;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSurfaces
{
    [XmlElement(ElementName = "surface")]
    public ColladaSurface[] Surface;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

