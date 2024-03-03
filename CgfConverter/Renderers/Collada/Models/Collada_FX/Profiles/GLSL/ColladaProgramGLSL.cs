using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.GLSL;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaProgramGLSL
{

    [XmlElement(ElementName = "shader")]
    public ColladaShaderGLSL[] Shader;

    [XmlElement(ElementName = "bind_attribute")]
    public ColladaBindAttribute[] Bind_Attribute;

    [XmlElement(ElementName = "bind_uniform")]
    public ColladaBindUniform[] Bind_Uniform;
}

