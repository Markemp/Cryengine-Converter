using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "bind_attribute", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindAttribute
{
    [XmlAttribute("symbol")]
    public string Symbol;

    [XmlElement(ElementName = "semantic")]
    public ColladaSemantic Semantic;
}

