using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaShader
{
    [XmlAttribute("stage")]
    [System.ComponentModel.DefaultValue(ColladaShaderStage.VERTEX)]
    public ColladaShaderStage Stage;

    [XmlElement(ElementName = "sources")]
    public ColladaShaderSources Sources;



}

