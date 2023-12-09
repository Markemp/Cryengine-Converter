using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "texcombiner", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTexCombiner
{

    [XmlElement(ElementName = "constant")]
    public ColladaConstantAttribute Constant;

    [XmlElement(ElementName = "RGB")]
    public ColladaRGB RGB;

    [XmlElement(ElementName = "alpha")]
    public ColladaAlpha Alpha;
}

