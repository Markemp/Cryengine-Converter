using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSIDBool
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlText()]
    public bool Value;
}

