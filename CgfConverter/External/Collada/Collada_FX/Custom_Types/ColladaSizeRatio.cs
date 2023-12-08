using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "size_ratio", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSizeRatio
{
    [XmlAttribute("width")]
    public float Width;

    [XmlAttribute("height")]
    public float Height;
}

