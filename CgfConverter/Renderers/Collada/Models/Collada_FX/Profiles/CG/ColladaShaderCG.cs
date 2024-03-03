using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.CG;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaShaderCG : ColladaShader
{

    [XmlElement(ElementName = "bind_uniform")]
    public ColladaBindUniform[] Bind_Uniform;

    [XmlElement(ElementName = "compiler")]
    public ColladaCompiler[] Compiler;
}

