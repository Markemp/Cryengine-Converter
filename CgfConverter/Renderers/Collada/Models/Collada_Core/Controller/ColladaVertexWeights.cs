using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Controller;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaVertexWeights
{
    [XmlAttribute("count")]
    public int Count;

    [XmlElement(ElementName = "input")]
    public ColladaInputShared[] Input;

    [XmlElement(ElementName = "vcount")]
    public ColladaIntArrayString VCount;

    [XmlElement(ElementName = "v")]
    public ColladaIntArrayString V;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

