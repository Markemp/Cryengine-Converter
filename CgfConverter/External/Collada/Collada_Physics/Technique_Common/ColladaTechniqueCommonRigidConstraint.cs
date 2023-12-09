using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonRigidConstraint : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "enabled")]
    public ColladaSIDBool Enabled;

    [XmlElement(ElementName = "interpenetrate")]
    public ColladaSIDBool Interpenetrate;

    [XmlElement(ElementName = "limits")]
    public ColladaConstraintLimits Limits;

    [XmlElement(ElementName = "spring")]
    public ColladaConstraintSpring Spring;
}

