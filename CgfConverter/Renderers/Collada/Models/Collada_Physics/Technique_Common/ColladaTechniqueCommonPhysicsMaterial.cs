using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Technique_Common;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonPhysicsMaterial : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "dynamic_friction")]
    public ColladaSIDFloat Dynamic_Friction;

    [XmlElement(ElementName = "restitution")]
    public ColladaSIDFloat Restitution;

    [XmlElement(ElementName = "static_friction")]
    public ColladaSIDFloat Static_Friction;
}

