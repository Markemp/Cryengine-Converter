using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;
using CgfConverter.Renderers.Collada.Collada.Enums;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "stencil_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaStencilTarget
{
    [XmlAttribute("index")]
    [System.ComponentModel.DefaultValue(typeof(int), "1")]
    public int Index;

    [XmlAttribute("slice")]
    [System.ComponentModel.DefaultValue(typeof(int), "0")]
    public int Slice;

    [XmlAttribute("mip")]
    [System.ComponentModel.DefaultValue(typeof(int), "0")]
    public int Mip;


    [XmlAttribute("face")]
    [System.ComponentModel.DefaultValue(ColladaFace.POSITIVE_X)]
    public ColladaFace Face;


    [XmlElement(ElementName = "param")]
    public ColladaParam Param;


    [XmlElement(ElementName = "instance_image")]
    public ColladaInstanceImage Instance_Image;

}

