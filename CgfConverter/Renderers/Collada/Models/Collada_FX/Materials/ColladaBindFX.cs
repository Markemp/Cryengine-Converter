using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Materials;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "bind", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindFX
{
    [XmlAttribute("semantic")]
    public string Semantic;

    [XmlAttribute("target")]
    public string Target;
}

