using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "fx_common_color_or_texture_type", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaFXCommonColorOrTextureType
    {
        [XmlAttribute("opaque")]
        [System.ComponentModel.DefaultValueAttribute(ColladaFXOpaqueChannel.A_ONE)]
        public ColladaFXOpaqueChannel Opaque;

        [XmlElement(ElementName = "param")]
        public ColladaParam Param;

        [XmlElement(ElementName = "color")]
        public ColladaColor Color;

        [XmlElement(ElementName = "texture")]
        public ColladaTexture Texture;
    }
}

