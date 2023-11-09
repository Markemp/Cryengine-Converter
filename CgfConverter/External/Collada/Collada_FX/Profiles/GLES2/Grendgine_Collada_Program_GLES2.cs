using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Program_GLES2
    {

        [XmlElement(ElementName = "linker")]
        public Grendgine_Collada_Linker[] Linker;

        [XmlElement(ElementName = "shader")]
        public Grendgine_Collada_Shader_GLES2[] Shader;

        [XmlElement(ElementName = "bind_attribute")]
        public Grendgine_Collada_Bind_Attribute[] Bind_Attribute;

        [XmlElement(ElementName = "bind_uniform")]
        public Grendgine_Collada_Bind_Uniform[] Bind_Uniform;
    }
}

