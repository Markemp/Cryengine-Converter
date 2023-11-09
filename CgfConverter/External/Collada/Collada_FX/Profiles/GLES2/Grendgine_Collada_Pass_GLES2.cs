using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Pass_GLES2 : Grendgine_Collada_Pass
    {

        [XmlElement(ElementName = "program")]
        public Grendgine_Collada_Program_GLES2 Program;
    }
}

