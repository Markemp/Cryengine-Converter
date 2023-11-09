using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_GLSL : Grendgine_Collada_Effect_Technique
    {

        [XmlElement(ElementName = "annotate")]
        public Grendgine_Collada_Annotate[] Annotate;

        [XmlElement(ElementName = "pass")]
        public Grendgine_Collada_Pass_GLSL[] Pass;
    }
}

