using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "bind_attribute", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindAttribute
{
    [XmlAttribute("symbol")]
    public string Symbol;

    [XmlElement(ElementName = "semantic")]
    public ColladaSemantic Semantic;
}

