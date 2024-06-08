using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "texcombiner", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTexCombiner
{

    [XmlElement(ElementName = "constant")]
    public ColladaConstantAttribute Constant;

    [XmlElement(ElementName = "RGB")]
    public ColladaRGB RGB;

    [XmlElement(ElementName = "alpha")]
    public ColladaAlpha Alpha;
}

