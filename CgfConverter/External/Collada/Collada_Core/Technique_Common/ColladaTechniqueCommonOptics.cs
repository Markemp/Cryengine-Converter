using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonOptics : ColladaTechniqueCommon
{

    [XmlElement(ElementName = "orthographic")]
    public ColladaOrthographic Orthographic;

    [XmlElement(ElementName = "perspective")]
    public ColladaPerspective Perspective;
}

