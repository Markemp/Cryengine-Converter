using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "limits", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConstraintLimits
{
    [XmlElement(ElementName = "swing_cone_and_twist")]
    public ColladaConstraintLimitDetail Swing_Cone_And_Twist;

    [XmlElement(ElementName = "linear")]
    public ColladaConstraintLimitDetail Linear;
}
