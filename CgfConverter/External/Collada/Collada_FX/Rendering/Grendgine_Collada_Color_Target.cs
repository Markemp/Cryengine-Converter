using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "color_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Color_Target
    {
        [XmlAttribute("index")]
        [System.ComponentModel.DefaultValueAttribute(typeof(int), "0")]
        public int Index;

        [XmlAttribute("slice")]
        [System.ComponentModel.DefaultValueAttribute(typeof(int), "0")]
        public int Slice;

        [XmlAttribute("mip")]
        [System.ComponentModel.DefaultValueAttribute(typeof(int), "0")]
        public int Mip;

        [XmlAttribute("face")]
        [System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Face.POSITIVE_X)]
        public Grendgine_Collada_Face Face;

        [XmlElement(ElementName = "param")]
        public Grendgine_Collada_Param Param;


        [XmlElement(ElementName = "instance_image")]
        public Grendgine_Collada_Instance_Image Instance_Image;

    }
}

