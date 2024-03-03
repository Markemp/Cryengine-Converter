using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "fx_sampler_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaFXSamplerCommon
{
    [XmlElement(ElementName = "texcoord")]
    public ColladaTexCoordSemantic TexCoord_Semantic;

    [XmlElement(ElementName = "wrap_s")]
    //[System.ComponentModel.DefaultValueAttribute(ColladaFXSamplerCommonWrapMode.WRAP)]		
    public ColladaFXSamplerCommonWrapMode Wrap_S;

    [XmlElement(ElementName = "wrap_t")]
    //[System.ComponentModel.DefaultValueAttribute(Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
    public ColladaFXSamplerCommonWrapMode Wrap_T;

    [XmlElement(ElementName = "wrap_p")]
    //[System.ComponentModel.DefaultValueAttribute(Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
    public ColladaFXSamplerCommonWrapMode Wrap_P;

    [XmlElement(ElementName = "minfilter")]
    //[System.ComponentModel.DefaultValueAttribute(Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
    public ColladaFXSamplerCommonFilterType MinFilter;

    [XmlElement(ElementName = "magfilter")]
    //[System.ComponentModel.DefaultValueAttribute(Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
    public ColladaFXSamplerCommonFilterType MagFilter;

    [XmlElement(ElementName = "mipfilter")]
    //[System.ComponentModel.DefaultValueAttribute(Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
    public ColladaFXSamplerCommonFilterType MipFilter;

    [XmlElement(ElementName = "border_color")]
    public ColladaFloatArrayString Border_Color;

    [XmlElement(ElementName = "mip_max_level")]
    public byte Mip_Max_Level;

    [XmlElement(ElementName = "mip_min_level")]
    public byte Mip_Min_Level;

    [XmlElement(ElementName = "mip_bias")]
    public float Mip_Bias;

    [XmlElement(ElementName = "max_anisotropy")]
    public int Max_Anisotropy;


    [XmlElement(ElementName = "instance_image")]
    public ColladaInstanceImage Instance_Image;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

