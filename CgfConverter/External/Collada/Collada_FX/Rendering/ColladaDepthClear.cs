using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "depth_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaDepthClear
{
    [XmlAttribute("index")]
    [System.ComponentModel.DefaultValueAttribute(typeof(int), "0")]
    public int Index;

    [XmlText()]
    public float Value;

}

