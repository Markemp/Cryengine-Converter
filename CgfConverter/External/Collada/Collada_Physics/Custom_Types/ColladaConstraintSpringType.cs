using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConstraintSpringType
{
    [XmlElement(ElementName = "stiffness")]
    public ColladaSIDFloat Stiffness;

    [XmlElement(ElementName = "damping")]
    public ColladaSIDFloat Damping;

    [XmlElement(ElementName = "target_value")]
    public ColladaSIDFloat Target_Value;
}

