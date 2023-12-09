using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "bind_uniform", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBindUniform
{
    [XmlAttribute("symbol")]
    public string Symbol;

    [XmlElement(ElementName = "param")]
    public ColladaParam Param;

    /// <summary>
    /// The element is the type and the element text is the value or space delimited list of values
    /// </summary>
    [XmlAnyElement]
    public XmlElement[] Data;
}

