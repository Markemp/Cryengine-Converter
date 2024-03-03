using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.GLES2;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaShaderGLES2 : ColladaShader
{

    [XmlElement(ElementName = "compiler")]
    public ColladaCompiler[] Compiler;
    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

