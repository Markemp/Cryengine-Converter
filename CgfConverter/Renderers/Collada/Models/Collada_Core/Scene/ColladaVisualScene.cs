using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaVisualScene
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

    [XmlElement(ElementName = "evaluate_scene")]
    public ColladaEvaluateScene[] Evaluate_Scene;


    [XmlElement(ElementName = "node")]
    public ColladaNode[] Node;

}

