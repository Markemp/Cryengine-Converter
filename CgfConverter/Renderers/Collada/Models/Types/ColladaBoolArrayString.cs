using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]

public partial class ColladaBoolArrayString
{

    //TODO: cleanup to legit array
    [XmlText()]
    public string Value_As_String;
}


