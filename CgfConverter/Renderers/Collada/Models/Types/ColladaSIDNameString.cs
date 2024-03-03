using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSIDNameString
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlText()]
    public string Value;
}

