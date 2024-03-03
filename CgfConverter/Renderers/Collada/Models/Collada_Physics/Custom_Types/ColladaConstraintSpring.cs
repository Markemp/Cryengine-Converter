using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "spring", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConstraintSpring
{
    [XmlElement(ElementName = "linear")]
    public ColladaConstraintSpringType Linear;

    [XmlElement(ElementName = "angular")]
    public ColladaConstraintSpringType Angular;
}
