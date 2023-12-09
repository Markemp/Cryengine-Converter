using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConstraintLimitDetail
{
    [XmlElement(ElementName = "min")]
    public ColladaSIDFloatArrayString Min;

    [XmlElement(ElementName = "max")]
    public ColladaSIDFloatArrayString Max;
}

