using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaNewParam
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlElement(ElementName = "semantic")]
    public string Semantic;

    [XmlElement(ElementName = "modifier")]
    public string Modifier;

    [XmlElement("annotate")]
    public Grendgine_Collada_Annotate[] Annotate;

    /// <summary>
    /// The element is the type and the element text is the value or space delimited list of values
    /// </summary>
    [XmlAnyElement]
    public XmlElement[] Data;

    /// <summary>
    /// Surface is for 1.4.1 compatibility
    /// </summary>
    // ggerber 1.4.1 elements.  Surface and Sampler2D are single elements for textures.
    [XmlElement("surface")]
    public ColladaSurface Surface;

    /// <summary>
    /// Sampler2D is for 1.4.1 compatibility
    /// </summary>
    [XmlElement("sampler2D")]
    public ColladaSampler2D Sampler2D;
}

