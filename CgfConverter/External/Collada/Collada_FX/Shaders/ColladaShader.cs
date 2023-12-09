using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaShader
{
    [XmlAttribute("stage")]
    [System.ComponentModel.DefaultValueAttribute(ColladaShaderStage.VERTEX)]
    public ColladaShaderStage Stage;

    [XmlElement(ElementName = "sources")]
    public ColladaShaderSources Sources;



}

