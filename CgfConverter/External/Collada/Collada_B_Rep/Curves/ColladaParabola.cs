using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaParabola
    {
        [XmlElement(ElementName = "focal")]
        public float Focal;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

