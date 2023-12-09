using System;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Controller;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSkeleton
{
    [XmlText()]
    public string Value;
}

