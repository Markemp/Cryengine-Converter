using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "limits", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaKinematicsLimits
{
    [XmlElement(ElementName = "min")]
    public ColladaSIDNameFloat Min;
    [XmlElement(ElementName = "max")]
    public ColladaSIDNameFloat Max;
}

