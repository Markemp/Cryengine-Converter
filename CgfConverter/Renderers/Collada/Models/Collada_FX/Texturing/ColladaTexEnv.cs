using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "texenv", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTexEnv
{
    [XmlAttribute("operator")]
    public ColladaTexEnvOperator Operator;

    [XmlAttribute("sampler")]
    public string Sampler;

    [XmlElement(ElementName = "constant")]
    public ColladaConstantAttribute Constant;
}

