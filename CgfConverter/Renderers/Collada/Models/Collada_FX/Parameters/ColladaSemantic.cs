using System;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Parameters;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "semantic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSemantic
{
    [XmlText()]
    public string Value;
}

