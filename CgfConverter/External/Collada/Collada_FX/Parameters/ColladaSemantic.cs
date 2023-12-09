using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "semantic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSemantic
{
    [XmlText()]
    public string Value;
}

