using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaProgramGLSL
{

    [XmlElement(ElementName = "shader")]
    public ColladaShaderGLSL[] Shader;

    [XmlElement(ElementName = "bind_attribute")]
    public ColladaBindAttribute[] Bind_Attribute;

    [XmlElement(ElementName = "bind_uniform")]
    public ColladaBindUniform[] Bind_Uniform;
}

