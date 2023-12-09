using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCommonSIDREFOrParamType : ColladaCommonParamType
{
    [XmlText()]
    public string Value;

}

