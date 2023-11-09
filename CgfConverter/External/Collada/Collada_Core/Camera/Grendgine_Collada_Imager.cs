using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Imager
    {


        [XmlElement(ElementName = "technique")]
        public Grendgine_Collada_Technique[] Technique;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

