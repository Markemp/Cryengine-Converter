using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Lighting;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Enums;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "fx_common_color_or_texture_type", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaFXCommonColorOrTextureType
    {
        [XmlAttribute("opaque")]
        [System.ComponentModel.DefaultValue(ColladaFXOpaqueChannel.A_ONE)]
        public ColladaFXOpaqueChannel Opaque;

        [XmlElement(ElementName = "param")]
        public ColladaParam Param;

        [XmlElement(ElementName = "color")]
        public ColladaColor Color;

        [XmlElement(ElementName = "texture")]
        public ColladaTexture Texture;
    }
}

