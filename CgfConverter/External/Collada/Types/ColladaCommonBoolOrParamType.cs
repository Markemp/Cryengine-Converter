using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCommonBoolOrParamType : ColladaCommonParamType
{
    [XmlElement(ElementName = "bool")]
    public bool Bool;
}

