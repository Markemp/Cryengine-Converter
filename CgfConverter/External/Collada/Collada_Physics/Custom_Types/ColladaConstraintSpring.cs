using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "spring", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConstraintSpring
{
    [XmlElement(ElementName = "linear")]
    public ColladaConstraintSpringType Linear;

    [XmlElement(ElementName = "angular")]
    public ColladaConstraintSpringType Angular;
}
