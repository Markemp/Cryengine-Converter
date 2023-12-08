using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "technique_override", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTechniqueOverride
{
    [XmlAttribute("ref")]
    public string Ref;
    [XmlAttribute("pass")]
    public string Pass;
}

