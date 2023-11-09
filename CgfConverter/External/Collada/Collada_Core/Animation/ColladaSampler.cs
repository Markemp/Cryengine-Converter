using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSampler
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("pre_behavior")]
    [System.ComponentModel.DefaultValueAttribute(ColladaSamplerBehavior.UNDEFINED)]
    public ColladaSamplerBehavior Pre_Behavior;

    [XmlAttribute("post_behavior")]
    [System.ComponentModel.DefaultValueAttribute(ColladaSamplerBehavior.UNDEFINED)]
    public ColladaSamplerBehavior Post_Behavior;


    [XmlElement(ElementName = "input")]
    public ColladaInputUnshared[] Input;
}

