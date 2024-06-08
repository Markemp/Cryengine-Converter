using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;


[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaConstantAttribute
{
    [XmlAttribute("value")]
    public string Value_As_String;
    //TODO: needs to be 4 float array

    [XmlAttribute("param")]
    public string Param_As_String;
    //TODO: needs to be 4 float array
}

