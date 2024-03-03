using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "texture", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaTexture
    {
        [XmlAttribute("texture")]
        public string Texture;

        [XmlAttribute("texcoord")]
        public string TexCoord;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

