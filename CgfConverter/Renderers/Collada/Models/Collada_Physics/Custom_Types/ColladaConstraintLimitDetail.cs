using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConstraintLimitDetail
{
    [XmlElement(ElementName = "min")]
    public ColladaSIDFloatArrayString Min;

    [XmlElement(ElementName = "max")]
    public ColladaSIDFloatArrayString Max;
}

