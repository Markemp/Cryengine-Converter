using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "create_3d", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Create_3D
    {

        [XmlElement(ElementName = "size")]
        public ColladaSize3D Size;

        [XmlElement(ElementName = "mips")]
        public ColladaMipsAttribute Mips;


        [XmlElement(ElementName = "array")]
        public ColladaArrayLength Array_Length;



        [XmlElement(ElementName = "format")]
        public Grendgine_Collada_Format Format;

        [XmlElement(ElementName = "init_from")]
        public ColladaInitFrom[] Init_From;

    }
}

