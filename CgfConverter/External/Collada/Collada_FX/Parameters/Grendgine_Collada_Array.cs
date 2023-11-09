using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "array", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Array
    {
        [XmlAttribute("length")]
        public int Length;
        [XmlAttribute("resizable")]
        public bool Resizable;

        /// <summary>
        /// Need to determine the type and value of the Object(s)
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] Data;
    }
}

