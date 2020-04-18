using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "depth_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Depth_Clear
    {
        [XmlAttribute("index")]
        [System.ComponentModel.DefaultValueAttribute(typeof(int), "0")]
        public int Index;

        [XmlTextAttribute()]
        public float Value;

    }
}

