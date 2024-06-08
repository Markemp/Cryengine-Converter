using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Animation;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSampler
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("pre_behavior")]
    [System.ComponentModel.DefaultValue(ColladaSamplerBehavior.UNDEFINED)]
    public ColladaSamplerBehavior Pre_Behavior;

    [XmlAttribute("post_behavior")]
    [System.ComponentModel.DefaultValue(ColladaSamplerBehavior.UNDEFINED)]
    public ColladaSamplerBehavior Post_Behavior;


    [XmlElement(ElementName = "input")]
    public ColladaInputUnshared[] Input;
}

