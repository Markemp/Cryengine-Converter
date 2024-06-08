using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "blinn", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBlinn
{

    [XmlElement(ElementName = "emission")]
    public ColladaFXCommonColorOrTextureType Eission;

    [XmlElement(ElementName = "ambient")]
    public ColladaFXCommonColorOrTextureType Ambient;

    [XmlElement(ElementName = "diffuse")]
    public ColladaFXCommonColorOrTextureType Diffuse;

    [XmlElement(ElementName = "specular")]
    public ColladaFXCommonColorOrTextureType Specular;

    [XmlElement(ElementName = "transparent")]
    public ColladaFXCommonColorOrTextureType Transparent;

    [XmlElement(ElementName = "reflective")]
    public ColladaFXCommonColorOrTextureType Reflective;



    [XmlElement(ElementName = "shininess")]
    public ColladaFXCommonFloatOrParamType Shininess;

    [XmlElement(ElementName = "reflectivity")]
    public ColladaFXCommonFloatOrParamType Reflectivity;

    [XmlElement(ElementName = "transparency")]
    public ColladaFXCommonFloatOrParamType Transparency;

    [XmlElement(ElementName = "index_of_refraction")]
    public ColladaFXCommonFloatOrParamType Index_Of_Refraction;


}

