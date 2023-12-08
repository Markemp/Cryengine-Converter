using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Pass
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlElement(ElementName = "annotate")]
        public ColladaAnnotate[] Annotate;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;

        [XmlElement(ElementName = "states")]
        public Grendgine_Collada_States States;

        [XmlElement(ElementName = "evaluate")]
        public Grendgine_Collada_Effect_Technique_Evaluate Evaluate;
    }
}

