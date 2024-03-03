using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "size_ratio", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSizeRatio
{
    [XmlAttribute("width")]
    public float Width;

    [XmlAttribute("height")]
    public float Height;
}

