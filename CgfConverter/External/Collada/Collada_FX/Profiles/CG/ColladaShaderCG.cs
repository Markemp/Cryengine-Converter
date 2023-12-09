using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaShaderCG : ColladaShader
{

    [XmlElement(ElementName = "bind_uniform")]
    public ColladaBindUniform[] Bind_Uniform;

    [XmlElement(ElementName = "compiler")]
    public ColladaCompiler[] Compiler;
}

