using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Set_Param
    {
        [XmlAttribute("ref")]
        public string Ref;


        /// <summary>
        /// The element is the type and the element text is the value or space delimited list of values
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] Data;
    }
}

