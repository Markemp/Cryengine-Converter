using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Directional
    {
        [XmlElement(ElementName = "color")]
        public ColladaColor Color;

    }
}

