using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Light : ColladaTechniqueCommon
    {
        [XmlElement(ElementName = "ambient")]
        public Grendgine_Collada_Ambient Ambient;

        [XmlElement(ElementName = "directional")]
        public Grendgine_Collada_Directional Directional;

        [XmlElement(ElementName = "point")]
        public Grendgine_Collada_Point Point;

        [XmlElement(ElementName = "spot")]
        public Grendgine_Collada_Spot Spot;






    }
}

