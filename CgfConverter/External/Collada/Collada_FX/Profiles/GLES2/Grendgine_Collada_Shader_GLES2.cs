using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Shader_GLES2 : Grendgine_Collada_Shader
    {

        [XmlElement(ElementName = "compiler")]
        public Grendgine_Collada_Compiler[] Compiler;
        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

