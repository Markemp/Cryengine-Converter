using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "plane", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPlane
{
    [XmlElement(ElementName = "equation")]
    public ColladaFloatArrayString Equation;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

