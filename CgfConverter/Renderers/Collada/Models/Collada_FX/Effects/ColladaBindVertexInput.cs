using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Effects;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "bind_vertex_input", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindVertexInput
{
    [XmlAttribute("semantic")]
    public string Semantic;

    [XmlAttribute("imput_semantic")]
    public string Imput_Semantic;

    [XmlAttribute("input_set")]
    public int Input_Set;

}

