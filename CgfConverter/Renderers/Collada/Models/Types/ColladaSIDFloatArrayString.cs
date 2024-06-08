using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSIDFloatArrayString
{
    [XmlAttribute("sid")]
    public string sID;

    //TODO: cleanup to legit array

    [XmlText()]
    public string Value_As_String;
}

