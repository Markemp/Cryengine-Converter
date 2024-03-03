using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "size", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSize3D
{
    [XmlAttribute("width")]
    public int Width;

    [XmlAttribute("height")]
    public int Height;

    [XmlAttribute("depth")]
    public int Depth;
}

