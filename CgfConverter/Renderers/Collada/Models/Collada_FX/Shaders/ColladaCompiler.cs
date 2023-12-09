using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "compiler", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaCompiler
{
    [XmlAttribute("platform")]
    public string Platform;

    [XmlAttribute("target")]
    public string Target;

    [XmlAttribute("options")]
    public string Options;

    [XmlElement(ElementName = "binary")]
    public ColladaBinary Binary;
}

