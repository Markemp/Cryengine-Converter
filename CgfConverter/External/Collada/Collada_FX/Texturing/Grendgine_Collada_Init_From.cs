using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "init_from", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Init_From
    {
        // Commented out parts are not recognized in Blender (and probably not part of Collada 1.4.1)
        //[XmlAttribute("mips_generate")]
        //public bool Mips_Generate;

        //[XmlAttribute("array_index")]
        //public int Array_Index;

        //[XmlAttribute("mip_index")]
        //public int Mip_Index;

        // Uri added to support 1.4.1 formats
        [XmlText()]
        public string Uri;

        //      [XmlAttribute("depth")]
        //public int Depth;

        [XmlAttribute("face")]
        [System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Face.POSITIVE_X)]
        public Grendgine_Collada_Face Face;

        [XmlElement(ElementName = "ref")]
        public string Ref;

        [XmlElement(ElementName = "hex")]
        public Grendgine_Collada_Hex Hex;
    }
}

