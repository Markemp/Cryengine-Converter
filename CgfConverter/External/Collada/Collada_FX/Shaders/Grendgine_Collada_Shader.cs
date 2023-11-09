using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Shader
    {
        [XmlAttribute("stage")]
        [System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Shader_Stage.VERTEX)]
        public Grendgine_Collada_Shader_Stage Stage;

        [XmlElement(ElementName = "sources")]
        public Grendgine_Collada_Shader_Sources Sources;



    }
}

