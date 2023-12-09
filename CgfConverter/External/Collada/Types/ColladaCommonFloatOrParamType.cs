using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCommonFloatOrParamType : ColladaCommonParamType
{
    [XmlText()]
    public string Value_As_String;
}

