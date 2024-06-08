using System;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCommonIntOrParamType : ColladaCommonParamType
{
    [XmlText()]
    public string Value_As_String;

}

