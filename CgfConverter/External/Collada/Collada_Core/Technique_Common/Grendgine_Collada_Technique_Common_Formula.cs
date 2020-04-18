using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Formula : Grendgine_Collada_Technique_Common
    {
        /// <summary>
        /// Need to determine the type and value of the Object(s)
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] Data;

    }
}

