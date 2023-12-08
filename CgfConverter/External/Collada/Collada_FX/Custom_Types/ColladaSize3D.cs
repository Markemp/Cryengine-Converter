using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "size", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSize3D
{
    [XmlAttribute("width")]
    public int Width;

    [XmlAttribute("height")]
    public int Height;

    [XmlAttribute("depth")]
    public int Depth;
}

