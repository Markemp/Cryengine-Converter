using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCommonParamType
{
    [XmlElement(ElementName = "param")]
    public ColladaParam Param;
}

