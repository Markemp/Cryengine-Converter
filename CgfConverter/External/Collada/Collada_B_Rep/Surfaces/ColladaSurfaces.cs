using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSurfaces
{
    [XmlElement(ElementName = "surface")]
    public ColladaSurface[] Surface;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

