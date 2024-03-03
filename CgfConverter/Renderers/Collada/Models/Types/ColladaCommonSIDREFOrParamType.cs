using System;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCommonSIDREFOrParamType : ColladaCommonParamType
{
    [XmlText()]
    public string Value;

}

