using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "annotate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaAnnotate
{
    [XmlAttribute("name")]
    public string Name;

    /// <summary>
    /// Need to determine the type and value of the Object(s)
    /// </summary>
    [XmlAnyElement]
    public XmlElement[] Data;

}

