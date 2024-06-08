using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConstraintSpringType
{
    [XmlElement(ElementName = "stiffness")]
    public ColladaSIDFloat Stiffness;

    [XmlElement(ElementName = "damping")]
    public ColladaSIDFloat Damping;

    [XmlElement(ElementName = "target_value")]
    public ColladaSIDFloat Target_Value;
}

