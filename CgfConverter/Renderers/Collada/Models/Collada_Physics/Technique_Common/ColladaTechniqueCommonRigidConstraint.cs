using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Technique_Common;

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

