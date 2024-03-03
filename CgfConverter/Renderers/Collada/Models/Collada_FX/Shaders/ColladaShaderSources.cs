using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "sources", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaShaderSources
{
    [XmlAttribute("entry")]
    public string Entry;

    [XmlElement(ElementName = "inline")]
    public string[] Inline;

    [XmlElement(ElementName = "import")]
    public ColladaRefString[] Import;


}

