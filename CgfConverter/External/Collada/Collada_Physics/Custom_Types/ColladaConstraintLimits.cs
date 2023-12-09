using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "limits", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConstraintLimits
{
    [XmlElement(ElementName = "swing_cone_and_twist")]
    public ColladaConstraintLimitDetail Swing_Cone_And_Twist;

    [XmlElement(ElementName = "linear")]
    public ColladaConstraintLimitDetail Linear;
}
