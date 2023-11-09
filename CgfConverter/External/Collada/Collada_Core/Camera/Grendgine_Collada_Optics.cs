using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Optics
    {

        [XmlElement(ElementName = "technique_common")]
        public Grendgine_Collada_Technique_Common_Optics Technique_Common;

        [XmlElement(ElementName = "technique")]
        public Grendgine_Collada_Technique[] Technique;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;


    }
}

