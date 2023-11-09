using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "texture", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

