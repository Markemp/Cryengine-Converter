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
        public ColladaTechnique[] Technique;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

