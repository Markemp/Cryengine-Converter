using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "sources", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaShaderSources
{
    [XmlAttribute("entry")]
    public string Entry;

    [XmlElement(ElementName = "inline")]
    public string[] Inline;

    [XmlElement(ElementName = "import")]
    public ColladaRefString[] Import;


}

