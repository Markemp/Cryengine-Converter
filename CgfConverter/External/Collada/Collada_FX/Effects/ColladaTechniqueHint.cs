using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "technique_hint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTechniqueHint
{
    [XmlAttribute("platform")]
    public string Platform;

    [XmlAttribute("ref")]
    public string Ref;

    [XmlAttribute("profile")]
    public string Profile;


}

