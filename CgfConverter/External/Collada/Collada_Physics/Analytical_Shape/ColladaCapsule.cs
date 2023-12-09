using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "capsule", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaCapsule
{
    [XmlElement(ElementName = "height")]
    public float Height;

    [XmlElement(ElementName = "radius")]
    public ColladaFloatArrayString Radius;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

