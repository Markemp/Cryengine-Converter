using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaOrthographic
{

    [XmlElement(ElementName = "xmag")]
    public ColladaSIDFloat XMag;

    [XmlElement(ElementName = "ymag")]
    public ColladaSIDFloat YMag;

    [XmlElement(ElementName = "aspect_ratio")]
    public ColladaSIDFloat Aspect_Ratio;

    [XmlElement(ElementName = "znear")]
    public ColladaSIDFloat ZNear;

    [XmlElement(ElementName = "zfar")]
    public ColladaSIDFloat ZFar;

}

