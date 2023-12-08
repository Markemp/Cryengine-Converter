using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "format", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Format
    {
        [XmlElement(ElementName = "hint")]
        public ColladaFormatHint Hint;

        [XmlElement(ElementName = "exact")]
        public string Exact;
    }
}

