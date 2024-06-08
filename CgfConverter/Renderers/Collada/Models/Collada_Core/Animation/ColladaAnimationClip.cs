using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Mathematics;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Animation;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAnimationClip
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;


    [XmlAttribute("start")]
    public double Start;

    [XmlAttribute("end")]
    public double End;


    [XmlElement(ElementName = "instance_animation")]
    public ColladaInstanceAnimation[] Instance_Animation;

    [XmlElement(ElementName = "instance_formula")]
    public ColladaInstanceFormula[] Instance_Formula;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

