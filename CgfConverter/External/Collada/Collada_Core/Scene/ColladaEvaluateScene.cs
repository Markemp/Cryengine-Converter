using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaEvaluateScene
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("sid")]
    public string sid;

    [XmlAttribute("enable")]
    public bool Enable;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

    [XmlElement(ElementName = "render")]
    public Grendgine_Collada_Render[] Render;

}

