using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "stencil_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaStencilTarget
{
    [XmlAttribute("index")]
    [System.ComponentModel.DefaultValueAttribute(typeof(int), "1")]
    public int Index;

    [XmlAttribute("slice")]
    [System.ComponentModel.DefaultValueAttribute(typeof(int), "0")]
    public int Slice;

    [XmlAttribute("mip")]
    [System.ComponentModel.DefaultValueAttribute(typeof(int), "0")]
    public int Mip;


    [XmlAttribute("face")]
    [System.ComponentModel.DefaultValueAttribute(ColladaFace.POSITIVE_X)]
    public ColladaFace Face;


    [XmlElement(ElementName = "param")]
    public ColladaParam Param;


    [XmlElement(ElementName = "instance_image")]
    public ColladaInstanceImage Instance_Image;

}

