using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSIDIntArrayString
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlText()]
    public string Value_As_String;
}

