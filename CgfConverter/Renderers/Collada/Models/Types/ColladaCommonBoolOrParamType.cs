using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCommonBoolOrParamType : ColladaCommonParamType
{
    [XmlElement(ElementName = "bool")]
    public bool Bool;
}

