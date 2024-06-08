using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Parameters;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "array", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaArray
{
    [XmlAttribute("length")]
    public int Length;
    [XmlAttribute("resizable")]
    public bool Resizable;

    /// <summary>
    /// Need to determine the type and value of the Object(s)
    /// </summary>
    [XmlAnyElement]
    public XmlElement[] Data;
}

