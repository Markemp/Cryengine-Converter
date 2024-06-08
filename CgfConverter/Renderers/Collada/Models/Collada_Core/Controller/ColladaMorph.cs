using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Controller;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaMorph
{
    [XmlAttribute("source")]
    public string Source_Attribute;

    [XmlAttribute("method")]
    public string Method;

    [XmlArray("targets")]
    public ColladaInputShared[] Targets;

    [XmlElement(ElementName = "source")]
    public ColladaSource[] Source;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

