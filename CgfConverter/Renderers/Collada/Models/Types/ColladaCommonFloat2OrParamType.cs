using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCommonFloat2OrParamType
{
    [XmlElement(ElementName = "param")]
    public ColladaParam Param;
    //TODO: cleanup to legit array

    [XmlText()]
    public string Value_As_String;
}



